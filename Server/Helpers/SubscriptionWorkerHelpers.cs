using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using Server.Data.Models;
using Confluent.Kafka;
using Newtonsoft.Json;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Server.Helpers
{
    public static class SubscriptionWorkerHelpers
    {
        public static bool DatabaseReady { get; set; }
        // This method will load subscriptions from the environment variable "WEATHERSTDR_INITIAL_SUBSCRIPTIONS"
        // Example of environment variable value: "StationId=IODENS3;Interval=10|StationId=IKASTR4;Interval=5|GeoCode=55.3733417,10.4079504;Interval=10"
        public static void InitialSubscriptionsLoad(ILogger logger, out List<CurrentConditionSubscription> currentConditionSubscriptions, out List<ForecastSubscription> forecastSubscriptions)
        {
            currentConditionSubscriptions = new List<CurrentConditionSubscription>();
            forecastSubscriptions = new List<ForecastSubscription>();

            if (!File.Exists(FileHelpers.InitialSubscriptionsFileCheckPath))
            {
                var initialSubscriptionsEnv = Environment.GetEnvironmentVariable("WEATHERSTDR_INITIAL_SUBSCRIPTIONS");
                if (initialSubscriptionsEnv != null)
                {
                    var initialSubscriptions = initialSubscriptionsEnv.Split("|");
                    foreach (var subscription in initialSubscriptions)
                    {
                        try
                        {
                            var arguments = subscription.Split(";");

                            int intervalSeconds;
                            var intervalConversionSucces = int.TryParse(arguments[1][9..], out intervalSeconds); // Cutting out "Interval="
                            if (!intervalConversionSucces) throw new FormatException();

                            if (arguments[0].StartsWith("StationId"))
                            {
                                string stationId = arguments[0][10..]; // Cutting out "StationId="

                                currentConditionSubscriptions.Add(new CurrentConditionSubscription
                                {
                                    StationId = stationId,
                                    IntervalSeconds = intervalSeconds
                                });
                            }
                            else if (arguments[0].StartsWith("GeoCode"))
                            {
                                string geoCode = arguments[0][8..]; // Cutting out "GeoCode="

                                forecastSubscriptions.Add(new ForecastSubscription
                                {
                                    GeoCode = geoCode,
                                    IntervalSeconds = intervalSeconds
                                });
                            }
                        }
                        catch (Exception e)
                        {
                            logger.LogWarning($"'WEATHERSTDR_INITIAL_SUBSCRIPTIONS' Has not followed the correct format");
                            Console.WriteLine(e.Message);
                        }
                    }

                    // Creating a file to indicate in future runs in the same container that the initial subscriptions already has been added to the database.
                    FileHelpers.CreateInitialSubscriptionsFile();
                }
                else
                {
                    logger.LogInformation("'WEATHERSTDR_INITIAL_SUBSCRIPTIONS' has not been provided");
                }
            }
        }

        public async static Task EnsureCreatedTopics(ILogger logger)
        {
            try
            {
                // The admin client contains metadata about the cluster, groups and topics.
                using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = KafkaHelpers.BootstrapServers }).Build();

                var clusterMetaData = adminClient.GetMetadata(TimeSpan.FromSeconds(10)); // 10 seconds timeout

                var amountOfBrokersInCluster = clusterMetaData.Brokers.Count();

                // List of all topics in the cluster
                var clusterTopics = clusterMetaData.Topics.Select(t => t.Topic);

                // If the amount of brokers is less than or equal to 1, replication should be 1. Otherwise if amount of brokers is 3 for example, replication should be 2.
                var amountOfReplicationForCluster = (short)(amountOfBrokersInCluster <= 1 ? 1 : amountOfBrokersInCluster - 1);

                // Testing if kafka contains the two topics we use
                if (!clusterTopics.Contains(KafkaHelpers.SubscriptionActionsTopic))
                {
                    await CreateTopic(adminClient, logger, KafkaHelpers.SubscriptionActionsTopic, replicationFactor: amountOfReplicationForCluster);
                }
                if (!clusterTopics.Contains(KafkaHelpers.WeatherDataTopic))
                {
                    await CreateTopic(adminClient, logger, KafkaHelpers.WeatherDataTopic, replicationFactor: amountOfReplicationForCluster);
                }

                logger.LogInformation("Kafka cluster succesfully configured");

            }
            catch (Confluent.Kafka.KafkaException e)
            {
                logger.LogError(e.Message);
            }
        }

        private async static Task CreateTopic(IAdminClient adminClient, ILogger logger, string topicName, int partitions = 1, short replicationFactor = 1)
        {
            var newTopic = new TopicSpecification
            {
                Name = topicName,
                NumPartitions = partitions,
                ReplicationFactor = replicationFactor
            };

            try
            {
                await adminClient.CreateTopicsAsync(new TopicSpecification[] { newTopic });
                logger.LogInformation($"Created topic: '{topicName}' with {partitions} partition{(partitions > 1 ? "s" : "")} and {replicationFactor} in replicationFactor");
            }
            catch (CreateTopicsException e)
            {
                var errorResult = e.Results[0];
                logger.LogError($"Error creating {errorResult.Topic}: {errorResult.Error.Reason} ");
            }
        }
    }
}