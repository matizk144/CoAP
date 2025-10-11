namespace CoAPnet.Protocol.Options;

internal record CoapMessageOptionStringValue(string Value) : ICoapMessageOptionValue
{
    public virtual bool Equals(CoapMessageOptionStringValue? obj)
    {
        if (obj != null)
        {
            return string.Equals(Value, obj.Value, StringComparison.Ordinal);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}