using System;
using System.Net;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Server.Helpers
{
    public class KafkaHelpers
    {
        public static readonly string BootstrapServers = Environment.GetEnvironmentVariable("WEATHERSTDR_KAFKA_URL") ?? "kafka1.cfei.dk:9092,kafka2.cfei.dk:9092,kafka3.cfei.dk:9092";
        public static readonly string SubscriptionActionsTopic = Environment.GetEnvironmentVariable("WEATHERSTDR_SUBSCRIPTION_ACTION_TOPIC") ?? "subscriptions-actions";
        // public static readonly string NewSubscriptionsResponseTopic = Environment.GetEnvironmentVariable("WEATHERSTDR_NEW_SUBSCRIPTION_RESPONSE_TOPIC") ?? "new-subscriptions-response";
        public static readonly string WeatherDataTopic = Environment.GetEnvironmentVariable("WEATHERSTDR_WEATHER_DATA_TOPIC") ?? "weather-data";

        public static async Task SendMessageAsync(string topic, object messageToSerialize, IProducer<Null, string> p)
        {
            try
            {
                var deliveryReport = await p.ProduceAsync(
                    topic, new Message<Null, string> { Value = JsonConvert.SerializeObject(messageToSerialize) });

                Console.WriteLine($"delivered to: {deliveryReport.TopicPartitionOffset}");
            }
            catch (ProduceException<Null, string> e)
            {
                Console.WriteLine($"Failed to deliver message: {e.Message} [{e.Error.Code}]");
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine($"Error {e.Message}");
            }
            catch (JsonSerializationException e)
            {
                Console.WriteLine($"Error serializing {e.Message}");
            }
        }

        public static ClientConfig SetKafkaConfigKerberos(ClientConfig config)
        {
            config.BootstrapServers = BootstrapServers;

            var saslEnabled = Environment.GetEnvironmentVariable("WEATHERSTDR_KERBEROS_PUBLIC_URL");

            if (saslEnabled != null)
            {
                config.SecurityProtocol = SecurityProtocol.SaslPlaintext;
                config.SaslKerberosServiceName = Environment.GetEnvironmentVariable("WEATHERSTDR_BROKER_KERBEROS_SERVICE_NAME") ?? "kafka";
                config.SaslKerberosKeytab = Environment.GetEnvironmentVariable("KEYTAB_LOCATION");

                // If the principal has been provided through volumes. The environment variable 'WEATHERSTDR_KERBEROS_PRINCIPAL' will be set. If not 'WEATHERSTDR_KERBEROS_API_SERVICE_USERNAME' will be set.
                var principalName = Environment.GetEnvironmentVariable("WEATHERSTDR_KERBEROS_PRINCIPAL") ?? Environment.GetEnvironmentVariable("WEATHERSTDR_KERBEROS_API_SERVICE_USERNAME");
                config.SaslKerberosPrincipal = principalName;
            }

            return config;
        }
    }
}
