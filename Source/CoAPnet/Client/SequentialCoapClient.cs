using CoAPnet.CoapMessageLevelClient;
using CoAPnet.Message;

namespace CoAPnet.Client;

internal sealed class SequentialCoapClient : BaseCoapClient
{
    private readonly SemaphoreSlim _requestSemaphore = new(1);
    private readonly CoapRequestMessageCreator _requestMessageCreator;
    private readonly CoapMessageToResponseConverter _messageToResponseConverter = new();

    public SequentialCoapClient(ICoapMessageLevelClient coapMessageLevelClient)
        : base(coapMessageLevelClient)
    {
        _requestMessageCreator = new(CoapMessageIdProvider);
    }

    public override async Task<CoapResponse> RequestAsync(CoapRequest request, CancellationToken cancellationToken)
    {
        if (IsDisposed) throw new ObjectDisposedException(GetType().ToString());

        await _requestSemaphore.WaitAsync(cancellationToken);

        var requestMessage = _requestMessageCreator.Convert(request);
        try
        {
            var response = await Send(requestMessage, cancellationToken);
            var payload = response!.Payload;
            return _messageToResponseConverter.Convert(response, payload);
        }
        finally
        {
            _requestSemaphore.Release();
        }
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