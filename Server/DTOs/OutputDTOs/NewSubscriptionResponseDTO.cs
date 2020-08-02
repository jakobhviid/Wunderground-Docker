using Newtonsoft.Json;

namespace Server.DTOs.OutputDTOs
{
    public class NewSubscriptionResponseDTO
    {
        [JsonProperty(Required = Required.Always)]
        public int ResponseStatusCode { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Message { get; set; }
    }
}