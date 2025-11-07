using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lib.Event.Net8.Cores.Events.Abstracts
{
    /// <summary>
    /// Événement de domaine représentant un fait métier significatif
    /// </summary>
    public abstract class DomainEvent(string? eventSource = null) : AbstractEvent(eventSource)
    {
        /// <summary>
        /// ID de l'aggrégat racine concerné
        /// </summary>
        public string AggregateId { get; set; }

        /// <summary>
        /// Type de l'aggrégat racine
        /// </summary>
        public string AggregateType { get; set; }

        /// <summary>
        /// Version de l'aggrégat après l'événement
        /// </summary>
        public long AggregateVersion { get; set; }
    }
}