using System.Collections.Generic;
namespace mpv_audio.Models;
/// <summary>
/// DTO for config.
/// Contains parameters saved into config
/// </summary>
public class Config
{
    public string MpvPath = "mpv";
    public string? LastDatabasePath { get; set; }
    public List<string> DatabaseHistory { get; set; } = [];
}