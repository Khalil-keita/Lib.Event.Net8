using System.Reflection;

namespace Lib.Event.Net8.Cores.Events.Interfaces
{
    /// <summary>
    /// Dispatcher d'événements avec support de l'injection de dépendances
    /// </summary>
    public interface IEventDispatcher
    {
        /// <summary>
        /// Dispatch un événement à tous les écouteurs enregistrés
        /// </summary>
        Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : IEvent;

        /// <summary>
        /// Enregistre un écouteur pour un type d'événement
        /// </summary>
        void Subscribe<TEvent>(IEventListener<TEvent> listener) where TEvent : IEvent;

        /// <summary>
        /// Désenregistre un écouteur
        /// </summary>
        void Unsubscribe<TEvent>(IEventListener<TEvent> listener) where TEvent : IEvent;

        /// <summary>
        /// Enregistre tous les écouteurs d'un assembly via reflection
        /// </summary>
        void SubscribeAllFromAssembly(Assembly assembly);
    }
}