using System.Runtime.Intrinsics.X86;
using System.Net.Http;
using System.Net;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.IO;
using Server.ServiceContracts;
using Polly.Retry;
using Newtonsoft.Json;
using Polly;

namespace Server.Services
{
    public class WundergroundService : IWundergroundService
    {
        private string WundergroundAPIKey = Environment.GetEnvironmentVariable("WEATHERSTDR_WUND_API_KEY");
        private const int MaxRetries = 3;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AsyncRetryPolicy _retryPolicy;

        public WundergroundService(IHttpClientFactory factory)
        {
            _httpClientFactory = factory;
            _retryPolicy = Policy.Handle<HttpRequestException>()
                .WaitAndRetryAsync(MaxRetries, times =>
                TimeSpan.FromMilliseconds(times * 200));
        }

        public async Task<CurrentConditions> GetCurrentConditionsAsync(string stationId)
        {
            var client = _httpClientFactory.CreateClient("Wunderground");

            return await _retryPolicy.ExecuteAsync(async () =>
            {
                var response = await client.GetAsync(
                    $"observations/current?stationId={stationId.Trim()}&format=json&units=m&apiKey={WundergroundAPIKey.Trim()}");

                response.EnsureSuccessStatusCode(); // Throws error if not successful

                if (
                    response.StatusCode == HttpStatusCode.NotFound ||
                    response.StatusCode == HttpStatusCode.NoContent
                    ) throw new HttpRequestException(response.StatusCode.ToString());


                string responseString = await response.Content.ReadAsStringAsync();

                // safe guarding
                if (responseString == null) throw new HttpRequestException("Improper response format");

                try
                {
                    var serialisedResponse = JsonConvert.DeserializeObject<WundergroundCurrentConditions>(responseString);
                    // safe guarding
                    if (serialisedResponse == null) throw new HttpRequestException("Improper response format");

                    return serialisedResponse.Observations[0];
                }
                catch (JsonSerializationException ex)
                {
                    throw new HttpRequestException(ex.Message);
                }
            });
        }
    }
}