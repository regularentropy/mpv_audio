using System;
using System.IO;
using mpv_audio.Models;

namespace mpv_audio.Services;

public interface IConfigService
{
    /// <summary>
    /// Gets or sets the current application configuration
    /// </summary>
    Config Config { get; set; }

    /// <summary>
    /// Saves the current configuration to persistent storage
    /// </summary>
    /// <exception cref="IOException">Thrown when configuration cannot be saved</exception>
    void SaveConfiguration();
}