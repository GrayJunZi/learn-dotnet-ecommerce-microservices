using System.Reflection;
using Basket.Application.GrpcServices;
using Basket.Application.Handlers;
using Basket.Application.Settings;
using Basket.Core.Repositories;
using Basket.Infrastructure.Repositories;
using Basket.Infrastructure.Settings;
using Discount.Grpc.Protos;
using MassTransit;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<IBasketRepository, BasketRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var assemblies = new Assembly[]
{
    Assembly.GetExecutingAssembly(),
    typeof(CreateShoppingCartHandler).Assembly,
};
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(assemblies));

builder.Services.Configure<CacheSettings>(
    builder.Configuration.GetSection(nameof(CacheSettings)));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration
        .GetSection(nameof(CacheSettings))
        .GetValue<string>("ConnectionString");
});


builder.Services.Configure<GrpcSettings>(
    builder.Configuration.GetSection(nameof(GrpcSettings)));

builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>((sp, options) =>
{
    var grpcSettings = sp.GetRequiredService<IOptions<GrpcSettings>>().Value;
    options.Address = new Uri(grpcSettings.DiscountUrl);
});
builder.Services.AddScoped<DiscountGrpcService>();

builder.Services.AddMassTransit(configure =>
{
    configure.UsingRabbitMq((ct, cfg) =>
    {
        cfg.Host(builder.Configuration["EventBusSettings:HostAddress"]);
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.Run();