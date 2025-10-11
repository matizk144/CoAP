using CoAPnet.Client;
using CoAPnet.LowLevelClient;
using CoAPnet.Protocol;
using Microsoft.Extensions.Logging;

namespace CoAPnet.CoapMessageLevelClient;

internal class RawCoapMessageLevelClient(ILogger logger) : ICoapMessageLevelClient
{
    private readonly LowLevelCoapClient _lowLevelCoapClient = new(logger);
    private bool _isDisposed;

    public Task ConnectAsync(CoapClientConnectOptions options, CancellationToken cancellationToken)
    {
        EndpointId = options.EndpointId;
        return _lowLevelCoapClient.ConnectAsync(options, cancellationToken);
    }

    public virtual async Task<CoapMessage> Send(CoapMessage coapMessage, CancellationToken cancellationToken)
    {
        await _lowLevelCoapClient.SendAsync(coapMessage, cancellationToken);
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var responseMessage = await _lowLevelCoapClient.ReceiveAsync(cancellationToken);
            if (responseMessage == null)
            {
                await Task.Yield();
                continue;
            }

            return responseMessage;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;
        _isDisposed = true;

        if (disposing)
        {
            _lowLevelCoapClient.Dispose();
        }
    }

    protected ILogger Logger => logger;

    protected string? EndpointId { get; private set; }
}