// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Services;

using System;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;

internal class NullFilePickerService : IFilePickerService
{
    public Task<string[]> GetFilePathsAsync(string title, bool allowMultiple, IReadOnlyList<FilePickerFileType> filters)
    {
        return Task.FromResult(Array.Empty<string>());
    }
}
