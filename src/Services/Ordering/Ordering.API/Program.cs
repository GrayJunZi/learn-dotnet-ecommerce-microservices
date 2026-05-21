using EventBus.Messages.Common;
using MassTransit;
using Ordering.API.Extensions;
using Ordering.Application.Dispatcher;
using Ordering.Application.EventBusConsumer;
using Ordering.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddOrderingServices(builder.Configuration);

builder.Services.AddHostedService<OutboxMessageDispatcher>();

builder.Services.AddMassTransit(configure =>
{
    configure.AddConsumer<BasketOrderingConsumer>();
    configure.AddConsumer<PaymentCompletedConsumer>();
    configure.AddConsumer<PaymentFailedConsumer>();
    configure.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["EventBusSettings:HostAddress"]);
        cfg.ReceiveEndpoint(EventBusConstants.BasketCheckoutQueue,
            c => { c.ConfigureConsumer<BasketOrderingConsumer>(ctx); });

        cfg.ReceiveEndpoint(EventBusConstants.PaymentCompletedQueue,
            c => { c.ConfigureConsumer<PaymentCompletedConsumer>(ctx); });

        cfg.ReceiveEndpoint(EventBusConstants.PaymentFailedQueue,
            c => { c.ConfigureConsumer<PaymentFailedConsumer>(ctx); });
    });
});

var app = builder.Build();

app.MigrateDatabase<OrderContext>((context, services) =>
{
    var logger = services.GetRequiredService<ILogger<OrderContextSeed>>();
    OrderContextSeed.SeedAsync(context, logger).Wait();
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();