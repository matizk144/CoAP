using CoAPnet.Protocol;
using CoAPnet.Transport;

namespace CoAPnet.Client;

public class CoapClientConnectOptions
{
    public string? Host { get; set; }

    public int Port { get; set; } = CoapDefaultPort.Unencrypted;

    public TimeSpan CommunicationTimeout { get; set; } = TimeSpan.FromSeconds(10);

    public Func<ICoapTransportLayer> TransportLayerFactory { get; set; } = () => new UdpCoapTransportLayer();

    public string EndpointId { get; set; } = Guid.NewGuid().ToString();
}