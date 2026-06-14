using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Shared.Messaging.Abstractions;
using Shared.Messaging.RabbitMq.RabbitMqConnection;

namespace Shared.Messaging.RabbitMq;

public class RabbitMqEventPublisher : IEventPublisher
{
    private readonly IRabbitMqConnection             _connection;
    private readonly RabbitMqOptions                 _options;
    private readonly ILogger<RabbitMqEventPublisher> _logger;

    public RabbitMqEventPublisher(
        IRabbitMqConnection connection,
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqEventPublisher> logger
    )
    {
        _connection = connection;
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync<T>(T message, string routingKey, CancellationToken cancellationToken = default)
    {
        var connection = await _connection.GetConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

        var properties = new BasicProperties
        {
            Persistent = true, ContentType = "application/json", Type = typeof(T).Name
        };

        await channel.BasicPublishAsync(
            exchange: _options.ExchangeName,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken
        );

        _logger.LogInformation(
            "Message {MessageType} published to exchange {Exchange} with routing key {RoutingKey}",
            typeof(T).Name,
            _options.ExchangeName,
            routingKey
        );
    }
}