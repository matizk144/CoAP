namespace CoAPnet.Protocol.Options;

internal record CoapMessageOptionOpaqueValue(byte[] Value) : ICoapMessageOptionValue
{
    public virtual bool Equals(CoapMessageOptionOpaqueValue? obj)
    {
        if (obj == null)
        {
            return false;
        }

        return Value.SequenceEqual(obj.Value);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}