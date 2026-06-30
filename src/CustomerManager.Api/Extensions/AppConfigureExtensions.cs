using CustomerManager.Api.Endpoints;
using CustomerManager.Infra.Database;
using Microsoft.EntityFrameworkCore;

namespace CustomerManager.Api.Extensions
{
    public static class AppConfigureExtensions
    {
        public static async Task Configure(this WebApplication app)
        {
            app.UseHttpsRedirection();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                    c.RoutePrefix = "";
                });

                using var scope = app.Services.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<CustomerContext>();

                if (db.Database.IsRelational())
                {
                    var retries = 0;
                    while (true)
                    {
                        try
                        {
                            await db.Database.MigrateAsync();
                            break;
                        }
                        catch (Exception) when (retries++ < 5)
                        {
                            await Task.Delay(TimeSpan.FromSeconds(3));
                        }
                    }
                }
            }

            app.MapCustomerManagerEndpoints();
        }
    }
}
