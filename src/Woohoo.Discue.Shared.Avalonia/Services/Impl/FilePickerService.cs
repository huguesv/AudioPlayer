// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Shared.Avalonia.Services.Impl;

using global::Avalonia.Platform.Storage;

public sealed class FilePickerService : IFilePickerService
{
    private readonly ITopLevelProvider topLevelProvider;

    public FilePickerService(ITopLevelProvider topLevelProvider)
    {
        ArgumentNullException.ThrowIfNull(topLevelProvider);

        this.topLevelProvider = topLevelProvider;
    }

    public async Task<string[]> GetFilePathsAsync(string startFolderPath, string title, bool allowMultiple, IReadOnlyList<FilePickerFileType> filters)
    {
        var window = this.topLevelProvider.GetTopLevel() ?? throw new InvalidOperationException();
        if (window.StorageProvider.CanOpen == true)
        {
            IStorageFolder? startLocation = await window.StorageProvider.TryGetFolderFromPathAsync(startFolderPath);

            var options = new FilePickerOpenOptions
            {
                Title = title,
                AllowMultiple = allowMultiple,
                SuggestedStartLocation = startLocation,
                FileTypeFilter = filters,
            };

            var files = await window.StorageProvider.OpenFilePickerAsync(options);

            var filePaths = new List<string>();
            foreach (var file in files)
            {
                string? path = file.TryGetLocalPath();
                if (!string.IsNullOrEmpty(path))
                {
                    filePaths.Add(path);
                }
            }

            return [.. filePaths];
        }

        return [];
    }
}
