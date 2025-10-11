namespace CoAPnet.Protocol;

internal record CoapMessageCode(byte Class, byte Detail)
{
    public override string ToString()
    {
        return $"{Class}.{Detail.ToString().PadLeft(2, '0')}";
    }

    public override int GetHashCode()
    {
        return Class.GetHashCode() ^ Detail.GetHashCode();
    }

    public virtual bool Equals(CoapMessageCode? other)
    {
        if (other == null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Class.Equals(other.Class) && Detail.Equals(other.Detail);
    }
}