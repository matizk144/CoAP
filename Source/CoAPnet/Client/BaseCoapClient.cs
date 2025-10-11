using CoAPnet.CoapMessageLevelClient;
using CoAPnet.Message;
using CoAPnet.Protocol;

namespace CoAPnet.Client;

internal abstract class BaseCoapClient(ICoapMessageLevelClient coapMessageLevelClient) : ICoapClient
{
    protected readonly CoapMessageIdProvider CoapMessageIdProvider = new();
    private bool _isDisposed;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;
        _isDisposed = true;
    }
    public Task ConnectAsync(CoapClientConnectOptions options, CancellationToken cancellationToken) =>
        coapMessageLevelClient.ConnectAsync(options, cancellationToken);

    public abstract Task<CoapResponse> RequestAsync(CoapRequest request, CancellationToken cancellationToken);

    //public Task<CoapObserveResponse> ObserveAsync(CoapObserveOptions options, CancellationToken cancellationToken) =>
    //    throw new NotSupportedException();

    //public Task StopObservationAsync(CoapObserveResponse observeResponse, CancellationToken cancellationToken) =>
    //    throw new NotSupportedException();

    protected Task<CoapMessage> Send(CoapMessage coapMessage, CancellationToken cancellationToken) =>
        coapMessageLevelClient.Send(coapMessage, cancellationToken);

    protected bool IsDisposed => _isDisposed;
}