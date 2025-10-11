using CoAPnet.Protocol;

namespace CoAPnet.LowLevelClient;

internal interface ILowLevelCoapClient : IDisposable
{
    Task SendAsync(CoapMessage message, CancellationToken cancellationToken);

    Task<CoapMessage?> ReceiveAsync(CancellationToken cancellationToken);
}