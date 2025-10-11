using CoAPnet.Client;
using CoAPnet.Protocol;

namespace CoAPnet.CoapMessageLevelClient;

internal interface ICoapMessageLevelClient : IDisposable
{
    Task ConnectAsync(CoapClientConnectOptions options, CancellationToken cancellationToken);

    Task<CoapMessage> Send(CoapMessage coapMessage, CancellationToken cancellationToken);
}