using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Server.ServiceContracts
{
    public class WundergroundCurrentConditions
    {
        [JsonProperty("observations")] public IList<CurrentConditions> Observations { get; set; }
    }

    public class CurrentConditions
    {
        [JsonProperty("stationId")] public string StationId { get; set; }
        [JsonProperty("obsTimeUtc")] public DateTime ObsTimeUtc { get; set; }
        [JsonProperty("obsTimeLocal")] public DateTime ObsTimeLocal { get; set; }
        [JsonProperty("neighborhood")] public string Neighborhood { get; set; }
        [JsonProperty("softwareType")] public string SoftwareType { get; set; }
        [JsonProperty("country")] public string Country { get; set; }
        [JsonProperty("solarRadiation")] public float SolarRadiation { get; set; }
        [JsonProperty("lon")] public float Lon { get; set; }
        [JsonProperty("lat")] public float Lat { get; set; }
        [JsonProperty("realtimeFrequency")] public float? RealtimeFrequency { get; set; }
        [JsonProperty("epoch")] public long Epoch { get; set; }
        [JsonProperty("uv")] public float Uv { get; set; }
        [JsonProperty("winddir")] public float Winddir { get; set; }
        [JsonProperty("humidtiy")] public float Humidtiy { get; set; }
        [JsonProperty("qcStatus")] public int QcStatus { get; set; }
        [JsonProperty("metric")] public CurrentConditionsMetric Metric { get; set; }
    }

    public class CurrentConditionsMetric
    {
        [JsonProperty("temp")] public float Temp { get; set; }
        [JsonProperty("heatIndex")] public float HeatIndex { get; set; }
        [JsonProperty("dewpt")] public float Dewpt { get; set; }
        [JsonProperty("windChill")] public float WindChill { get; set; }
        [JsonProperty("windSpeed")] public float WindSpeed { get; set; }
        [JsonProperty("windGust")] public float WindGust { get; set; }
        [JsonProperty("pressure")] public float Pressure { get; set; }
        [JsonProperty("precipRate")] public float PrecipRate { get; set; }
        [JsonProperty("precipTotal")] public float PrecipTotal { get; set; }
        [JsonProperty("elev")] public float Elev { get; set; }
    }
}