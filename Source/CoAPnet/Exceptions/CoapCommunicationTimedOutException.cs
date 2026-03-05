using System;

namespace CoAPnet.Exceptions
{
    public class CoapCommunicationTimedOutException(string message, Exception exception)
        : CoapCommunicationException(message, exception)
    {
        public CoapCommunicationTimedOutException()
            : this("CoAP communication timed out.", null)
        {
        }
    }
}
