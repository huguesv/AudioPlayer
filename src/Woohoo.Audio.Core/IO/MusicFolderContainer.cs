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

    public byte[] ReadFileBytes(string fileName, long offset, long count)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(fileName);
        using var stream = new FileStream(Path.Combine(this.ContainerPath, fileName), FileMode.Open, FileAccess.Read);
        stream.Seek(offset, SeekOrigin.Begin);
        byte[] buffer = new byte[count];
        int bytesRead = stream.Read(buffer);
        if (bytesRead < count)
        {
            Array.Resize(ref buffer, bytesRead);
        }

        return buffer;
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
