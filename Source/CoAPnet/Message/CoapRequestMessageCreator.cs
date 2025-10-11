using CoAPnet.Client;
using CoAPnet.Protocol;

namespace CoAPnet.Message;

internal class CoapRequestMessageCreator(CoapMessageIdProvider coapMessageIdProvider)
{
    private readonly CoapRequestToMessageConverter _requestToMessageConverter = new(coapMessageIdProvider);


    public CoapMessage Convert(CoapRequest request)
    {
        var message = _requestToMessageConverter.Convert(request);
        return message;
    }
}