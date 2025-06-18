using Avalonia.Controls;
using Avalonia.Platform.Storage;
using mpv_audio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using mpv_audio.Models;
using mpv_audio.Services;

namespace mpv_audio.Services;

public class CacheService : ICacheService
{
    private readonly Window _window;
    private readonly IConfigService _configService;

    public event Action? DatabaseHistoryChanged;

    public CacheService(Window window, IConfigService configService)
    {
        _window = window;
        _configService = configService;
    }

    /// <summary>
    /// Saves audio and video file references to a cache file in JSON format
    /// </summary>
    /// <param name="audio">Collection of audio files to cache</param>
    /// <param name="video">Collection of video files to cache</param>
    /// <exception cref="Exception">Thrown if cache saving fails</exception>
    public async Task SaveMediaCacheAsync(IReadOnlyList<IStorageFile>? audio, IReadOnlyList<IStorageFile>? video)
    {
        try
        {
            var cacheData = CreateCacheFromMediaFiles(audio, video);
            var file = await GetCacheSaveFileAsync();
            if (file == null) return;

            await SerializeAndSaveCacheAsync(file, cacheData);

            UpdateLastCachePath(file.Path.LocalPath);
            AddToCacheHistory(file.Path.LocalPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving cache: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Loads audio and video file references from a cache file
    /// </summary>
    /// <returns>Tuple containing audio and video file collections</returns>
    /// <exception cref="InvalidOperationException">Thrown when cache file is not found</exception>
    /// <exception cref="Exception">Thrown if cache loading fails</exception>
    public async Task<(IReadOnlyList<IStorageFile> audio, IReadOnlyList<IStorageFile> video)> LoadMediaCacheAsync()
    {
        try
        {
            var cacheData = await GetCacheLoadFileAsync();

            if (cacheData.Count == 0)
                throw new InvalidOperationException("Cache files not found");

            var cachePath = cacheData[0].Path.LocalPath;

            UpdateLastCachePath(cachePath);
            AddToCacheHistory(cachePath);

            _configService.SaveConfiguration();

            return await LoadMediaCacheFromPathAsync(cachePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading cache: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Loads media files from a specified cache file path
    /// </summary>
    /// <param name="cachePath">Full path to the cache file</param>
    /// <returns>Tuple containing audio and video file collections</returns>
    /// <remarks>Returns empty collections if file doesn't exist</remarks>
    public async Task<(IReadOnlyList<IStorageFile> audio, IReadOnlyList<IStorageFile> video)>
        LoadMediaCacheFromPathAsync(string cachePath)
    {
        if (!File.Exists(cachePath))
            return ([], []);

        try
        {
            var data = await DeserializeCacheFileAsync(cachePath);

            var audioFiles = await ResolveFilePathsAsync(data.AudioPaths);
            var videoFiles = await ResolveFilePathsAsync(data.VideoPaths);

            return (audioFiles, videoFiles);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading cache from path: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Creates cache data structure from media file collections
    /// </summary>
    /// <param name="audio">Audio files to include in cache</param>
    /// <param name="video">Video files to include in cache</param>
    /// <returns>Initialized Cache object with file paths</returns>
    private Cache CreateCacheFromMediaFiles(IReadOnlyList<IStorageFile>? audio, IReadOnlyList<IStorageFile>? video)
    {
        return new Cache
        {
            AudioPaths = audio?.Select(f => f.Path.LocalPath).ToList() ?? [],
            VideoPaths = video?.Select(f => f.Path.LocalPath).ToList() ?? []
        };
    }

    /// <summary>
    /// Shows file picker dialog to select cache save location
    /// </summary>
    /// <returns>Selected storage file or null if canceled</returns>
    private async Task<IStorageFile?> GetCacheSaveFileAsync()
    {
        return await _window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save Cache File",
            SuggestedFileName = "cache",
            DefaultExtension = ".json",
            FileTypeChoices =
            [
                new FilePickerFileType("JSON File")
                {
                    Patterns = ["*.json"]
                }
            ]
        });
    }

    /// <summary>
    /// Shows file picker dialog to select cache file to load
    /// </summary>
    /// <returns>Collection of selected files (single item)</returns>
    private async Task<IReadOnlyList<IStorageFile>> GetCacheLoadFileAsync()
    {
        return await _window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Load Cache File",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("JSON Files")
                {
                    Patterns = ["*.json"]
                }
            ]
        });
    }

    /// <summary>
    /// Serializes cache data to JSON and writes to specified file
    /// </summary>
    /// <param name="file">Destination storage file</param>
    /// <param name="data">Cache data to serialize</param>
    private async Task SerializeAndSaveCacheAsync(IStorageFile file, Cache data)
    {
        string json = JsonSerializer.Serialize(data, JsonContext.Default.Cache);
        await using var stream = await file.OpenWriteAsync();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(json);
    }

    /// <summary>
    /// Updates configuration with the last used cache file path
    /// </summary>
    /// <param name="path">Full path to the cache file</param>
    private void UpdateLastCachePath(string path)
    {
        _configService.Config.LastDatabasePath = path;
    }

    /// <summary>
    /// Adds cache file path to history if not already present
    /// </summary>
    /// <param name="path">Full path to the cache file</param>
    private void AddToCacheHistory(string path)
    {
        if (_configService.Config.DatabaseHistory.Contains(path)) return;
        _configService.Config.DatabaseHistory.Add(path);

        DatabaseHistoryChanged?.Invoke();
    }

    /// <summary>
    /// Reads and deserializes cache data from specified file path
    /// </summary>
    /// <param name="path">Full path to the cache file</param>
    /// <returns>Deserialized Cache object or null</returns>
    private async Task<Cache?> DeserializeCacheFileAsync(string path)
    {
        string json = await File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize(json, JsonContext.Default.Cache);
    }

    /// <summary>
    /// Converts file paths to IStorageFile references
    /// </summary>
    /// <param name="paths">Collection of file paths to resolve</param>
    /// <returns>Collection of resolved storage files</returns>
    private async Task<IReadOnlyList<IStorageFile>> ResolveFilePathsAsync(IEnumerable<string> paths)
    {
        var tasks = paths.Select(p => _window.StorageProvider.TryGetFileFromPathAsync(p));
        var files = await Task.WhenAll(tasks);
        return files.Where(f => f != null).Cast<IStorageFile>().ToList();
    }
}