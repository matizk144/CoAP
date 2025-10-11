namespace CoAPnet.Client;

public sealed class CoapRequest
{
    public CoapRequestMethod Method { get; internal set; } = CoapRequestMethod.Get;

    public CoapRequestOptions Options { get; internal set; } = new();

    public ArraySegment<byte> Payload { get; internal set; }
}