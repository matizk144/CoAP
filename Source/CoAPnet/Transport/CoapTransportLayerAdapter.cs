using CoAPnet.Exceptions;
using Microsoft.Extensions.Logging;
namespace CoAPnet.Transport;

internal sealed class CoapTransportLayerAdapter(ICoapTransportLayer transportLayer, ILogger logger, string endpointId) : IDisposable
{
    public async Task ConnectAsync(CoapTransportLayerConnectOptions? connectOptions, CancellationToken cancellationToken)
    {
        if (connectOptions == null)
        {
            throw new ArgumentNullException(nameof(connectOptions));
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            logger.LogInformation("{EndpointId}: {Module}: Connecting...", endpointId, nameof(CoapTransportLayerAdapter));
            await transportLayer.ConnectAsync(connectOptions, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new CoapCommunicationException("Error while connecting with CoAP server.", exception);
        }
    }

    public async Task SendAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogTrace("{EndpointId}: {Module}: Sending {BytesCount} bytes...", endpointId, nameof(CoapTransportLayerAdapter), buffer.Count);
            await transportLayer.SendAsync(buffer, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            throw new CoapCommunicationException("Error while sending CoAP message.", exception);
        }
    }

    public async Task<int> ReceiveAsync(ArraySegment<byte> receiveBuffer, CancellationToken cancellationToken)
    {
        try
        {
            var receivedBytes = await transportLayer.ReceiveAsync(receiveBuffer, cancellationToken).ConfigureAwait(false);

            logger.LogTrace("{EndpointId}: {Module}: Received {BytesCount} bytes...", endpointId, nameof(CoapTransportLayerAdapter), receivedBytes);

            return receivedBytes;
        }
        catch (Exception exception)
        {
            throw new CoapCommunicationException("Error receiving CoAP messages.", exception);
        }
    }

    public void Dispose()
    {
        transportLayer.Dispose();
    }
}
