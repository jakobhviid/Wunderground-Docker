using Newtonsoft.Json;

namespace Server.DTOs.InputDTOs
{
    public class NewSubscriptionRequestDTO
    {
        [JsonProperty(Required = Required.Always)]
        public string StationId { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int IntervalSeconds { get; set; }
    }
}