using RabbitMQ.Client;

namespace Shared.Messaging.RabbitMq.RabbitMqConnection;

public interface IRabbitMqConnection : IAsyncDisposable
{
    Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default);
}