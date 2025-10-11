namespace CoAPnet.Client;

public sealed class CoapRequestOptions
{
    /// <summary>
    /// This is only required when accessing virtual servers.
    /// </summary>
    public string? UriHost
    {
        get; internal set;
    }

    /// <summary>
    /// This is only required when accessing virtual servers.
    /// </summary>
    public int? UriPort
    {
        get; internal set;
    }

    public string? UriPath
    {
        get; internal set;
    }

    public ICollection<string>? UriQuery
    {
        get; internal set;
    }
}