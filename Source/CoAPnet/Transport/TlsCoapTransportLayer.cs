namespace CoAPnet.Transport;

internal sealed class TlsCoapTransportLayer : ICoapTransportLayer
{
    public Task ConnectAsync(CoapTransportLayerConnectOptions connectOptions, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Task<int> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task SendAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
