using System.Diagnostics;
using System.Net;
using CoAPnet.Client;
using CoAPnet.Exceptions;
using CoAPnet.Protocol;
using CoAPnet.Protocol.Encoding;
using CoAPnet.Transport;
using Microsoft.Extensions.Logging;

namespace CoAPnet.LowLevelClient;

internal sealed class LowLevelCoapClient(ILogger logger) : ILowLevelCoapClient
{
    private CoapMessageDecoder? _messageDecoder;
    private readonly CoapMessageEncoder _messageEncoder = new();

    // The size of the receive buffer is large enough so that a whole
    // UDP datagram will fit into the buffer at once.
    private readonly ArraySegment<byte> _receiveBuffer = new ArraySegment<byte>(new byte[65535]);
    private readonly SemaphoreSlim _syncRoot = new(1, 1);

    CoapClientConnectOptions? _connectOptions;
    CoapTransportLayerAdapter? _transportLayerAdapter;

    public async Task SendAsync(CoapMessage message, CancellationToken cancellationToken)
    {
        if (_transportLayerAdapter == null)
        {
            throw new InvalidOperationException("Client not connected!");
        }

        var requestMessageBuffer = _messageEncoder.Encode(message);

        await _syncRoot.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await _transportLayerAdapter.SendAsync(requestMessageBuffer, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _syncRoot.Release();
        }
    }

    public async Task<CoapMessage?> ReceiveAsync(CancellationToken cancellationToken)
    {
        if (_transportLayerAdapter == null || _messageDecoder == null)
        {
            throw new InvalidOperationException("Client not connected!");
        }

        var datagramLength = await _transportLayerAdapter.ReceiveAsync(_receiveBuffer, cancellationToken)
            .ConfigureAwait(false);

        if (datagramLength == 0)
        {
            return null;
        }

        Debug.Assert(_receiveBuffer.Array != null);
        return _messageDecoder.Decode(new ArraySegment<byte>(_receiveBuffer.Array, 0, datagramLength));
    }

    public void Dispose()
    {
        _transportLayerAdapter?.Dispose();
    }

    public async Task ConnectAsync(CoapClientConnectOptions options, CancellationToken cancellationToken)
    {
        _connectOptions = options ?? throw new ArgumentNullException(nameof(options));

        var transportLayer = options.TransportLayerFactory?.Invoke();

        if (transportLayer == null)
        {
            throw new InvalidOperationException("No CoAP transport layer is set.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        _messageDecoder = new CoapMessageDecoder(logger, options.EndpointId);

        _transportLayerAdapter = new CoapTransportLayerAdapter(transportLayer, logger, options.EndpointId);
        try
        {
            var transportLayerConnectOptions = new CoapTransportLayerConnectOptions
            {
                EndPoint = await ResolveIpEndPoint(options).ConfigureAwait(false)
            };

            await _transportLayerAdapter.ConnectAsync(transportLayerConnectOptions, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception)
        {
            _transportLayerAdapter.Dispose();
            throw;
        }
    }

    private async Task<IPEndPoint> ResolveIpEndPoint(CoapClientConnectOptions connectOptions)
    {
        if (IPAddress.TryParse(connectOptions.Host, out var ipAddress))
        {
            return new IPEndPoint(ipAddress, connectOptions.Port);
        }
        try
        {
            var hostIpAddresses = await Dns.GetHostAddressesAsync(connectOptions.Host).ConfigureAwait(false);
            if (hostIpAddresses.Length == 0)
            {
                throw new CoapCommunicationException("Failed to resolve DNS end point", null);
            }

            // We only use the first address for now.
            return new IPEndPoint(hostIpAddresses[0], _connectOptions.Port);
        }
        catch (Exception exception)
        {
            throw new CoapCommunicationException("Error while resolving DNS name.", exception);
        }
    }
}