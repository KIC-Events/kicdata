namespace KiCData.Exceptions;

public class InventoryException : KicException
{
    
    public InventoryException() : base("An error occured while processing the inventory request")
    {
    }

    public InventoryException(string message) : base(message)
    {
    }
}