namespace KiCData.Exceptions;

public class KicException : Exception
{
    public KicException() : base("An error occured in the KIC application.")
    {
    }

    public KicException(string message) : base(message)
    {
    }
}