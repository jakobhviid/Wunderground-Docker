using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Server.DTOs.InputDTOs
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SubscriptionAction
    {
        CREATE,
        DELETE
    }
    public class SubscriptionRequestDTO
    {
        [JsonProperty(Required = Required.Always)]
        public SubscriptionAction Action { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string StationId { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int IntervalSeconds { get; set; }
    }
}