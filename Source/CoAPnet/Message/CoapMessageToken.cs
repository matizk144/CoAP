namespace CoAPnet.Message;

internal record CoapMessageToken(byte[] Value)
{
    public override int GetHashCode()
    {
        var hash = 0;
        foreach (var @byte in Value)
        {
            hash ^= @byte;
        }

        return hash;
    }

    public virtual bool Equals(CoapMessageToken? obj)
    {
        if (obj == null)
        {
            return false;
        }

        return Value.SequenceEqual(obj.Value);
    }
}