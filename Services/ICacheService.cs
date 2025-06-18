using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

internal interface ICacheService
{
    Task SaveMediaCacheAsync(IReadOnlyList<IStorageFile>? audio, IReadOnlyList<IStorageFile>? video);
    Task<(IReadOnlyList<IStorageFile> audio, IReadOnlyList<IStorageFile> video)> LoadMediaCacheAsync();

    Task<(IReadOnlyList<IStorageFile> audio, IReadOnlyList<IStorageFile> video)> LoadMediaCacheFromPathAsync(
        string cachePath);

    public event Action? DatabaseHistoryChanged;
}