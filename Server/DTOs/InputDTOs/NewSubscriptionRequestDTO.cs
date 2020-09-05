using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Server.DTOs.InputDTOs
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SubscriptionAction
    {
        CREATECURRENTCONDITION,
        CREATEFORECAST,
        DELETE
    }

    public class SubscriptionRequestDTO
    {
        [JsonProperty(Required = Required.Always)]
        public SubscriptionAction Action { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int IntervalSeconds { get; set; }
    }

    public class NewCurrentConditionSubscriptionRequestDTO
    {
        public SubscriptionAction Action = SubscriptionAction.CREATECURRENTCONDITION;
        [JsonProperty(Required = Required.Always)]
        public string StationId { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int IntervalSeconds { get; set; }
    }

    public class NewForecastSubscriptionRequestDTO
    {
        public SubscriptionAction Action = SubscriptionAction.CREATEFORECAST;
        [JsonProperty(Required = Required.Always)]
        public string GeoCode { get; set; }
        
        [JsonProperty(Required = Required.Always)]
        public int IntervalSeconds { get; set; }
    }
}
