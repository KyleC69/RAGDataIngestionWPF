// Build Date: 2026/03/13
// Solution: RAGDataIngestionWPF
// Project:   DataIngestionLib
// File:         SettingsProvider.cs
// Author: Kyle L. Crowder
// Build Num: 202413



using System.Collections.Specialized;
using System.Configuration;

using Microsoft.Extensions.Logging;




namespace DataIngestionLib.Providers;





public interface ISettingsProvider
{


    void AddUpdateAppSettings(string key, string value);


    NameValueCollection? ReadAllSettings();


    string ReadSetting(string key);
}





public sealed class SettingsProvider(ILogger<SettingsProvider> logger) : ISettingsProvider
{

    private ILogger<SettingsProvider> _logger = logger;








    public NameValueCollection? ReadAllSettings()
    {
        try
        {
            NameValueCollection? appSettings = ConfigurationManager.AppSettings;

            if (appSettings.Count == 0)
            {
                _logger?.LogWarning("AppSettings is empty.");
            }
            else
            {
                return appSettings;
            }
        }
        catch (ConfigurationErrorsException)
        {
            _logger?.LogWarning("Error reading app settings");
        }

        return null;
    }








    public string ReadSetting(string key)
    {
        try
        {
            NameValueCollection? appSettings = ConfigurationManager.AppSettings;
            var result = appSettings[key] ?? "Not Found";
            if (result == "Not Found")
            {
                _logger.LogWarning("Key '{0}' not found. Adding Key to app settings.", key);
                AddUpdateAppSettings(key, "Empty");
            }

            return result;
        }
        catch (ConfigurationErrorsException)
        {
            _logger.LogWarning("Error reading app settings");
            return "Not Found";
        }
    }








    public void AddUpdateAppSettings(string key, string value)
    {
        try
        {
            Configuration? configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            KeyValueConfigurationCollection? settings = configFile.AppSettings.Settings;
            if (settings[key] == null)
            {
                settings.Add(key, value);
            }
            else
            {
                settings[key].Value = value;
            }

            configFile.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
        }
        catch (ConfigurationErrorsException)
        {
            _logger.LogWarning("Error writing app settings");
        }
    }
}