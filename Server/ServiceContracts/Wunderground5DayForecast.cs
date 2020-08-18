using System;
using Newtonsoft.Json;

namespace Server.ServiceContracts
{
    public class Wunderground5DayForecast
    {
        [JsonProperty("dayOfWeek")] public string[] DayOfWeek { get; set; }
        [JsonProperty("expirationTimeUtc")] public long ExpirationTimeUtc { get; set; }
        [JsonProperty("moonPhase")] public string[] MoonPhase { get; set; }
        [JsonProperty("moonPhaseCode")] public string[] MoonPhaseCode { get; set; }
        [JsonProperty("moonPhaseDay")] public int[] MoonPhaseDay { get; set; }
        [JsonProperty("moonriseTimeLocal")] public DateTime[] MoonriseTimeLocal { get; set; }
        [JsonProperty("moonriseTimeUtc")] public long[] MoonriseTimeUtc { get; set; }
        [JsonProperty("moonsetTimeLocal")] public DateTime[] MoonsetTimeLocal { get; set; }
        [JsonProperty("moonsetTimeUtc")] public long[] MoonsetTimeUtc { get; set; }
        [JsonProperty("narrative")] public string[] Narrative { get; set; }
        [JsonProperty("qpf")] public float[] Qpf { get; set; }
        [JsonProperty("qpfSnow")] public float[] QpfSnow { get; set; }
        [JsonProperty("sunriseTimeLocal")] public DateTime[] SunriseTimeLocal { get; set; }
        [JsonProperty("sunriseTimeUtc")] public long[] SunriseTimeUtc { get; set; }
        [JsonProperty("sunsetTimeLocal")] public DateTime[] SunsetTimeLocal { get; set; }
        [JsonProperty("sunsetTimeUtc")] public long[] SunsetTimeUtc { get; set; }
        [JsonProperty("temperatureMax")] public int[] TemperatureMax { get; set; }
        [JsonProperty("temperatureMin")] public int[] TemperatureMin { get; set; }
        [JsonProperty("validTimeLocal")] public DateTime[] ValidTimeLocal { get; set; }
        [JsonProperty("validTimeUtc")] public long[] ValidTimeUtc { get; set; }
        [JsonProperty("daypart")] public DayPart[] DayPart { get; set; }
    }
    public class DayPart 
    {
        [JsonProperty("cloudCover")] public int[] CloudCover { get; set; }
        [JsonProperty("dayOrNight")] public string[] DayOrNight { get; set; }
        [JsonProperty("daypartName")] public string[] DaypartName { get; set; }
        [JsonProperty("iconCode")] public int[] IconCode { get; set; }
        [JsonProperty("iconCodeExtend")] public int[] IconCodeExtend { get; set; }
        [JsonProperty("narrative")] public string[] Narrative { get; set; }
        [JsonProperty("precipChance")] public int[] PrecipChance { get; set; }
        [JsonProperty("precipType")] public string[] PrecipType { get; set; }
        [JsonProperty("qpf")] public float[] Qpf { get; set; }
        [JsonProperty("qpfSnow")] public float[] QpfSnow { get; set; }
        [JsonProperty("qualifierCode")] public string[] QualifierCode { get; set; }
        [JsonProperty("qualifierPhrase")] public string[] QualifierPhrase { get; set; }
        [JsonProperty("relativeHumidity")] public int[] RelativeHumidity { get; set; }
        [JsonProperty("snowRange")] public float[] SnowRange { get; set; }
        [JsonProperty("temperature")] public int[] Temperature { get; set; }
        [JsonProperty("temperatureHeatIndex")] public int[] TemperatureHeatIndex { get; set; }
        [JsonProperty("temperatureWindChill")] public int[] TemperatureWindChill { get; set; }
        [JsonProperty("thunderCategory")] public string[] ThunderCategory { get; set; }
        [JsonProperty("thunderIndex")] public int[] ThunderIndex { get; set; }
        [JsonProperty("uvDescription")] public string[] UvDescription { get; set; }
        [JsonProperty("uvIndex")] public int[] UvIndex { get; set; }
        [JsonProperty("windDirection")] public int[] WindDirection { get; set; }
        [JsonProperty("windDirectionCardinal")] public string[] WindDirectionCardinal { get; set; }
        [JsonProperty("windPhrase")] public string[] WindPhrase { get; set; }
        [JsonProperty("windSpeed")] public int[] WindSpeed { get; set; }
        [JsonProperty("wxPhraseLong")] public string[] WxPhraseLong { get; set; }
        [JsonProperty("wxPhraseShort")] public string[] WxPhraseShort { get; set; }
    }
}