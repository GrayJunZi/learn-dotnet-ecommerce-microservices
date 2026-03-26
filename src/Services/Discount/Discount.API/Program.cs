using System.Reflection;
using Discount.API.Services;
using Discount.Application.Handlers;
using Discount.Core.Repositories;
using Discount.Infrastructure.Repositories;
using Discount.Infrastructure.Settings;

var builder = WebApplication.CreateBuilder(args);

var assemblies = new Assembly[]
{
    Assembly.GetExecutingAssembly(), typeof(CreateDiscountHandler).Assembly,
};
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssemblies(assemblies));

builder.Services.AddScoped<IDiscountRepository, DiscountRepository>();
builder.Services.AddGrpc();

builder.Services.Configure<DatabaseSettings>(
    builder.Configuration.GetSection(nameof(DatabaseSettings)));

var app = builder.Build();

app.MigrateDatabase();
app.UseRouting();
app.MapGrpcService<DiscountService>();

app.Run();