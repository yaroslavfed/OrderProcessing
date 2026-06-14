namespace Shared.Messaging.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default);
}