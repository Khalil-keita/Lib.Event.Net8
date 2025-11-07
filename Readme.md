# Lib.Event.Net8

Une bibliothèque .NET robuste et prête à l'emploi pour implémenter le pattern `Event-Driven` Architecture dans vos applications.

## 🚀 Fonctionnalités

- ✅ Dispatcher d'événements avec support DI natif
- ✅ Découverte automatique des écouteurs via reflection
- ✅ Gestion d'erreurs configurable (continue ou fail-fast)
- ✅ Logging structuré avec ILogger intégré
- ✅ Tracing et métriques avec System.Diagnostics.Activity
- ✅ Thread-safe grâce aux collections concurrentes
- ✅ Asynchrone complet avec support CancellationToken
- ✅ Extensible via options de configuration

## Configurations

### Enregistrement des services

```C#
var builder = WebApplication.CreateBuilder(args);

// Configuration de base
builder.Services.AddEvent();

// Configuration avancée
builder.Services.AddEvent(options =>
{
    options.AutoRegisterListeners = true;
    options.ContinueOnError = true;
    options.AssembliesToScan = new[] { 
        typeof(Program).Assembly,
        typeof(YourEventListener).Assembly
    };
});
```

### Définition d'un événement
```C#
public class UserCreatedEvent : DomainEvent
{
    public UserCreatedEvent(Guid userId, string email, string firstName, string lastName)
    {
        AggregateId = userId.ToString();
        AggregateType = "User";
        UserId = userId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }

    public Guid UserId { get; }
    public string Email { get; }
    public string FirstName { get; }
    public string LastName { get; }
}
```

### Implémentation d'un écouteur
```C#
public class UserCreatedEmailListener : IEventListener<UserCreatedEvent>
{
    private readonly ILogger<UserCreatedEmailListener> _logger;

    public UserCreatedEmailListener(ILogger<UserCreatedEmailListener> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(UserCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📧 Envoi d'email de bienvenue à {Email}", @event.Email);
        
        // Votre logique d'envoi d'email ici
        await SendWelcomeEmailAsync(@event.Email, @event.FirstName);
        
        _logger.LogInformation("✅ Email de bienvenue envoyé à {Email}", @event.Email);
    }

    private async Task SendWelcomeEmailAsync(string email, string firstName)
    {
        // Implémentation de l'envoi d'email
        await Task.Delay(100); // Simulation
    }
}
```

### Utilisation dans un service métier
```C#
public class UserService
{
    private readonly IEventDispatcher _eventDispatcher;
    private readonly ILogger<UserService> _logger;

    public UserService(IEventDispatcher eventDispatcher, ILogger<UserService> logger)
    {
        _eventDispatcher = eventDispatcher;
        _logger = logger;
    }

    public async Task<User> CreateUserAsync(CreateUserCommand command)
    {
        // 1. Logique métier de création
        var user = new User(command.Email, command.FirstName, command.LastName);
        
        // 2. Persistance (exemple)
        await _userRepository.AddAsync(user);
        
        // 3. Publication de l'événement
        var userCreatedEvent = new UserCreatedEvent(
            user.Id, 
            user.Email, 
            user.FirstName, 
            user.LastName
        );
        
        await _eventDispatcher.DispatchAsync(userCreatedEvent);

        return user;
    }
}
```

### API Principale
```C#
public interface IEventDispatcher
{
    Task DispatchAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) 
        where TEvent : IEvent;

    void Subscribe<TEvent>(IEventListener<TEvent> listener) where TEvent : IEvent;
    void Unsubscribe<TEvent>(IEventListener<TEvent> listener) where TEvent : IEvent;
    void SubscribeAllFromAssembly(Assembly assembly);
}
```

### Options de Configuration 
| Option                | Description                                       | Défaut                         |
|------------------------|---------------------------------------------------|---------------------------------|
| AutoRegisterListeners  | Découverte automatique des écouteurs              | true                            |
| ContinueOnError        | Continue en cas d'erreur dans un écouteur         | true                            |
| AssembliesToScan       | Assemblies à scanner pour les écouteurs           | Assembly.GetEntryAssembly()     |


###  Logging et Diagnostic
```log
 Début du dispatch de l'événement UserCreatedEvent (a1b2c3d4...)
 Dispatch de l'événement UserCreatedEvent à 3 écouteur(s)
 Exécution de l'écouteur WelcomeEmailListener pour UserCreatedEvent
 Écouteur WelcomeEmailListener exécuté avec succès
 Dispatch de l'événement UserCreatedEvent terminé avec succès

```
