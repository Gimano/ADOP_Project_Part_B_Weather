using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json; 
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;

using ADOP_Project_Part_B_Weather.Models;
using System.Collections.Concurrent;

namespace ADOP_Project_Part_B_Weather.Services
{
    public class OpenWeatherService
    {
        HttpClient httpClient = new HttpClient();

        ConcurrentDictionary<(double, double, string), Forecast> cachedGeoForecasts = new ConcurrentDictionary<(double, double, string), Forecast>();
        ConcurrentDictionary<(string, string), Forecast> cachedCityForecasts = new ConcurrentDictionary<(string, string), Forecast>();
        //Your API Key
        readonly string apiKey = "a1720388be09d3ae9a7780925427f106";

        public async Task<Forecast> GetForecastAsync(string City)
        {
            Forecast forecast = null;
            var key = (City, DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

            if (!cachedCityForecasts.TryGetValue(key, out forecast))
            {
                //https://openweathermap.org/current
                var language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                var uri = $"https://api.openweathermap.org/data/2.5/forecast?q={City}&units=metric&lang={language}&appid={apiKey}";

                forecast = await ReadWebApiAsync(uri);
                cachedCityForecasts[key] = forecast;
                return forecast;
            }

            return forecast;

        }
        public async Task<Forecast> GetForecastAsync(double latitude, double longitude)
        {
            Forecast forecast = null;
            var key = (latitude, longitude, DateTime.Now.ToString("yyyy-MM-dd HH:mm"));

            if (!cachedGeoForecasts.TryGetValue(key, out forecast))
            {
                //https://openweathermap.org/current
                var language = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                var uri = $"https://api.openweathermap.org/data/2.5/forecast?lat={latitude}&lon={longitude}&units=metric&lang={language}&appid={apiKey}";

                forecast = await ReadWebApiAsync(uri);
                cachedGeoForecasts[key] = forecast;
                return forecast;
            }

            return forecast;
        }
        private async Task<Forecast> ReadWebApiAsync(string uri)
        {
            HttpResponseMessage response = await httpClient.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            WeatherApiData wd = await response.Content.ReadFromJsonAsync<WeatherApiData>();

            var forecast = new Forecast()
            {
                City = wd.city.name,
                Items = wd.list.Select(wdle => new ForecastItem()
                {
                    DateTime = UnixTimeStampToDateTime(wdle.dt),
                    Temperature = wdle.main.temp,
                    WindSpeed = wdle.wind.speed,
                    Description = wdle.weather.First().description,
                    Icon = $"https://openweathermap.org/img/w/{wdle.weather.First().icon}.png"
                }).ToList()
            };
            return forecast;
        }
        private DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }
    }
}
