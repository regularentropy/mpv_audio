using System.Collections.Generic;

namespace mpv_audio.Models;
/// <summary>
/// DTO for cache.
/// Contains Audio and Video path
/// </summary>
public class Cache
{
    public List<string> AudioPaths { get; set; } = [];
    public List<string> VideoPaths { get; set; } = [];
}
