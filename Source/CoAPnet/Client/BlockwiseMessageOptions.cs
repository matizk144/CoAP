namespace CoAPnet.Client;

public record BlockwiseMessageOptions(
    BlockwisePayloadSize? RequestPayloadSize = BlockwisePayloadSize._1024,
    BlockwisePayloadSize? ResponsePayloadSize = BlockwisePayloadSize._1024);

public enum BlockwisePayloadSize
{
    _16 = 16,
    _32 = 32,
    _64 = 64,
    _128 = 128,
    _256 = 256,
    _512 = 512,
    _1024 = 1024
}