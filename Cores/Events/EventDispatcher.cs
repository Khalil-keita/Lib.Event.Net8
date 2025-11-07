// Events/EventDispatcher.cs
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Lib.Event.Net8.Cores.Events.Diagnostics;
using Lib.Event.Net8.Cores.Events.Exceptions;
using Lib.Event.Net8.Cores.Events.Interfaces;
using Microsoft.Extensions.Logging;

namespace Lib.Event.Net8.Cores.Events;

/// <summary>
/// Implémentation robuste du dispatcher d'événements avec gestion d'erreurs et métriques
/// </summary>
public class EventDispatcher(
    IServiceProvider serviceProvider,
    ILogger<EventDispatcher> logger,
    bool continueOnError = true) : IEventDispatcher
{
    private readonly ILogger<EventDispatcher> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly ConcurrentDictionary<Type, List<Type>> _eventListeners = new();
    private readonly bool _continueOnError = continueOnError;

    public async Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IEvent
    {
        if (@event == null)
            throw new ArgumentNullException(nameof(@event));

        var eventType = typeof(TEvent);
        var eventName = @event.EventName;

        using var activity = DiagnosticActivities.Source.StartActivity("DispatchEvent", ActivityKind.Internal);

        activity?.SetTag("event.name", eventName);
        activity?.SetTag("event.id", @event.EventId.ToString());

        _logger.LogDebug("Début du dispatch de l'événement {EventName} ({EventId})",
            eventName, @event.EventId);

        try
        {
            if (!_eventListeners.TryGetValue(eventType, out var listenerTypes) || !listenerTypes.Any())
            {
                _logger.LogWarning("Aucun écouteur enregistré pour l'événement {EventName}", eventName);
                return;
            }

            _logger.LogInformation("Dispatch de l'événement {EventName} à {ListenerCount} écouteur(s)",
                eventName, listenerTypes.Count);

            var tasks = new List<Task>();
            foreach (var listenerType in listenerTypes)
            {
                var task = ExecuteListenerAsync(listenerType, @event, cancellationToken);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            _logger.LogInformation("Dispatch de l'événement {EventName} terminé avec succès", eventName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du dispatch de l'événement {EventName}", eventName);
            throw new EventDispatchException($"Erreur lors du dispatch de l'événement {eventName}", ex);
        }
    }

    private async Task ExecuteListenerAsync<TEvent>(Type listenerType, TEvent @event,
        CancellationToken cancellationToken) where TEvent : IEvent
    {
        try
        {
            if (_serviceProvider.GetService(listenerType) is IEventListener<TEvent> listener)
            {
                using var listenerActivity = DiagnosticActivities.Source.StartActivity(
                    $"Process{listenerType.Name}", System.Diagnostics.ActivityKind.Internal);
                listenerActivity?.SetTag("listener.type", listenerType.Name);

                _logger.LogTrace("Exécution de l'écouteur {ListenerType} pour {EventName}",
                    listenerType.Name, @event.EventName);

                await listener.HandleAsync(@event, cancellationToken);

                _logger.LogDebug("Écouteur {ListenerType} exécuté avec succès", listenerType.Name);
            }
            else
            {
                _logger.LogWarning("⚠Impossible de résoudre l'écouteur {ListenerType}", listenerType.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur dans l'écouteur {ListenerType} pour l'événement {EventName}",
                listenerType.Name, @event.EventName);

            if (!_continueOnError)
                throw;
        }
    }

    public void Subscribe<TEvent>(IEventListener<TEvent> listener) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(listener);

        var eventType = typeof(TEvent);
        var listenerType = listener.GetType();

        _eventListeners.AddOrUpdate(
            eventType,
            _ => [listenerType],
            (_, existing) =>
            {
                if (!existing.Contains(listenerType))
                    existing.Add(listenerType);
                return existing;
            });

        _logger.LogDebug("Enregistrement de l'écouteur {ListenerType} pour {EventType}",
            listenerType.Name, eventType.Name);
    }

    public void Unsubscribe<TEvent>(IEventListener<TEvent> listener) where TEvent : IEvent
    {
        ArgumentNullException.ThrowIfNull(listener);

        var eventType = typeof(TEvent);
        var listenerType = listener.GetType();

        if (_eventListeners.TryGetValue(eventType, out var listeners))
        {
            listeners.Remove(listenerType);
            _logger.LogDebug("Désenregistrement de l'écouteur {ListenerType} pour {EventType}",
                listenerType.Name, eventType.Name);
        }
    }

    public void SubscribeAllFromAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        var eventListenerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventListener<>))
                .Select(i => new
                {
                    ListenerType = t,
                    EventType = i.GetGenericArguments()[0]
                }))
            .ToList();

        foreach (var item in eventListenerTypes)
        {
            _eventListeners.AddOrUpdate(
                item.EventType,
                _ => [item.ListenerType],
                (_, existing) =>
                {
                    if (!existing.Contains(item.ListenerType))
                        existing.Add(item.ListenerType);
                    return existing;
                });
        }

        _logger.LogInformation("{Count} écouteurs enregistrés depuis l'assembly {Assembly}",
            eventListenerTypes.Count, assembly.GetName().Name);
    }
}