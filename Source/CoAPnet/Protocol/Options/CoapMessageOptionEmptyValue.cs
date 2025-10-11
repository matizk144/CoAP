namespace CoAPnet.Protocol.Options;

internal record CoapMessageOptionEmptyValue : ICoapMessageOptionValue
{
    public virtual bool Equals(CoapMessageOptionEmptyValue? obj)
    {
        return obj != null;
    }

    public override int GetHashCode()
    {
        return 0;
    }
}