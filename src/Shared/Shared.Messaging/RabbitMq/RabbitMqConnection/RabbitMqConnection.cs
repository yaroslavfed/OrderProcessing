using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Shared.Messaging.RabbitMq.RabbitMqConnection;

public sealed class RabbitMqConnection : IRabbitMqConnection
{
    private readonly ILogger<RabbitMqConnection> _logger;
    private readonly RabbitMqOptions             _options;
    private readonly SemaphoreSlim               _connectionLock = new(1, 1);

    private IConnection? _connection;

    public RabbitMqConnection(IOptions<RabbitMqOptions> options, ILogger<RabbitMqConnection> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IConnection> GetConnectionAsync(CancellationToken cancellationToken = default)
    {
        if (_connection is
        {
            IsOpen: true
        })
            return _connection;

        await _connectionLock.WaitAsync(cancellationToken);

        try
        {
            if (_connection is
            {
                IsOpen: true
            })
                return _connection;

            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                UserName = _options.Username,
                Password = _options.Password,
                ClientProvidedName = AppDomain.CurrentDomain.FriendlyName
            };

            _connection = await factory.CreateConnectionAsync(cancellationToken);

            _logger.LogInformation("RabbitMQ connection established to {Host}:{Port}", _options.Host, _options.Port);

            return _connection;
        } finally
        {
            _connectionLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection is
            { })
            await _connection.DisposeAsync();

        _connectionLock.Dispose();
    }

}