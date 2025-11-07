namespace Lib.Event.Net8.Cores.Events.Interfaces
{
    /// <summary>
    /// Interface de base pour tous les événements du domaine
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// Identifiant unique de l'événement
        /// </summary>
        public Guid EventId { get; }

        /// <summary>
        /// Date/heure de survenue de l'événement (UTC)
        /// </summary>
        public DateTime OccurredOn { get; }

        /// <summary>
        /// Nom de l'événement pour le routing et le logging
        /// </summary>
        public string EventName { get; }

        /// <summary>
        /// Version du schéma de l'événement pour la compatibilité
        /// </summary>
        public string EventVersion { get; }

        /// <summary>
        /// Source de l'événement (microservice, contexte limité)
        /// </summary>
        public string EventSource { get; }
    }
}