// Extensions/ServiceCollectionExtensions.cs
using System.Reflection;
using Lib.Event.Net8.Cores.Events;
using Lib.Event.Net8.Cores.Events.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Lib.Event.Net8.Cores.Config;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Enregistre le système d'événements avec discovery automatique des écouteurs
    /// </summary>
    public static IServiceCollection AddEvent(this IServiceCollection services, Action<EventOptions>? configureOptions = null)
    {
        var options = new EventOptions();
        configureOptions?.Invoke(options);

        // Enregistre le dispatcher
        services.AddSingleton<IEventDispatcher, EventDispatcher>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<EventDispatcher>>();
            return new EventDispatcher(provider, logger, options.ContinueOnError);
        });

        // Discovery automatique des écouteurs
        if (options.AutoRegisterListeners)
        {
            services.RegisterEventListeners(options.AssembliesToScan);
        }

        return services;
    }

    /// <summary>
    /// Enregistre tous les écouteurs d'événements des assemblies spécifiées
    /// </summary>
    private static void RegisterEventListeners(this IServiceCollection services, Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var listenerTypes = assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .Where(t => t.GetInterfaces()
                    .Any(i => i.IsGenericType &&
                             i.GetGenericTypeDefinition() == typeof(IEventListener<>)));

            foreach (var listenerType in listenerTypes)
            {
                services.AddTransient(listenerType);
            }
        }
    }
}