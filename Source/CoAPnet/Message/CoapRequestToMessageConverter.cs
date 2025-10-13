using CoAPnet.Client;
using CoAPnet.Protocol;
using CoAPnet.Protocol.Options;

namespace CoAPnet.Message;

internal sealed class CoapRequestToMessageConverter(CoapMessageIdProvider coapMessageIdProvider)
{
    readonly CoapMessageOptionFactory _optionFactory = new CoapMessageOptionFactory();

    public CoapMessage Convert(CoapRequest request, IReadOnlyCollection<CoapMessageOption>? additionalOptions = null)
    {
        List<CoapMessageOption> options = [];

        ApplyUriHost(options, request);
        ApplyUriPort(options, request);
        ApplyUriPath(options, request);
        ApplyUriQuery(options, request);

        if (additionalOptions != null)
        {
            options.AddRange(additionalOptions);
        }

        var message = new CoapMessage
        {
            Id = coapMessageIdProvider.Next(),
            Type = CoapMessageType.Confirmable,
            Code = GetMessageCode(request.Method),
            Options = options,
            Payload = request.Payload
        };


        return message;
    }

    private void ApplyUriHost(List<CoapMessageOption> options, CoapRequest request)
    {
        if (string.IsNullOrEmpty(request.Options.UriHost))
        {
            return;
        }

        options.Add(_optionFactory.CreateUriHost(request.Options.UriHost));
    }

    void ApplyUriPort(List<CoapMessageOption> options, CoapRequest request)
    {
        if (!request.Options.UriPort.HasValue)
        {
            return;
        }

        options.Add(_optionFactory.CreateUriPort((uint)request.Options.UriPort.Value));
    }

    void ApplyUriPath(List<CoapMessageOption> options, CoapRequest request)
    {
        if (string.IsNullOrEmpty(request.Options.UriPath))
        {
            return;
        }

        var paths = request.Options.UriPath.Split(['/'], StringSplitOptions.RemoveEmptyEntries);

        foreach (var path in paths)
        {
            options.Add(_optionFactory.CreateUriPath(path));
        }
    }

    void ApplyUriQuery(List<CoapMessageOption> options, CoapRequest request)
    {
        if (request.Options.UriQuery == null)
        {
            return;
        }

        foreach (var query in request.Options.UriQuery)
        {
            options.Add(_optionFactory.CreateUriQuery(query));
        }
    }

    static CoapMessageCode GetMessageCode(CoapRequestMethod method)
    {
        return method switch
        {
            CoapRequestMethod.Get => CoapMessageCodes.Get,
            CoapRequestMethod.Post => CoapMessageCodes.Post,
            CoapRequestMethod.Delete => CoapMessageCodes.Delete,
            CoapRequestMethod.Put => CoapMessageCodes.Put,
            _ => throw new NotSupportedException()
        };
    }
}