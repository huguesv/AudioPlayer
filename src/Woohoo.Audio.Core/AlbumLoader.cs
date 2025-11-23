// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Woohoo.Audio.Core.Cue;
using Woohoo.Audio.Core.Cue.Serialization;
using Woohoo.Audio.Core.IO;

public sealed class AlbumLoader
{
    public ImmutableList<AlbumTrack> LoadFrom(string filePath)
    {
        if (string.Equals(Path.GetExtension(filePath), ".cue", StringComparison.OrdinalIgnoreCase))
        {
            return this.OpenCue(filePath);
        }
        else if (string.Equals(Path.GetExtension(filePath), ".zip", StringComparison.OrdinalIgnoreCase))
        {
            return this.OpenArchive(filePath);
        }

        throw new ArgumentException("Unsupported file format.", nameof(filePath));
    }

    private ImmutableList<AlbumTrack> OpenArchive(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found.", filePath);
        }

        return this.ProcessContainer(new MusicZipContainer(filePath));
    }

    private ImmutableList<AlbumTrack> OpenCue(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found.", filePath);
        }

        string? folder = Path.GetDirectoryName(filePath);
        if (folder is null)
        {
            throw new ArgumentException("Invalid file path.", nameof(filePath));
        }

        return this.ProcessContainer(new MusicFolderContainer(folder));
    }

    private ImmutableList<AlbumTrack> ProcessContainer(IMusicContainer container)
    {
        var albumTracks = new List<AlbumTrack>();

        var cueFile = container.EnumerateFilesByExtension("cue").FirstOrDefault();
        if (cueFile is null)
        {
            throw new InvalidOperationException("No .cue file found in the container.");
        }

        var cueSheetName = Path.GetFileNameWithoutExtension(cueFile);
        var cueSheetFileName = cueFile;
        var cueSheetContainerPath = container.ContainerPath;

        var cueData = container.ReadFileText(cueSheetFileName);

        var parser = new CueSheetReader();
        var cueSheet = parser.Parse(cueData);

        foreach (var file in cueSheet.Files)
        {
            var fileExists = container.FileExists(file.FileName);
            long fileSize = fileExists ? container.GetFileSize(file.FileName) : 0;

            var tracksInfo = new (int StartOffset, int PlayOffset)[file.Tracks.Count + 1];
            for (int i = 0; i < file.Tracks.Count; i++)
            {
                var firstIndexOffset = file.Tracks[i].Indexes.FirstOrDefault()?.Time.ToBytes();
                var firstIndex1Offset = file.Tracks[i].Indexes.Where(idx => idx.IndexNumber == 1).FirstOrDefault()?.Time.ToBytes();

                tracksInfo[i] = (StartOffset: firstIndexOffset ?? 0, PlayOffset: firstIndex1Offset ?? firstIndexOffset ?? 0);
            }

            tracksInfo[file.Tracks.Count] = (StartOffset: (int)fileSize, PlayOffset: (int)fileSize);

            for (int i = 0; i < file.Tracks.Count; i++)
            {
                var track = file.Tracks[i];
                if (track.TrackMode != KnownTrackModes.Audio)
                {
                    continue;
                }

                var albumTrack = new AlbumTrack
                {
                    Container = container,
                    CueSheet = cueSheet,
                    CueSheetName = cueSheetName,
                    CueSheetFileName = cueSheetFileName,
                    CueSheetContainerPath = cueSheetContainerPath,
                    AlbumTitle = cueSheet.Title ?? string.Empty,
                    AlbumPerformer = cueSheet.Performer ?? string.Empty,
                    TrackFileName = file.FileName,
                    TrackFileNotFound = !fileExists,
                    TrackNumber = track.TrackNumber,
                    TrackTitle = track.Title ?? string.Empty,
                    TrackPerformer = track.Performer ?? cueSheet.Performer ?? string.Empty,
                    TrackSongwriter = track.Songwriter ?? cueSheet.Songwriter ?? string.Empty,
                    TrackOffset = fileExists ? tracksInfo[i].PlayOffset : 0,
                    TrackSize = fileExists ? tracksInfo[i + 1].StartOffset - tracksInfo[i].PlayOffset : 0,
                };

                albumTracks.Add(albumTrack);
            }
        }

        return [.. albumTracks];
    }
}
