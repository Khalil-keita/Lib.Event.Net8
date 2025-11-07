namespace Lib.Event.Net8.Cores.Events.Interfaces
{
    /// <summary>
    /// Contrat pour les écouteurs d'événements
    /// </summary>
    public interface IEventListener<in TEvent> where TEvent : IEvent
    {
        /// <summary>
        /// Traite un événement de manière asynchrone
        /// </summary>
        Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default);
    }
}