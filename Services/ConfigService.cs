using System;
using System.IO;
using System.Text.Json;
using mpv_audio.Models;

namespace mpv_audio.Services;

/// <summary>
/// Service for managing application configuration persistence
/// </summary>
public class ConfigService : IConfigService
{
    private static readonly string ConfigDirectory =
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

    private static readonly string ConfigPath =
        Path.Combine(ConfigDirectory, "mpv-audio", "config.json");

    /// <inheritdoc/>
    public Config Config { get; set; }

    /// <summary>
    /// Initializes a new instance of the ConfigService
    /// </summary>
    public ConfigService() => LoadConfiguration();

    private void LoadConfiguration()
    {
        if (!File.Exists(ConfigPath))
        {
            CreateDefaultConfiguration();
            return;
        }

        try
        {
            string configJson = File.ReadAllText(ConfigPath);
            Config = JsonSerializer.Deserialize(configJson, JsonContext.Default.Config) ?? new Config();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deserializing config: {ex.Message}");
            CreateDefaultConfiguration();
        }
    }

    private void CreateDefaultConfiguration()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
            Config = new Config() { MpvPath = "mpv" };
            SaveConfiguration();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create config: {ex.Message}");
            throw new IOException("Could not create configuration file", ex);
        }
    }

    /// <inheritdoc/>
    public void SaveConfiguration()
    {
        try
        {
            string config = JsonSerializer.Serialize(Config, JsonContext.Default.Config);
            File.WriteAllText(ConfigPath, config);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save config: {ex.Message}");
            throw;
        }
    }
}