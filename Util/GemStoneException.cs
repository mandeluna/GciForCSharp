public class GemStoneException : Exception
{
    public int ErrorNumber { get; }
    public string RawMessage { get; }

    public GemStoneException(int errorNumber, string message) 
        : base($"Guava Ops exception {errorNumber} received: {message}")
    {
        ErrorNumber = errorNumber;
        RawMessage = message;
    }
}