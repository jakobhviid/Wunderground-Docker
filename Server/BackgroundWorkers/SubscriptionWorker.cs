using System.Net.Http;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Server.Services;
using Server.Data.Models;
using Server.Data.Repositories;
using Server.DTOs.InputDTOs;
using Server.Helpers;
using Newtonsoft.Json.Serialization;

namespace Server.BackgroundWorkers
{
    public class SubscriptionWorker : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<SubscriptionWorker> _logger;
        private readonly IProducer<Null, string> _producer;

        public SubscriptionWorker(ILogger<SubscriptionWorker> logger, IServiceProvider services)
        {
            _services = services;
            _logger = logger;

            var producerConfig = new ProducerConfig { Acks = Acks.Leader };
            KafkaHelpers.SetKafkaConfigKerberos(producerConfig);

            _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await SubscriptionWorkerHelpers.EnsureCreatedTopics(_logger);

            using (var scope = _services.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<ISubscriptionRepo>();

                // waiting for database to be ready
                while (!SubscriptionWorkerHelpers.DatabaseReady) {
                    await Task.Delay(TimeSpan.FromMilliseconds(200)); 
                }
                // Loading subscriptions from the database (if any)
                var currentConditionSubscriptions = await repo.GetAllCurrentConditionSubScriptions();
                var forecastSubscriptions = await repo.GetAllForecastSubScriptions();

                // Loading subscriptions from environment (if any)
                List<CurrentConditionSubscription> initialCurrentConditionSubscriptions;
                List<ForecastSubscription> initialForecastSubscriptions;
                SubscriptionWorkerHelpers.InitialSubscriptionsLoad(_logger, out initialCurrentConditionSubscriptions, out initialForecastSubscriptions);

                // Adding subscriptions from environment to database for future startups.
                await repo.AddCurrentConditionSubscriptions(initialCurrentConditionSubscriptions);
                await repo.AddForecastSubscriptions(initialForecastSubscriptions);

                // Adding subscriptions from environment to list of subcsriptions from database
                currentConditionSubscriptions.AddRange(initialCurrentConditionSubscriptions);
                forecastSubscriptions.AddRange(initialForecastSubscriptions);

                // Start current condition subscriptions
                foreach (var currentSubscription in currentConditionSubscriptions)
                {
                    // This timer should run in the background. So the result is discarded with '_'
                    _ = Task.Run(() => StartTimer(currentSubscription));
                }

                // Start forecast subscriptions
                foreach (var forecastSubscription in forecastSubscriptions)
                {
                    // This timer should run in the background. So the result is discarded with '_'
                    _ = Task.Run(() => StartTimer(forecastSubscription));
                }

                ListenForSubscriptionsActions(stoppingToken, currentConditionSubscriptions, forecastSubscriptions);
            }
        }

