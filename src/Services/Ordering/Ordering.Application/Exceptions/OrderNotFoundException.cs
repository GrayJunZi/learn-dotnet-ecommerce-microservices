namespace Ordering.Application.Exceptions;

public class OrderNotFoundException : ApplicationException
{
    public OrderNotFoundException(string name, object @object):base($"Entity {name} with {@object} was not found")
    {
        
    }
}