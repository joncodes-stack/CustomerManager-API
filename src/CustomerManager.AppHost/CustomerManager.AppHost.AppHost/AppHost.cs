using Microsoft.Extensions.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var isTesting = builder.Environment.IsEnvironment("Testing");

// PostgreSQL
var postgres = isTesting
    ? builder.AddPostgres("Postgres")
        .WithLifetime(ContainerLifetime.Session)
        .AddDatabase("Default", "fgcgames-db")
    : builder.AddPostgres("Postgres", port: 5432)
        .WithLifetime(ContainerLifetime.Session)
        .WithPgAdmin(c => c.WithLifetime(ContainerLifetime.Session))
        .AddDatabase("Default", "customer-manager-db");

// Redis
var redis = builder.AddRedis("redis");

// LocalStack
var localstack = builder.AddContainer("localstack", "localstack/localstack-pro")
    .WithHttpEndpoint(targetPort: 4566, name: "edge")
    .WithEnvironment("SERVICES", "sns")
    .WithEnvironment("AWS_DEFAULT_REGION", "us-east-1")
    .WithEnvironment("DEBUG", "1")
    .WithEnvironment("LOCALSTACK_AUTH_TOKEN",
        Environment.GetEnvironmentVariable("LOCALSTACK_AUTH_TOKEN") ?? "");

var localstackEdge = localstack.GetEndpoint("edge");

// API
builder.AddProject<Projects.CustomerManager_Api>("api")
    .WithReference(postgres)
    .WithReference(redis)
    .WithEnvironment("AWS__SNS__ServiceUrl", localstackEdge)
    .WaitFor(postgres)
    .WaitFor(redis)
    .WaitFor(localstack);

builder.Build().Run();

builder.Build().Run();