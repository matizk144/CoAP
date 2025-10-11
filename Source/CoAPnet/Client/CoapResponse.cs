namespace CoAPnet.Client;

public record CoapResponse(CoapResponseStatusCode StatusCode, CoapResponseOptions Options, byte[] Payload);

