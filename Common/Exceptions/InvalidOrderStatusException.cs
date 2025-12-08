namespace Common.Exceptions;

public class InvalidOrderStatusException : Exception
{
    public InvalidOrderStatusException(string message) : base(message)
    {
    }

    public InvalidOrderStatusException(string currentStatus, string attemptedAction)
        : base($"Cannot {attemptedAction} order with status '{currentStatus}'.")
    {
    }
}

