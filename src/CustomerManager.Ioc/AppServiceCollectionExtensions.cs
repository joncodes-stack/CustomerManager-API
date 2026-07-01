using Amazon.SimpleNotificationService;
using CustomerManager.Application.Handlers;
using CustomerManager.Application.Interfaces;
using CustomerManager.Application.Validators;
using CustomerManager.Domain.Interfaces.Repositories;
using CustomerManager.Domain.Interfaces.Services;
using CustomerManager.Infra.Database;
using CustomerManager.Infra.Messaging;
using CustomerManager.Infra.Repositories;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CustomerManager.Ioc
{
    public static class AppServiceCollectionExtensions
    {
        public static void ConfigureAppDependencies(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddValidatorsFromAssemblyContaining<IValidators>();

            services.AddDbContext<CustomerContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("Default"),
                    npgsql => npgsql.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorCodesToAdd: null)));

            services.AddSingleton<IAmazonSimpleNotificationService>(_ =>
            {
                var serviceUrl = configuration["AWS:SNS:ServiceUrl"] ?? "http://localhost:4566";
                var config = new AmazonSimpleNotificationServiceConfig
                {
                    ServiceURL = serviceUrl,
                    AuthenticationRegion = "us-east-1"
                };
                return new AmazonSimpleNotificationServiceClient(
                    new Amazon.Runtime.BasicAWSCredentials("test", "test"),
                    config
                );
            });

            // Distributed Cache (Redis)
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            });

            // Event Publisher
            services.AddScoped<ICustomerEventPublisher, CustomerEventPublisher>();

            // Handlers
            services.AddScoped<ICreateCustomerHandler, CreateCustomerHandler>();
            services.AddScoped<IGetAllCustomerHandler, GetAllCustomersHandler>();
            services.AddScoped<IGetCustomerByIdHandler, GetCustomerByIdHandler>();
            services.AddScoped<IGetCustomerByCPFHandler, GetCustomerByCpfHandler>();
            services.AddScoped<IUpdateCustomerHandler, UpdateCustomerHandler>();
            services.AddScoped<IDeleteCustomerHandler, DeleteCustomerHandler>();

            // Repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();
        }
    }
}
