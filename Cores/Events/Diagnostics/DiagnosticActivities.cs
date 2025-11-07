using System.Diagnostics;

namespace Lib.Event.Net8.Cores.Events.Diagnostics
{
    public static class DiagnosticActivities
    {
        public static readonly string SourceName = "Event.Core";
        public static readonly ActivitySource Source = new(SourceName);
    }
}