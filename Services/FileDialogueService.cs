using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace mpv_audio.Services;

public class FileDialogService : IFileDialogService
{
    private Window? _currentWindow;

    public FileDialogService(Window? currentWindow)
    {
        _currentWindow = currentWindow;
    }

    public async Task<IReadOnlyList<IStorageFile>?> OpenFileAsync()
    {
        if (_currentWindow == null) throw new InvalidOperationException("Window not set");

        var topLevel = TopLevel.GetTopLevel(_currentWindow);
        var selectedFile = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Media",
            AllowMultiple = true
        });
        if (selectedFile.Count == 0) throw new Exception("Files not selected");
        return selectedFile;
    }
}