using System.Net.Sockets;

namespace CoAPnet.Transport;

internal sealed class UdpCoapTransportLayer : ICoapTransportLayer
{
    CoapTransportLayerConnectOptions? _connectOptions;
    UdpClient? _udpClient;

    public Task ConnectAsync(CoapTransportLayerConnectOptions options, CancellationToken cancellationToken)
    {
        _connectOptions = options ?? throw new ArgumentNullException(nameof(options));

        Dispose();

        _udpClient = new UdpClient(0, options.EndPoint.AddressFamily);

        return Task.CompletedTask;
    }

    public async Task<int> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
        ThrowIfNotConnected();
        if (buffer.Array == null) throw new InvalidOperationException("Buffer for received data is not initialized");
        var receiveResult = await _udpClient!.ReceiveAsync(cancellationToken).ConfigureAwait(false);

        Array.Copy(receiveResult.Buffer, 0, buffer.Array, buffer.Offset, receiveResult.Buffer.Length);

        return receiveResult.Buffer.Length;
    }

    public Task SendAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
        ThrowIfNotConnected();

        return _udpClient!.SendAsync(buffer.Array ?? [], buffer.Count, _connectOptions?.EndPoint);
    }

    public void Dispose()
    {
        _udpClient?.Dispose();
    }

    private void ThrowIfNotConnected()
    {
        if (_udpClient == null)
        {
            throw new InvalidOperationException("The CoAP transport layer is not connected.");
        }
    }
}