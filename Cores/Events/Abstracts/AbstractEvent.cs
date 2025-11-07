using System.Reflection;
using Lib.Event.Net8.Cores.Events.Interfaces;

namespace Lib.Event.Net8.Cores.Events.Abstracts
{
    /// <summary>
    /// Implémentation de base pour tous les événements du domaine
    /// </summary>
    public abstract class AbstractEvent : IEvent
    {
        protected AbstractEvent(string? eventSource = null)
        {
            EventId = Guid.NewGuid();
            OccurredOn = DateTime.UtcNow;
            EventName = GetType().Name;
            EventVersion = "1.0";
            EventSource = eventSource ?? Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown";
        }

        public Guid EventId { get; }
        public DateTime OccurredOn { get; }
        public string EventName { get; }
        public string EventVersion { get; }
        public string EventSource { get; }
    }
}