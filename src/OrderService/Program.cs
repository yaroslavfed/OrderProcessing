using Microsoft.EntityFrameworkCore;
using OrderService.Application.Services.OrdersService;
using OrderService.Endpoints;
using OrderService.Extensions;
using OrderService.Infrastructure.Cache;
using OrderService.Infrastructure.Persistence;
using Shared.Messaging;
using Shared.Messaging.Abstractions;
using Shared.Messaging.RabbitMq;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiDocumentation();

builder.Services.AddDbContext<OrdersDbContext>(options =>
    {
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("Postgres")
        );
    }
);

builder.Services.AddRabbitMqMessaging(builder.Configuration);

builder.Services.AddScoped<IOrdersService, OrdersService>();
builder.Services.AddScoped<IOrdersCache, RedisOrdersCache>();

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    {
        var connectionString = builder.Configuration["Redis:ConnectionString"];

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Redis connection string is not configured.");

        return ConnectionMultiplexer.Connect(connectionString);
    }
);

var app = builder.Build();

app.UseApiDocumentation(app.Environment);

app.UseHttpsRedirection();

app.MapOrdersEndpoints();

app.Run();