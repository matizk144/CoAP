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
        return options.Blockwise != null ?
            new BlockwiseSequentialCoapClient(coapMessageClient, options.Blockwise) : 
            new SequentialCoapClient(coapMessageClient);
    }
}

public class CoapClientFactoryOptions
{
    internal IReadOnlyCollection<TimeSpan>? Retries { get; set; }
    internal BlockwiseMessageOptions? Blockwise { get; set; }

    public CoapClientFactoryOptions AddRetiresMechanism(IReadOnlyCollection<TimeSpan> retries)
    {
        Retries = retries.ToImmutableList();
        return this;
    }

    public CoapClientFactoryOptions EnableBlockwise(BlockwiseMessageOptions blockwiseMessageOptions)
    {
        Blockwise = blockwiseMessageOptions;
        return this;
    }
}