using CoAPnet.CoapMessageLevelClient;
using Microsoft.Extensions.Logging;
using System.Collections.Immutable;

namespace CoAPnet.Client;

public static class CoapClientFactory
{
    public static ICoapClient CreateClient(ILogger logger, Action<CoapClientFactoryOptions>? optionsAction = null)
    {
        var options = new CoapClientFactoryOptions();
        optionsAction?.Invoke(options);
        ICoapMessageLevelClient coapMessageClient = options.Retries != null ?
            new RetriesCoapMessageLevelClient(options.Retries.ToImmutableList(), logger) :
            new RawCoapMessageLevelClient(logger);
        return new SequentialCoapClient(coapMessageClient);
    }
}

public class CoapClientFactoryOptions
{
    internal IReadOnlyCollection<TimeSpan>? Retries { get; set; }
    internal uint? MaxBlockwiseSize { get; set; }

    public CoapClientFactoryOptions AddRetiresMechanism(IReadOnlyCollection<TimeSpan> retries)
    {
        Retries = retries.ToImmutableList();
        return this;
    }

    public CoapClientFactoryOptions SetBlockwiseSize(uint maxBlockwiseSize)
    {
        MaxBlockwiseSize = maxBlockwiseSize;
        return this;
    }
}