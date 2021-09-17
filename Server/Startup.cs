using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.BackgroundWorkers;
using Server.Data;
using Server.Data.Repositories;
using Server.Helpers;
using Server.Services;

namespace Server
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            EnsureRequiredEnvironmentVariables();

            // With the use of App.Metrics.AspNetCore.Health.Endpoints we will now expose a http '/health' endpoint which will return the status of this service
            services.AddHealthEndpoints();

            services.AddDbContext<DataContext>(options =>
                options.UseSqlite($"Data Source={FileHelpers.SQLiteDBFilePath}"));

            services.AddScoped<ISubscriptionRepo, SubscriptionRepo>();

            // NewSubscriptionsWorker will constantly listen for new subscriptions in the background
            services.AddHostedService<SubscriptionWorker>();

            services.AddSingleton<IWundergroundService, WundergroundService>();
            services.AddHttpClient("WundergroundCurrent", client =>
            {
                client.BaseAddress = new Uri("https://api.weather.com/v2/pws/observations/current");
                client.DefaultRequestHeaders.Add("User-Agent", "SDUWeatherStationDriver");
            });
            services.AddHttpClient("WundergroundForecast", client => 
            {
                client.BaseAddress = new Uri("https://api.weather.com/v3/wx/forecast/daily/5day");
                client.DefaultRequestHeaders.Add("User-Agent", "SDUWeatherStationDriver");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            app.UseHealthEndpoint();
            await UpdateDatabase(app, logger);
        }

        public void EnsureRequiredEnvironmentVariables()
        {
            Console.ForegroundColor = ConsoleColor.Red;

            if (Environment.GetEnvironmentVariable("WEATHERSTDR_WUND_API_KEY") == null)
            {
                Console.WriteLine("'WEATHERSTDR_WUND_API_KEY' not found");
                System.Environment.Exit(1);
            }

            Console.ResetColor();
        }

        private async Task UpdateDatabase(IApplicationBuilder app, ILogger<Startup> logger)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<DataContext>())
                {
                    // Npgsql resiliency strategy does not work with Database.EnsureCreated() and Database.Migrate().
                    // Therefore a retry pattern is implemented for this purpose 
                    // if database connection is not ready it will retry 3 times before finally quiting
                    var retryCount = 3;
                    var currentRetry = 0;
                    while (true)
                    {
                        try
                        {
                            logger.LogInformation("Attempting database migration");

                            context.Database.Migrate();

                            logger.LogInformation("Database migration & connection successful");

                            SubscriptionWorkerHelpers.DatabaseReady = true;

                            break; // just break if migration is successful
                        }
                        catch (Exception)
                        {
                            logger.LogError("Database migration failed. Retrying in 5 seconds ...");

                            currentRetry++;

                            if (currentRetry == retryCount) // Here it is possible to check the type of exception if needed with an OR. And exit if it's a specific exception.
                            {
                                // We have tried as many times as retryCount specifies. Now we throw it and exit the application
                                logger.LogCritical($"Database migration failed after {retryCount} retries");
                                throw;
                            }

                        }
                        // Waiting 5 seconds before trying again
                        await Task.Delay(TimeSpan.FromSeconds(5));
                    }
                }
            }
        }
    }
}
