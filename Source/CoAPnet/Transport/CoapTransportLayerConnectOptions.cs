using System.Net;

namespace CoAPnet.Transport;

public class CoapTransportLayerConnectOptions
{
    public required IPEndPoint EndPoint { get; init; }
}