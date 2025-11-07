namespace Lib.Event.Net8.Cores.Events.Exceptions
{
    public class EventDispatchException : Exception
    {
        public EventDispatchException(string message) : base(message)
        {
        }

        public EventDispatchException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}