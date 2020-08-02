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

                // this list will hold all subscriptions and if the container restarts the subscriptions are fetched from the sqlite database.
                List<Subscription> subscriptions = await repo.GetAllSubScriptions();

                var initialSubscriptions = SubscriptionWorkerHelpers.InitialSubscriptionsLoad(_logger);

                subscriptions.AddRange(initialSubscriptions);

                await repo.AddSubscriptions(initialSubscriptions);

                foreach (var subscription in subscriptions)
                {
                    // This timer should run in the background. So the result is discarded with '_'
                    _ = Task.Run(() => StartTimer(subscription));
                }

                ListenForNewSubscriptions(stoppingToken, subscriptions);
            }
        }

        public async void ListenForNewSubscriptions(CancellationToken stoppingToken, List<Subscription> subscriptions)
        {
            // Listening and waiting for new subscriptions
            var consumerConfig = new ConsumerConfig
            {
                GroupId = "new-subscriptions-group",
                AutoOffsetReset = AutoOffsetReset.Earliest,
            };

            KafkaHelpers.SetKafkaConfigKerberos(consumerConfig);

            using (var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build())
            {
                using (var scope = _services.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<ISubscriptionRepo>();

                    consumer.Subscribe(KafkaHelpers.NewSubscriptionsTopic);

                    _logger.LogInformation("Listening for new subscription requests");
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        try
                        {
                            // consumer does not have an async method. So it is wrapped in a task, so that the rest of the application doesn't hang here
                            var consumeResult = await Task.Factory.StartNew(() => consumer.Consume(stoppingToken));

                            var messageJsonString = consumeResult.Message.Value;

                            // Checking format required.
                            NewSubscriptionRequestDTO request = JsonConvert.DeserializeObject<NewSubscriptionRequestDTO>(messageJsonString);

                            _logger.LogInformation("A new subscription request has arrived");

                            var subscription = new Subscription
                            {
                                StationId = request.StationId,
                                IntervalSeconds = request.IntervalSeconds
                            };
                            // persisting the newly added subscription
                            await repo.AddSubscription(subscription);

                            // adding it to the list of subscriptions
                            subscriptions.Add(subscription);

                            // starting the subscription. I want this to run in the background. So I discard the result  with '_'
                            _ = Task.Run(() => StartTimer(subscription));
                        }
                        catch (ConsumeException ex)
                        {
                            _logger.LogError(ex.Error.ToString());
                        }
                        catch (Newtonsoft.Json.JsonException ex)
                        {
                            _logger.LogError(ex.Message);
                        }
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

        public async Task FetchData(Subscription subscription)
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

        public void StartTimer(Subscription subscription)
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