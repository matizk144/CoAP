using CoAPnet.Protocol.Options;

namespace CoAPnet.Protocol;

internal sealed class CoapMessage
{
    public required CoapMessageType Type { get; init; }

    public byte[]? Token { get; init; }

    public required CoapMessageCode Code { get; init; }

    public required ushort Id { get; init; }

    public required IReadOnlyCollection<CoapMessageOption> Options { get; init; }

    public ArraySegment<byte> Payload { get; init; }
}