        public async void ListenForSubscriptionsActions(CancellationToken stoppingToken, List<CurrentConditionSubscription> currentConditionSubscriptions, List<ForecastSubscription> forecastSubscriptions)
        {
            // Listening and waiting for new subscription requests from kafka
            var consumerConfig = new ConsumerConfig
            {
                GroupId = "weather-station-new-subscription-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
            };

            KafkaHelpers.SetKafkaConfigKerberos(consumerConfig);

            using (var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build())
            {
                using (var scope = _services.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<ISubscriptionRepo>();

                    consumer.Subscribe(KafkaHelpers.SubscriptionActionsTopic);

                    _logger.LogInformation("Listening for subscription action requests");
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            // consumer does not have an async method. So it is wrapped in a task, so that the rest of the application doesn't hang here
                            var consumeResult = await Task.Factory.StartNew(() => consumer.Consume(stoppingToken));

                            var messageJsonString = consumeResult.Message.Value;

                            // Checking format required.
                            SubscriptionRequestDTO request = JsonConvert.DeserializeObject<SubscriptionRequestDTO>(messageJsonString);

                            _logger.LogInformation("A subscription action request has arrived");
                            switch (request.Action)
                            {
                                case SubscriptionAction.CREATECURRENTCONDITION:
                                    var currentConditionRequest = JsonConvert.DeserializeObject<NewCurrentConditionSubscriptionRequestDTO>(messageJsonString);

                                    var currentConditionSubscription = new CurrentConditionSubscription
                                    {
                                        StationId = currentConditionRequest.StationId,
                                        IntervalSeconds = currentConditionRequest.IntervalSeconds
                                    };

                                    // persisting the newly added subscription
                                    await repo.AddCurrentConditionSubscription(currentConditionSubscription);

                                    // adding it to the list of subscriptions
                                    currentConditionSubscriptions.Add(currentConditionSubscription);

                                    // starting the subscription. I want this to run in the background. So I discard the result  with '_'
                                    _ = Task.Run(() => StartTimer(currentConditionSubscription));
                                    break;
                                case SubscriptionAction.CREATEFORECAST:
                                    var forecastSubscriptionRequest = JsonConvert.DeserializeObject<ForecastSubscription>(messageJsonString);

                                    var forecastSubscription = new ForecastSubscription
                                    {
                                        GeoCode = forecastSubscriptionRequest.GeoCode,
                                        IntervalSeconds = forecastSubscriptionRequest.IntervalSeconds
                                    };

                                    // persisting the newly added subscription
                                    await repo.AddForecastSubscription(forecastSubscription);

                                    // adding it to the list of subscriptions
                                    forecastSubscriptions.Add(forecastSubscription);

                                    // starting the subscription. I want this to run in the background. So I discard the result  with '_'
                                    _ = Task.Run(() => StartTimer(forecastSubscription));
                                    break;
                                case SubscriptionAction.DELETE:
                                    // TODO: Delete
                                    break;
                                default:
                                    throw new Newtonsoft.Json.JsonException("Invalid Subscription action");
                            }
                        }
                        catch (ConsumeException ex)
                        {
                            _logger.LogError(ex.Error.ToString());
                        }
                        // If the request from kafka is not correctly formatted an error will be thrown and catched here
                        catch (Newtonsoft.Json.JsonException ex)
                        {
                            _logger.LogError(ex.Message);
                        }
                        // Cancelled background worker
                        catch (OperationCanceledException)
                        {
                            consumer.Close();
                        }
                    }
                }
            }
            // If we reach this point it means the application is shutting down. Therefore we clean up
            _logger.LogInformation("Cleaning up");
            _producer.Flush(TimeSpan.FromSeconds(5));
            _producer.Dispose();
        }

        public async Task FetchData(CurrentConditionSubscription subscription)
        {
            try
            {
                using var scope = _services.CreateScope();
                var wundergroundService = scope.ServiceProvider.GetRequiredService<IWundergroundService>();
                var data = await wundergroundService.GetCurrentConditionsAsync(subscription.StationId);

                await KafkaHelpers.SendMessageAsync(KafkaHelpers.WeatherDataTopic, data, _producer);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"Fetch for StationId={subscription.StationId} threw an error: {e.Message}");
            }
        }

        public void StartTimer(CurrentConditionSubscription subscription)
        {
            object timerState = new object();

            // Start an accurate timer based on subscriptions.intervalseconds
            Timer timer = new Timer(async (timerState) =>
            {
                await FetchData(subscription);
            }, timerState, 0, (int)TimeSpan.FromSeconds(subscription.IntervalSeconds).TotalMilliseconds);
        }

        public async Task FetchData(ForecastSubscription subscription)
        {
            try
            {
                using var scope = _services.CreateScope();
                var wundergroundService = scope.ServiceProvider.GetRequiredService<IWundergroundService>();
                var data = await wundergroundService.Get5DayForecast(subscription.GeoCode);

                await KafkaHelpers.SendMessageAsync(KafkaHelpers.WeatherDataTopic, data, _producer);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"Fetch for GeoCode={subscription.GeoCode} threw an error: {e.Message}");
            }
        }

        public void StartTimer(ForecastSubscription subscription)
        {
            object timerState = new object();

            // Start an accurate timer based on subscriptions.intervalseconds
            Timer timer = new Timer(async (timerState) =>
            {
                await FetchData(subscription);
            }, timerState, 0, (int)TimeSpan.FromSeconds(subscription.IntervalSeconds).TotalMilliseconds);
        }
    }
}