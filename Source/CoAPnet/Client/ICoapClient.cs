using System;
using System.Threading;
using System.Threading.Tasks;
using CoAPnet.Client.Options;

namespace CoAPnet.Client
{
    public interface ICoapClient : IDisposable
    {
        Task ConnectAsync(CoapClientConnectOptions options, CancellationToken cancellationToken);

        Task<CoapResponse> RequestAsync(CoapRequest request, CancellationToken cancellationToken, Action<IRequestOptions>? options = null);

        //Task<CoapObserveResponse> ObserveAsync(CoapObserveOptions options, CancellationToken cancellationToken);

        //Task StopObservationAsync(CoapObserveResponse observeResponse, CancellationToken cancellationToken);
    }
}
