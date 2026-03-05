namespace CoAPnet.Exceptions
{
    public class CoapCommunicationException(string message, Exception exception) : IOException(message, exception);
}
