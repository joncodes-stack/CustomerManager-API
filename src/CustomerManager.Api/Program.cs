using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.SimpleNotificationService;
using CustomerManager.Api.Extensions;
using CustomerManager.Infra.Database;
using Microsoft.EntityFrameworkCore;
using Serilog;

// Permite DateTime sem Kind (Unspecified) em colunas timestamp with time zone do PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.OpenTelemetry();
});



builder.Services.ConfigureServices(builder.Configuration);
var app = builder.Build();

// Roda migrations automaticamente ao subir
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CustomerContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.Configure();

app.Run();
