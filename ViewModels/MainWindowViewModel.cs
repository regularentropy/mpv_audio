using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using mpv_audio.Services;

namespace mpv_audio.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(AudioTitles))]
    private IReadOnlyList<IStorageFile>? _audioList;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(VideoTitles))]
    private IReadOnlyList<IStorageFile>? _videoList;

    [ObservableProperty] private int _selectedAudioID;

    [ObservableProperty] private int _selectedVideoID;

    [ObservableProperty] private string? _selectedDatabasePath;

    [ObservableProperty] private string? _selectedDatabaseDisplayName;

    public ObservableCollection<string> AudioTitles { get; } = new();
    public ObservableCollection<string> VideoTitles { get; } = new();
    public ObservableCollection<string> DatabaseHistory { get; }
    public ObservableCollection<string> DatabaseHistoryDisplayNames { get; }

    private readonly IFileDialogService _fileService;
    private readonly ICacheService _cacheService;
    private readonly IConfigService _configService;

    public MainWindowViewModel(Window window)
    {
        _fileService = new FileDialogService(window);
        _configService = new ConfigService();
        _cacheService = new CacheService(window, _configService);

        DatabaseHistory = new(_configService.Config.DatabaseHistory);
        DatabaseHistoryDisplayNames = new(_configService.Config.DatabaseHistory.Select(Path.GetFileName));

        _cacheService.DatabaseHistoryChanged += RefreshDatabaseHistory;

        SelectedDatabasePath = _configService.Config.LastDatabasePath;
        SelectedDatabaseDisplayName = !string.IsNullOrEmpty(SelectedDatabasePath)
            ? Path.GetFileName(SelectedDatabasePath)
            : null;

        _ = LoadLastCacheAsync();
    }

    [RelayCommand]
    private async Task OpenVideoFileAsync()
    {
        try
        {
            VideoList = await _fileService.OpenFileAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: no video files were selected");
        }
    }

    [RelayCommand]
    private async Task OpenAudioFileAsync()
    {
        try
        {
            AudioList = await _fileService.OpenFileAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: no audio files were selected");
        }
    }

    [RelayCommand]
    public void ClearVideo() => VideoList = null;

    [RelayCommand]
    public void ClearAudio() => AudioList = null;

    [RelayCommand]
    private async Task SaveCache()
    {
        if (HasFilesToCache())
        {
            await _cacheService.SaveMediaCacheAsync(AudioList, VideoList);
        }
    }

    [RelayCommand]
    private async Task LoadCache()
    {
        try
        {
            (AudioList, VideoList) = await _cacheService.LoadMediaCacheAsync();
        }
        catch (Exception e)
        {
            // ignored
        }
    }

    [RelayCommand]
    private async Task RunMPVAsync()
    {
        if (CanRunPlayer())
        {
            var videoPath = VideoList![SelectedVideoID].Path.AbsoluteUri;
            var audioPath = AudioList![SelectedAudioID].Path.AbsoluteUri;
            await Task.Run(() => OpenerService.SpawnMPV(videoPath, audioPath, _configService.Config.MpvPath));
        }
    }

    partial void OnSelectedDatabasePathChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _ = LoadCacheFromPath(value);
        }
    }

    partial void OnSelectedDatabaseDisplayNameChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            var index = DatabaseHistoryDisplayNames.IndexOf(value);
            var fullPath = DatabaseHistory[index];
            SelectedDatabasePath = fullPath;
        }
    }

    partial void OnAudioListChanged(IReadOnlyList<IStorageFile>? value)
    {
        AudioTitles.Clear();
        if (value != null)
        {
            foreach (var file in value)
            {
                AudioTitles.Add(file.Name);
            }
        }
    }

    partial void OnVideoListChanged(IReadOnlyList<IStorageFile>? value)
    {
        VideoTitles.Clear();
        if (value != null)
        {
            foreach (var file in value)
            {
                VideoTitles.Add(file.Name);
            }
        }
    }

    private async Task LoadLastCacheAsync()
    {
        if (string.IsNullOrEmpty(_configService.Config?.LastDatabasePath))
            return;

        (AudioList, VideoList) = await _cacheService.LoadMediaCacheFromPathAsync(
            _configService.Config.LastDatabasePath);
    }

    private async Task LoadCacheFromPath(string path)
    {
        (AudioList, VideoList) = await _cacheService.LoadMediaCacheFromPathAsync(path);
    }

    private void RefreshDatabaseHistory()
    {
        DatabaseHistory.Clear();
        DatabaseHistoryDisplayNames.Clear();
        foreach (var item in _configService.Config.DatabaseHistory!)
        {
            DatabaseHistory.Add(item);
            DatabaseHistoryDisplayNames.Add(Path.GetFileName(item));
        }
    }

    private bool HasFilesToCache() => (AudioList?.Count ?? 0) > 0 || (VideoList?.Count ?? 0) > 0;

    private bool CanRunPlayer() => VideoList?.Count > 0 && AudioList?.Count > 0;
}