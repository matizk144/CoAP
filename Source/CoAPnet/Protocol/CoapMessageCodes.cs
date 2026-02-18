namespace CoAPnet.Protocol;

internal static class CoapMessageCodes
{
    public static CoapMessageCode Empty { get; } = new(0, 0);

    public static CoapMessageCode Get { get; } = new(0, 1);
    public static CoapMessageCode Post { get; } = new(0, 2);
    public static CoapMessageCode Put { get; } = new(0, 3);
    public static CoapMessageCode Delete { get; } = new(0, 4);

    public static CoapMessageCode Created { get; } = new(2, 1);
    public static CoapMessageCode Deleted { get; } = new(2, 2);
    public static CoapMessageCode Valid { get; } = new(2, 3);
    public static CoapMessageCode Changed { get; } = new(2, 4);
    public static CoapMessageCode Content { get; } = new(2, 5);
    public static CoapMessageCode Continue { get; } = new(2, 31);

    public static CoapMessageCode BadRequest { get; } = new(4, 0);
    public static CoapMessageCode Unauthorized { get; } = new(4, 1);
    public static CoapMessageCode BadOption { get; } = new(4, 2);
    public static CoapMessageCode Forbidden { get; } = new(4, 3);
    public static CoapMessageCode NotFound { get; } = new(4, 4);
    public static CoapMessageCode MethodNotAllowed { get; } = new(4, 5);
    public static CoapMessageCode NotAcceptable { get; } = new(4, 6);
    public static CoapMessageCode RequestEntityIncomplete { get; } = new(4, 8);
    public static CoapMessageCode PreconditionFailed { get; } = new(4, 12);
    public static CoapMessageCode RequestEntityTooLarge { get; } = new(4, 13);
    public static CoapMessageCode UnsupportedContentFormat { get; } = new(4, 15);
    public static CoapMessageCode TooManyRequests { get; } = new(4, 29);

    public static CoapMessageCode InternalServerError { get; } = new(5, 0);
    public static CoapMessageCode NotImplemented { get; } = new(5, 1);
    public static CoapMessageCode BadGateway { get; } = new(5, 2);
    public static CoapMessageCode ServiceUnavailable { get; } = new(5, 3);
    public static CoapMessageCode GatewayTimeout { get; } = new(5, 4);
    public static CoapMessageCode ProxyingNotSupported { get; } = new(5, 5);
}