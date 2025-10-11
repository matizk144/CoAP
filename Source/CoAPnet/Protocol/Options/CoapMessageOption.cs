namespace CoAPnet.Protocol.Options;

internal record CoapMessageOption(CoapMessageOptionNumber Number, ICoapMessageOptionValue Value)
{
    public virtual bool Equals(CoapMessageOption? obj)
    {
        if (obj == null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        return Number.Equals(obj.Number) && Value.Equals(obj.Value);
    }

    public override int GetHashCode()
    {
        return Number.GetHashCode();
    }
}