// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Avalonia.Services;

using System.Threading.Tasks;
using global::Avalonia.Platform.Storage;

public interface IFilePickerService
{
    Task<string[]> GetFilePathsAsync(string startFolderPath, string title, bool allowMultiple, IReadOnlyList<FilePickerFileType> filters);
}
