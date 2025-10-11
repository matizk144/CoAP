using CoAPnet.Protocol.Options;

namespace CoAPnet.Client;

public sealed class CoapResponseOptions
{
    public CoapMessageContentFormat? ContentFormat
    {
        get; internal set;
    }

    public int MaxAge
    {
        get; internal set;
    }

    public byte[]? ETag
    {
        get; internal set;
    }
}

