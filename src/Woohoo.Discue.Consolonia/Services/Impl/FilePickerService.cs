// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Tui.Services;

using System.Collections.Generic;
using Avalonia.Platform.Storage;

internal class FilePickerService : IFilePickerService
{
    private readonly ITopLevelProvider topLevelProvider;

    public FilePickerService(ITopLevelProvider topLevelProvider)
    {
        this.topLevelProvider = topLevelProvider;
    }

    public async Task<string?> GetFilePathAsync(string startFolderPath, string title, bool allowMultiple, IReadOnlyList<FilePickerFileType> filters)
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

            IReadOnlyList<IStorageFile> files = await window.StorageProvider.OpenFilePickerAsync(options);
            IStorageFile? file = files?.FirstOrDefault();
            if (file != null)
            {
                return file.TryGetLocalPath();
            }
        }

        return null;
    }
}
