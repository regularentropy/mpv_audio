using Avalonia.Platform.Storage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mpv_audio.Services;

internal interface IFileDialogService
{
    Task<IReadOnlyList<IStorageFile>?> OpenFileAsync();
}