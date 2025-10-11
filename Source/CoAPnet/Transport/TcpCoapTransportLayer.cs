using System.Net.Sockets;

namespace CoAPnet.Transport;

internal sealed class TcpCoapTransportLayer : ICoapTransportLayer
{
    TcpClient? _tcpClient;
    NetworkStream? _networkStream;

    public async Task ConnectAsync(CoapTransportLayerConnectOptions options, CancellationToken cancellationToken)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        Dispose();

        _tcpClient = new TcpClient();
        await using (cancellationToken.Register(Dispose))
        {
            await _tcpClient.ConnectAsync(options.EndPoint.Address, options.EndPoint.Port, cancellationToken).ConfigureAwait(false);
            _networkStream = _tcpClient.GetStream();
        }
    }

    public void Dispose()
    {
        _tcpClient?.Close();
        _networkStream?.Dispose();
    }

    public Task<int> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
        ThrowIfNotConnected();
        if (buffer.Array == null) throw new InvalidOperationException("Buffer for received data is not initialized");
        return _networkStream!.ReadAsync(buffer.Array, buffer.Offset, buffer.Count, cancellationToken);
    }

    public Task SendAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
        ThrowIfNotConnected();
        return _networkStream!.WriteAsync(buffer.Array ?? [], buffer.Offset, buffer.Count, cancellationToken);
    }

    private void ThrowIfNotConnected()
    {
        if (_networkStream == null)
        {
            throw new InvalidOperationException("The CoAP transport layer is not connected.");
        }
    }
}
