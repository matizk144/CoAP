using System.Collections.Immutable;
using CoAPnet.Protocol;
using Microsoft.Extensions.Logging;

namespace CoAPnet.CoapMessageLevelClient;

internal sealed class RetriesCoapMessageLevelClient(ImmutableList<TimeSpan> retryTimeouts, ILogger logger) : RawCoapMessageLevelClient(logger)
{
    public override async Task<CoapMessage> Send(CoapMessage coapMessage, CancellationToken cancellationToken)
    {
        short retriesCount = 0;

        while (retriesCount < retryTimeouts.Count)
        {
            try
            {
                using var timeoutToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutToken.CancelAfter(retryTimeouts[retriesCount]);
                var response = await base.Send(coapMessage, timeoutToken.Token);
                return response;
            }
            catch (OperationCanceledException)
            {
                Logger.LogInformation("{EndpointId}: {Module}: Retrying CoAP Request: {CoapMessageId}", EndpointId, nameof(RetriesCoapMessageLevelClient), coapMessage.Id);
                retriesCount++;
            }
        }

        throw new TimeoutException("Coap message not received");
    }
}