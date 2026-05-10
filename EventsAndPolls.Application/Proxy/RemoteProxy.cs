using System.Net.Http.Json;

namespace EventsAndPolls.Application.Proxy;

// Proxy Pattern — Remote Proxy (Theoretical Example)
//
// CONCEPT: Represents an object that lives in a different address space —
// another server, external API, or microservice. The client calls methods
// on the proxy as if the object were local. The proxy handles all network
// communication, serialization, timeouts, and error handling transparently.
//
// GENERAL SCENARIO: A weather service client.
// Your application needs weather data but the weather service runs on a
// remote server. Without a proxy, every piece of code that needs weather
// data must handle HTTP calls, JSON deserialization, and network errors itself.
// The RemoteWeatherServiceProxy makes the remote service look like a local object.

public interface IWeatherService
{
     Task<WeatherReport> GetCurrentWeatherAsync(string city);
     Task<IEnumerable<WeatherReport>> GetForecastAsync(string city, int days);
}

public class WeatherReport
{
     public string City { get; set; } = string.Empty;
     public double TemperatureCelsius { get; set; }
     public string Condition { get; set; } = string.Empty;
     public int Humidity { get; set; }
     public DateTime RecordedAt { get; set; }
}

// Real local implementation (used when running the weather service yourself)
public class LocalWeatherService : IWeatherService
{
     public Task<WeatherReport> GetCurrentWeatherAsync(string city)
     {
          Console.WriteLine($"[WeatherService] Fetching local weather for {city}");
          return Task.FromResult(new WeatherReport
          {
               City = city,
               TemperatureCelsius = 22.5,
               Condition = "Sunny",
               Humidity = 55,
               RecordedAt = DateTime.UtcNow
          });
     }

     public Task<IEnumerable<WeatherReport>> GetForecastAsync(string city, int days)
     {
          Console.WriteLine($"[WeatherService] Fetching local {days}-day forecast for {city}");
          return Task.FromResult(Enumerable.Empty<WeatherReport>());
     }
}

// Remote Proxy — makes the remote weather API look like a local IWeatherService
// The client has no idea it is making HTTP calls to https://api.weatherprovider.com
public class RemoteWeatherServiceProxy : IWeatherService
{
     private readonly HttpClient _httpClient;
     private readonly string _baseUrl;

     public RemoteWeatherServiceProxy(HttpClient httpClient,
         string baseUrl = "https://api.weatherprovider.com/v1")
     {
          _httpClient = httpClient;
          _baseUrl = baseUrl;
     }

     public async Task<WeatherReport> GetCurrentWeatherAsync(string city)
     {
          Console.WriteLine($"[RemoteProxy] Calling remote weather API for city: {city}");

          try
          {
               // Proxy transparently handles the HTTP call
               var response = await _httpClient.GetAsync($"{_baseUrl}/current?city={city}");
               response.EnsureSuccessStatusCode();

               var report = await response.Content.ReadFromJsonAsync<WeatherReport>();
               return report ?? throw new InvalidOperationException("Empty response from weather service");
          }
          catch (HttpRequestException ex)
          {
               // Proxy handles network errors and wraps them in domain exceptions
               Console.WriteLine($"[RemoteProxy] Network error: {ex.Message}");
               throw new InvalidOperationException($"Could not reach weather service: {ex.Message}", ex);
          }
     }

     public async Task<IEnumerable<WeatherReport>> GetForecastAsync(string city, int days)
     {
          Console.WriteLine($"[RemoteProxy] Calling remote forecast API — city: {city}, days: {days}");

          try
          {
               var response = await _httpClient.GetAsync($"{_baseUrl}/forecast?city={city}&days={days}");
               response.EnsureSuccessStatusCode();

               return await response.Content.ReadFromJsonAsync<IEnumerable<WeatherReport>>()
                      ?? Enumerable.Empty<WeatherReport>();
          }
          catch (HttpRequestException ex)
          {
               Console.WriteLine($"[RemoteProxy] Network error: {ex.Message}");
               throw new InvalidOperationException($"Could not reach weather service: {ex.Message}", ex);
          }
     }
}

// Client — uses IWeatherService, completely unaware whether it is local or remote
public class WeatherDashboard
{
     private readonly IWeatherService _weatherService;

     public WeatherDashboard(IWeatherService weatherService)
     {
          _weatherService = weatherService;
     }

     public async Task DisplayCurrentWeather(string city)
     {
          // Same code works whether _weatherService is local or remote proxy
          var report = await _weatherService.GetCurrentWeatherAsync(city);
          Console.WriteLine($"{report.City}: {report.TemperatureCelsius}°C, {report.Condition}");
     }
}

// Example usage:
//   // Swap between local and remote with no changes to WeatherDashboard
//   IWeatherService service = new RemoteWeatherServiceProxy(new HttpClient());
//   var dashboard = new WeatherDashboard(service);
//   await dashboard.DisplayCurrentWeather("Chisinau");