using CoAPnet.Client;

namespace CoAPnet.Exceptions;

public class BitwiseCoapCommunicationInterruptedException(CoapResponse coapResponse) : IOException
{
    public CoapResponse CoapResponse { get; } = coapResponse;
}