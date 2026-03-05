namespace CoAPnet.Client.Options;

internal class RequestOptions : IRequestOptions
{
    public Func<CoapResponseStatusCode, CoapResponseBitwiseHandler>? BitwiseHandler { get; private set; }

    public IRequestOptions SetBitwiseHandler(Func<CoapResponseStatusCode, CoapResponseBitwiseHandler> handlerFunc)
    {
        BitwiseHandler = handlerFunc;
        return this;
    }
}

public interface IRequestOptions
{
    IRequestOptions SetBitwiseHandler(Func<CoapResponseStatusCode, CoapResponseBitwiseHandler> handlerFunc);
}

public enum CoapResponseBitwiseHandler
{
    Continue,
    Repeat,
    Interrupt
}