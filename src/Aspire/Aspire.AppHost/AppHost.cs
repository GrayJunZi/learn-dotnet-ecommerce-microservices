var builder = DistributedApplication.CreateBuilder(args);

// Infrastructure

// MongoDB - Catalog
var mongo = builder.AddMongoDB("catalogdb").WithDataVolume();

// Redis - Basket
var redis = builder.AddRedis("basketdb");

// Postgres - Discount
var postgres = builder.AddPostgres("discountdb").WithDataVolume();
var discountDb = postgres.AddDatabase("discount-db");

// Sqlserver - Ordering + Identity
var sqlserver = builder.AddSqlServer("sqlserver").WithDataVolume();
var orderingDb = sqlserver.AddDatabase("OrderingDb");
var identityDb = sqlserver.AddDatabase("IdentityDb");

// RabbitMQ
var rabbitmq = builder.AddRabbitMQ("rabbitmq");

// APIs
var catalog = builder.AddProject<Projects.Catalog_API>("catalog")
    .WithReference(mongo);

var basket = builder.AddProject<Projects.Basket_API>("basket")
    .WithReference(redis)
    .WithReference(rabbitmq);

var ordering = builder.AddProject<Projects.Ordering_API>("ordering")
    .WithReference(orderingDb)
    .WithReference(rabbitmq);

var discount = builder.AddProject<Projects.Discount_API>("discount")
    .WithReference(discountDb);

var payment = builder.AddProject<Projects.Payment_API>("payment")
    .WithReference(rabbitmq);

var identity = builder.AddProject<Projects.Identity_API>("identity")
    .WithReference(identityDb)
    .WithEnvironment("Jwt__Key","super_secure_secret_key1234567890#@!")
    .WithEnvironment("Jwt__Issuer","learn-dotnet-ecommerce-microservices")
    .WithEnvironment("Jwt__Audience","learn-dotnet-ecommerce-microservices")
    .WithEnvironment("Jwt__DurationInMinutes","60");

// API Gateway
var gateway = builder.AddProject<Projects.ApiGateway>("apigateway");
gateway
    .WithReference(catalog)
    .WithReference(basket)
    .WithReference(ordering)
    .WithReference(discount)
    .WithReference(payment);

builder.Build().Run();