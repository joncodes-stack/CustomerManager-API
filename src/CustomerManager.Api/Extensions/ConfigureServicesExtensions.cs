using CustomerManager.Api.Filters;
using CustomerManager.Ioc;

namespace CustomerManager.Api.Extensions
{
    public static class ConfigureServicesExtensions
    {
        public static void ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication();
            services.AddAuthorization();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
            });

            services.AddScoped(typeof(ValidationFilter<>));
            services.ConfigureAppDependencies(configuration);
        }
    }
}
