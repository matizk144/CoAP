using CoAPnet.CoapMessageLevelClient;
using CoAPnet.Message;
using CoAPnet.Protocol;
using CoAPnet.Protocol.BlockTransfer;
using CoAPnet.Protocol.Options;
using Microsoft.Extensions.Options;

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

    public override async Task<CoapResponse> RequestAsync(CoapRequest request, CancellationToken cancellationToken)
    {
        if (IsDisposed) throw new ObjectDisposedException(GetType().ToString());

        await _requestSemaphore.WaitAsync(cancellationToken);

        BlockwisePayloadSize requestBlockSize = _options?.RequestPayloadSize ?? BlockwisePayloadSize._1024;
        BlockwisePayloadSize responseBlockSize = _options?.ResponsePayloadSize ?? BlockwisePayloadSize._1024;

        byte[] currentPayload = request.Payload.Count > 0 ? [.. request.Payload.ToArray()] : [];
        byte[] responseByte = [];
        ushort requestFrameNumber = 0;
        ushort responseFrameNumber = 0;
        bool hasResponseMoreData = true;
        CoapResponse? blockCoapResponse = null;
        while (currentPayload.Length > 0)
        {
            bool hasMoreData = false;
            byte[] blockPayload;
            if (currentPayload.Length > (int)requestBlockSize)
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

            var block1Option = new CoapBlockTransferOptionValue
            {
                Number = requestFrameNumber++,
                HasFollowingBlocks = hasMoreData,
                Size = (ushort)requestBlockSize
            };

            CoapBlockTransferOptionValue? block2Option = null;

            if (!block1Option.HasFollowingBlocks)
            {
                block2Option = new CoapBlockTransferOptionValue()
                {
                    Number = responseFrameNumber++,
                    HasFollowingBlocks = false,
                    Size = (ushort)responseBlockSize
                };
            }

            var blockwiseOptions = GetBlockwiseOptions(block1Option, block2Option);

            var blockCoapRequest = new CoapRequest()
            {
                Method = request.Method,
                Options = request.Options,
                Payload = blockPayload
            };

            var requestMessage = _requestToMessageConverter.Convert(blockCoapRequest, blockwiseOptions);

            var responseCoapMessage = await Send(requestMessage, cancellationToken);
            UpdateBlockWiseMessageOptions(responseCoapMessage.Options, ref requestBlockSize, ref responseBlockSize);
            hasResponseMoreData = GetHasResponseMoreData(responseCoapMessage.Options);

            if (responseCoapMessage.Payload.Count > 0)
            {
                responseByte = [.. responseByte, .. responseCoapMessage.Payload.ToArray()];
            }

            blockCoapResponse = _messageToResponseConverter.Convert(responseCoapMessage, responseCoapMessage.Payload);
        }

        while (hasResponseMoreData)
        {
            var block2Option = new CoapBlockTransferOptionValue
            {
                Number = responseFrameNumber++,
                Size = (ushort)responseBlockSize
            };

            var blockwiseOptions = GetBlockwiseOptions(block2Option: block2Option);

            var blockCoapRequest = new CoapRequest()
            {
                Method = request.Method,
                Options = request.Options,
            };

            var requestMessage = _requestToMessageConverter.Convert(blockCoapRequest, blockwiseOptions);

            var responseCoapMessage = await Send(requestMessage, cancellationToken);
            UpdateBlockWiseMessageOptions(responseCoapMessage.Options, ref requestBlockSize, ref responseBlockSize);
            hasResponseMoreData = GetHasResponseMoreData(responseCoapMessage.Options);

            if (responseCoapMessage.Payload.Count > 0)
            {
                responseByte = [.. responseByte, .. responseCoapMessage.Payload.ToArray()];
            }

            blockCoapResponse = _messageToResponseConverter.Convert(responseCoapMessage, responseCoapMessage.Payload);
        }

        var coapRespone = blockCoapResponse! with { Payload = responseByte };

        _requestSemaphore.Release();

        return coapRespone;

        //try
        //{
        //    var response = await Send(requestMessage, cancellationToken);
        //    var payload = response!.Payload;
        //    return _messageToResponseConverter.Convert(response, payload);
        //}
        //finally
        //{
        //    _requestSemaphore.Release();
        //}
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

    private void UpdateBlockWiseMessageOptions(IReadOnlyCollection<CoapMessageOption> options, ref BlockwisePayloadSize block1Size, ref BlockwisePayloadSize block2Size)
    {
        var block1Option = options.SingleOrDefault(op => op.Number == CoapMessageOptionNumber.Block1);
        if (block1Option != null)
        {
            block1Size = CoapBlockTransferOptionValueDecoder
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

        var block2Option = options.SingleOrDefault(op => op.Number == CoapMessageOptionNumber.Block2);
        if (block2Option != null)
        {
            block2Size = CoapBlockTransferOptionValueDecoder
                    .Decode(((CoapMessageOptionUintValue)block2Option.Value).Value).Size switch
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