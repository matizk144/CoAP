using CoAPnet.Client.Options;
using CoAPnet.CoapMessageLevelClient;
using CoAPnet.Exceptions;
using CoAPnet.Message;
using CoAPnet.Protocol;
using CoAPnet.Protocol.BlockTransfer;
using CoAPnet.Protocol.Options;

namespace CoAPnet.Client;

internal sealed class BlockwiseSequentialCoapClient : BaseCoapClient
{
    private readonly SemaphoreSlim _requestSemaphore = new(1);
    private readonly CoapRequestToMessageConverter _requestToMessageConverter;
    private readonly CoapMessageToResponseConverter _messageToResponseConverter = new();
    readonly BlockwiseMessageOptions? _options;

    public BlockwiseSequentialCoapClient(ICoapMessageLevelClient coapMessageLevelClient, BlockwiseMessageOptions? options = null)
    : base(coapMessageLevelClient)
    {
        _options = options;
        _requestToMessageConverter = new(CoapMessageIdProvider);
    }

    public override async Task<CoapResponse> RequestAsync(CoapRequest request, CancellationToken cancellationToken, Action<IRequestOptions>? options = null)
    {
        if (IsDisposed) throw new ObjectDisposedException(GetType().ToString());

        var requestOptions = new RequestOptions();
        options?.Invoke(requestOptions);

        await _requestSemaphore.WaitAsync(cancellationToken);

        try
        {
            CoapMessage? lastCoapMessage = null;
            if (request.Payload.Count > 0)
            {
                lastCoapMessage = await SendRequest(request, requestOptions, cancellationToken);
            }
            var response = await GetResponse(request, requestOptions, lastCoapMessage, cancellationToken);

            return _messageToResponseConverter.Convert(response.LastCoapMessage, response.Bytes);
        }
        finally
        {
            _requestSemaphore.Release();
        }
    }

    private async Task<CoapMessage> SendRequest(CoapRequest request, RequestOptions requestOptions, CancellationToken cancellationToken)
    {
        if (request.Payload.Count <= 0)
        {
            throw new ArgumentException("Request payload is empty", nameof(request));
        }

        BlockwisePayloadSize? requestBlockSize = _options?.RequestPayloadSize;

        byte[] currentPayload = [.. request.Payload.ToArray()];
        int bytesAlreadySend = 0;
        CoapMessage? responseCoapMessage = null;

        while (currentPayload.Length > 0)
        {
            bool hasMoreData = false;
            byte[] blockPayload;
            CoapBlockTransferOptionValue? block1Option = null;
            CoapBlockTransferOptionValue? block2Option = null;

            if (requestBlockSize != null && currentPayload.Length > (int)requestBlockSize)
            {
                hasMoreData = true;
                blockPayload = currentPayload[..(int)requestBlockSize];
                currentPayload = currentPayload[(int)requestBlockSize..];
            }
            else
            {
                blockPayload = [.. currentPayload];
                currentPayload = [];
            }

            if (requestBlockSize != null)
            {
                block1Option = new CoapBlockTransferOptionValue
                {
                    Number = CalculateBlockNum(bytesAlreadySend, requestBlockSize.Value),
                    HasFollowingBlocks = hasMoreData,
                    Size = (ushort)requestBlockSize
                };
            }

            if (!hasMoreData && _options?.RequestPayloadSize != null)
            {
                block2Option = new CoapBlockTransferOptionValue()
                {
                    Number = 0,
                    HasFollowingBlocks = false,
                    Size = (ushort)(_options?.RequestPayloadSize!.Value!)
                };
            }

            var blockwiseOptions = GetBlockwiseOptions(block1Option, block2Option);

            var blockCoapRequest = new CoapRequest()
            {
                Method = request.Method,
                Options = request.Options,
                Payload = blockPayload
            };

            responseCoapMessage = await SendAndHandleResponse(blockCoapRequest, blockwiseOptions, requestOptions, cancellationToken);
            bytesAlreadySend += blockPayload.Length;
            requestBlockSize = GetBlock1Size(responseCoapMessage.Options);
        }

        return responseCoapMessage!;
    }

    private async Task<(byte[] Bytes, CoapMessage LastCoapMessage)> GetResponse(CoapRequest generalCoapRequest, RequestOptions requestOptions, CoapMessage? lastCoapMessage, CancellationToken cancellationToken)
    {
        bool hasMoreData;
        BlockwisePayloadSize? responseBlockSize;
        byte[] responseBytes;
        CoapMessage? lastResponseCoapMessage = null;

        if (lastCoapMessage != null)
        {
            hasMoreData = GetHasResponseMoreData(lastCoapMessage.Options);
            responseBlockSize = GetBlock2Size(lastCoapMessage.Options);
            responseBytes = [.. lastCoapMessage.Payload.ToArray()];
            lastResponseCoapMessage = lastCoapMessage;
        }
        else
        {
            hasMoreData = true;
            responseBlockSize = _options?.ResponsePayloadSize;
            responseBytes = [];
        }

        int bytesAlreadyReceived = responseBytes.Length;

        while (hasMoreData)
        {
            CoapBlockTransferOptionValue? block2Option = null;
            if (responseBlockSize != null)
            {
                block2Option = new CoapBlockTransferOptionValue
                {
                    Number = CalculateBlockNum(bytesAlreadyReceived, responseBlockSize.Value),
                    Size = (ushort)responseBlockSize
                };
            }

            var blockwiseOptions = GetBlockwiseOptions(block2Option: block2Option);

            var blockCoapRequest = new CoapRequest()
            {
                Method = generalCoapRequest.Method,
                Options = generalCoapRequest.Options,
            };

            lastResponseCoapMessage = await SendAndHandleResponse(blockCoapRequest, blockwiseOptions, requestOptions, cancellationToken);
            responseBytes = [.. responseBytes, .. lastResponseCoapMessage.Payload.ToArray()];
            bytesAlreadyReceived = responseBytes.Length;
            responseBlockSize = GetBlock2Size(lastResponseCoapMessage.Options);
            hasMoreData = GetHasResponseMoreData(lastResponseCoapMessage.Options);
        }

        return (responseBytes, lastResponseCoapMessage!);
    }

