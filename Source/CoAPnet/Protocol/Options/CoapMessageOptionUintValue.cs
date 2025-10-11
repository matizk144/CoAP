namespace CoAPnet.Protocol.Options;

internal record CoapMessageOptionUintValue(uint Value) : ICoapMessageOptionValue
{
    public virtual bool Equals(CoapMessageOptionUintValue? obj)
    {
        return obj is not null && Value.Equals(obj.Value);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}