using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace EventsAndPolls.Application.Services;

// Singleton Pattern
public sealed class AppConfiguration
{
     // Static instance - the one and only instance
     private static AppConfiguration? _instance;
     private static readonly object _lock = new object();

     private readonly IConfiguration _configuration;
     private readonly Dictionary<string, object> _settings;

     private AppConfiguration()
     {
          _settings = new Dictionary<string, object>();

          // Load default settings
          _settings["MaxEventsPerUser"] = 10;
          _settings["MaxPollsPerEvent"] = 20;
          _settings["DefaultPollDurationDays"] = 7;
          _settings["AllowAnonymousVoting"] = true;
          _settings["VoteCooldownMinutes"] = 0;
     }

     // Public access point to get the instance
     public static AppConfiguration Instance
     {
          get
          {
               if (_instance == null)
               {
                    lock (_lock)
                    {
                         _instance ??= new AppConfiguration();
                    }
               }
               return _instance;
          }
     }

     // Methods to access settings
     public T GetSetting<T>(string key, T defaultValue = default)
     {
          if (_settings.ContainsKey(key) && _settings[key] is T value)
               return value;

          return defaultValue;
     }

     public void SetSetting(string key, object value)
     {
          _settings[key] = value;
     }

     public void LoadFromConfiguration(IConfiguration configuration)
     {
          if (int.TryParse(configuration["AppSettings:MaxEventsPerUser"], out int maxEvents))
               _settings["MaxEventsPerUser"] = maxEvents;

          if (int.TryParse(configuration["AppSettings:MaxPollsPerEvent"], out int maxPolls))
               _settings["MaxPollsPerEvent"] = maxPolls;
     }

     public void DisplaySettings()
     {
          Console.WriteLine("=== Application Configuration (Singleton) ===");
          foreach (var setting in _settings)
          {
               Console.WriteLine($"{setting.Key}: {setting.Value}");
          }
     }
}