    private async Task<CoapMessage> SendAndHandleResponse(CoapRequest blockCoapRequest, IReadOnlyCollection<CoapMessageOption> blockwiseOptions, RequestOptions requestOptions, CancellationToken cancellationToken)
    {
        CoapMessage responseCoapMessage;
        while (true)
        {
            var requestMessage = _requestToMessageConverter.Convert(blockCoapRequest, blockwiseOptions);
            responseCoapMessage = await Send(requestMessage, cancellationToken);

            if (requestOptions?.BitwiseHandler != null)
            {
                CoapResponse response = _messageToResponseConverter.Convert(responseCoapMessage, responseCoapMessage.Payload);
                var handleStatus = requestOptions.BitwiseHandler.Invoke(response.StatusCode);

                switch (handleStatus)
                {
                    default:
                    case CoapResponseBitwiseHandler.Continue:
                        break;
                    case CoapResponseBitwiseHandler.Repeat:
                        continue;
                    case CoapResponseBitwiseHandler.Interrupt:
                        throw new BitwiseCoapCommunicationInterruptedException(response);
                }
            }

            break;
        }

        return responseCoapMessage;
    }


    private ushort CalculateBlockNum(int bytesAlreadySendReceived, BlockwisePayloadSize blockSize)
    {
        return (ushort)(bytesAlreadySendReceived / (int)blockSize);
    }

    private IReadOnlyCollection<CoapMessageOption> GetBlockwiseOptions(CoapBlockTransferOptionValue? block1Option = null, CoapBlockTransferOptionValue? block2Option = null)
    {
        List<CoapMessageOption> blockwiseOptions = [];
        if (block1Option != null)
        {
            blockwiseOptions.Add(
                new CoapMessageOption(CoapMessageOptionNumber.Block1,
                    new CoapMessageOptionUintValue(CoapBlockTransferOptionValueEncoder.Encode(block1Option))));
        }

        if (block2Option != null)
        {
            blockwiseOptions.Add(
                new CoapMessageOption(CoapMessageOptionNumber.Block2,
                    new CoapMessageOptionUintValue(CoapBlockTransferOptionValueEncoder.Encode(block2Option))));
        }

        return blockwiseOptions;
    }

    private BlockwisePayloadSize? GetBlock1Size (IReadOnlyCollection<CoapMessageOption> options)
    {
        var block1Option = options.SingleOrDefault(op => op.Number == CoapMessageOptionNumber.Block1);

        if (block1Option == null) return null;

        return CoapBlockTransferOptionValueDecoder
                .Decode(((CoapMessageOptionUintValue)block1Option.Value).Value).Size switch
            {
                16 => BlockwisePayloadSize._16,
                32 => BlockwisePayloadSize._32,
                64 => BlockwisePayloadSize._64,
                128 => BlockwisePayloadSize._128,
                256 => BlockwisePayloadSize._256,
                512 => BlockwisePayloadSize._512,
                1024 => BlockwisePayloadSize._1024,
                _ => throw new ArgumentOutOfRangeException()
            };
    }

    private BlockwisePayloadSize? GetBlock2Size(IReadOnlyCollection<CoapMessageOption> options)
    {
        var block1Option = options.SingleOrDefault(op => op.Number == CoapMessageOptionNumber.Block2);

        if (block1Option == null) return null;

        return CoapBlockTransferOptionValueDecoder
                .Decode(((CoapMessageOptionUintValue)block1Option.Value).Value).Size switch
            {
                16 => BlockwisePayloadSize._16,
                32 => BlockwisePayloadSize._32,
                64 => BlockwisePayloadSize._64,
                128 => BlockwisePayloadSize._128,
                256 => BlockwisePayloadSize._256,
                512 => BlockwisePayloadSize._512,
                1024 => BlockwisePayloadSize._1024,
                _ => throw new ArgumentOutOfRangeException()
            };
    }

    private bool GetHasResponseMoreData(IReadOnlyCollection<CoapMessageOption> options)
    {
        var block2Option = options.SingleOrDefault(op => op.Number == CoapMessageOptionNumber.Block2);

        if (block2Option == null) return false;

        var block2 = CoapBlockTransferOptionValueDecoder.Decode(((CoapMessageOptionUintValue)block2Option.Value).Value);

        return block2.HasFollowingBlocks;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _requestSemaphore.Dispose();
        }

        base.Dispose(disposing);
    }
}