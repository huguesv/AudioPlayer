// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.IO;

using System.Collections.Generic;
using System.IO;

public class MusicFolderContainer : IMusicContainer
{
    public MusicFolderContainer(string folderPath)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(folderPath);
        this.ContainerPath = folderPath;
    }

    public string ContainerPath { get; }

    public bool FileExists(string fileName)
    {
        return !string.IsNullOrEmpty(fileName) && File.Exists(Path.Combine(this.ContainerPath, fileName));
    }

    public IEnumerable<string> EnumerateFilesByExtension(string extension)
    {
        ArgumentNullException.ThrowIfNull(extension);
        return Directory.EnumerateFiles(this.ContainerPath, $"*.{extension}");
    }

    public long GetFileSize(string fileName)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(fileName);
        return new FileInfo(Path.Combine(this.ContainerPath, fileName)).Length;
    }

    public byte[] ReadFileBytes(string fileName)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(fileName);
        return File.ReadAllBytes(Path.Combine(this.ContainerPath, fileName));
    }

    public string ReadFileText(string fileName)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(fileName);
        return File.ReadAllText(Path.Combine(this.ContainerPath, fileName));
    }

    public Stream OpenFileStream(string fileName)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(fileName);
        return File.OpenRead(Path.Combine(this.ContainerPath, fileName));
    }
}
