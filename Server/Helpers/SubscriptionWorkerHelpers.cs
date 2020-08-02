using System.Linq;
using System.Net.Sockets;
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
        public static List<Subscription> InitialSubscriptionsLoad(ILogger logger)
        {
            var subscriptions = new List<Subscription>();
            if (!File.Exists(FileHelpers.InitialSubscriptionsFileCheckPath))
            {

                var initialSubscriptionsEnv = Environment.GetEnvironmentVariable("WEATHERSTDR_INITIAL_SUBSCRIPTIONS");
                // Example of initialSubscriptions string "StationId=IODENS3;Interval=10,StationId=IKASTR4;Interval=5"
                if (initialSubscriptionsEnv != null)
                {
                    var initialSubscriptions = initialSubscriptionsEnv.Split(",");
                    foreach (var subscription in initialSubscriptions)
                    {
                        var arguments = subscription.Split(";");
                        if (!arguments.Any(a => a.StartsWith("StationId")) || !arguments.Any(a => a.StartsWith(("Interval"))))
                        {
                            logger.LogWarning($"'WEATHERSTDR_INITIAL_SUBSCRIPTIONS' Has not followed the correct format for subscription {initialSubscriptions[0]}");
                            break;
                        }
                        else
                        {
                            string stationId = arguments[0][10..]; // Cutting out "StationId="
                            int intervalSeconds;
                            var intervalConversionSucces = int.TryParse(arguments[1][9..], out intervalSeconds); // Cutting out "Interval="
                            if (!intervalConversionSucces) logger.LogWarning($"'WEATHERSTDR_INITIAL_SUBSCRIPTIONS' Has not followed the correct format for subscription {subscription}. Can't convert interval to a number");
                            subscriptions.Add(new Subscription
                            {
                                StationId = stationId,
                                IntervalSeconds = intervalSeconds
                            });
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
            return subscriptions;
        }

        public async static Task EnsureCreatedTopics(ILogger logger)
        {
            // The admin client contains metadata about the cluster, groups and topics.
            using var adminClient = new AdminClientBuilder(new AdminClientConfig { BootstrapServers = KafkaHelpers.BootstrapServers }).Build();

            var clusterMetaData = adminClient.GetMetadata(TimeSpan.FromSeconds(10)); // 10 seconds timeout

            var amountOfBrokersInCluster = clusterMetaData.Brokers.Count();

            // List of all topics in the cluster
            var clusterTopics = clusterMetaData.Topics.Select(t => t.Topic);

            // If the amount of brokers is less than or equal to 1, replication should be 1. Otherwise if amount of brokers is 3 for example, replication should be 2.
            var amountOfReplicationForCluster = (short)(amountOfBrokersInCluster <= 1 ? 1 : amountOfBrokersInCluster - 1);

            // Testing if topics contains the two topics we use
            if (!clusterTopics.Contains(KafkaHelpers.NewSubscriptionsTopic))
            {
                await CreateTopic(adminClient, logger, KafkaHelpers.NewSubscriptionsTopic, replicationFactor: amountOfReplicationForCluster);
            }
            if (!clusterTopics.Contains(KafkaHelpers.WeatherDataTopic))
            {
                await CreateTopic(adminClient, logger, KafkaHelpers.WeatherDataTopic, replicationFactor: amountOfReplicationForCluster);
            }

            logger.LogInformation("Kafka cluster succesfully configured");

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