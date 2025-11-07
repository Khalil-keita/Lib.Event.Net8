using System.Reflection;

namespace Lib.Event.Net8.Cores.Config
{
    public class EventOptions
    {
        public bool AutoRegisterListeners { get; set; } = true;
        public bool ContinueOnError { get; set; } = true;

        public Assembly[] AssembliesToScan { get; set; } =
        [
            Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()
        ];
    }
}