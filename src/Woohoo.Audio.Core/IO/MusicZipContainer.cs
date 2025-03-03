﻿// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.IO;

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

public class MusicZipContainer : IMusicContainer
{
    private readonly ZipArchive archive;

    public MusicZipContainer(string containerPath)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(containerPath);

        this.ContainerPath = containerPath;
        this.archive = new ZipArchive(new FileStream(containerPath, FileMode.Open, FileAccess.Read));
    }

    public string ContainerPath { get; }

    public bool FileExists(string fileName)
    {
        return !string.IsNullOrEmpty(fileName) && this.archive.GetEntry(fileName) is not null;
    }

    public IEnumerable<string> EnumerateFilesByExtension(string extension)
    {
        ArgumentNullException.ThrowIfNull(extension);

        foreach (var entry in this.archive.Entries)
        {
            if (entry.FullName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                yield return entry.FullName;
            }
        }
    }

    public long GetFileSize(string fileName)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(fileName);

        var entry = this.archive.GetEntry(fileName);
        if (entry is null)
        {
            throw EntryNotFound(fileName);
        }

        return entry.Length;
    }

    public byte[] ReadFileBytes(string fileName)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(fileName);

        var entry = this.archive.GetEntry(fileName);
        if (entry is null)
        {
            throw EntryNotFound(fileName);
        }

        using var stream = entry.Open();
        byte[] bytes = new byte[entry.Length];
        stream.ReadExactly(bytes);
        return bytes;
    }

    public string ReadFileText(string fileName)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(fileName);

        var entry = this.archive.GetEntry(fileName);
        if (entry is null)
        {
            throw EntryNotFound(fileName);
        }

        using var stream = entry.Open();
        StreamReader reader = new(stream, Encoding.UTF8);
        var data = reader.ReadToEnd();
        return data;
    }

    public Stream OpenFileStream(string fileName)
    {
        ArgumentNullException.ThrowIfNullOrEmpty(fileName);

        var entry = this.archive.GetEntry(fileName);
        if (entry is null)
        {
            throw EntryNotFound(fileName);
        }

        return new ZipEntrySeekableStream(entry);
    }

    private static FileNotFoundException EntryNotFound(string fileName)
    {
        return new FileNotFoundException("File not found inside archive.", fileName);
    }
}
