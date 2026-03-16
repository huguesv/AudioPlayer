// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Media;

using System;
using System.Collections.Generic;
using System.Linq;
using Woohoo.Audio.Core.Internal.Cue;
using Woohoo.Audio.Core.Internal.Cue.Serialization;
using Woohoo.Audio.Core.Internal.CueToolsDatabase;
using Woohoo.Audio.Core.Internal.IO;
using Woohoo.IO.Compression.Chd;

public sealed class MediaLoader
{
    public IAlbumMedia LoadFrom(string filePath)
    {
        string ext = Path.GetExtension(filePath);
        if (string.Equals(ext, ".cue", StringComparison.OrdinalIgnoreCase))
        {
            return this.OpenCue(filePath);
        }
        else if (string.Equals(ext, ".zip", StringComparison.OrdinalIgnoreCase))
        {
            return this.OpenArchive(filePath);
        }
        else if (string.Equals(ext, ".chd", StringComparison.OrdinalIgnoreCase))
        {
            return this.OpenChd(filePath);
        }

        throw new ArgumentException("Unsupported file format.", nameof(filePath));
    }

    private IAlbumMedia OpenArchive(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("File not found.", filePath);
        }

        return this.ProcessContainer(new ZipFolder(filePath));
    }

    private IAlbumMedia OpenCue(string filePath)
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

        return this.ProcessContainer(new RegularFolder(folder));
    }

    private IAlbumMedia ProcessContainer(IFolder folder)
    {
        var cueFile = folder.EnumerateFilesByExtension("cue").FirstOrDefault();
        if (cueFile is null)
        {
            throw new InvalidOperationException("No .cue file found in the container.");
        }

        var cueSheetName = Path.GetFileNameWithoutExtension(cueFile);
        var cueSheetFileName = cueFile;
        var cueSheetContainerPath = folder.ContainerPath;

        var cueData = folder.ReadFileText(cueSheetFileName);

        var parser = new CueSheetReader();
        var cueSheet = parser.Parse(cueData);

        var albumTracks = new List<IAlbumTrack>();

        foreach (var file in cueSheet.Files)
        {
            var fileExists = folder.FileExists(file.FileName);
            long fileSize = fileExists ? folder.GetFileSize(file.FileName) : 0;

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

                var albumTrack = new BinCueAlbumTrack(
                    folder,
                    track.TrackNumber,
                    track.TrackMode,
                    file.FileName,
                    !fileExists,
                    cueSheetName,
                    fileExists ? tracksInfo[i].PlayOffset : 0,
                    fileExists ? tracksInfo[i + 1].StartOffset - tracksInfo[i].PlayOffset : 0)
                {
                    AlbumTitle = cueSheet.Title ?? string.Empty,
                    AlbumPerformer = cueSheet.Performer ?? string.Empty,
                    TrackTitle = track.Title ?? string.Empty,
                    TrackPerformer = track.Performer ?? cueSheet.Performer ?? string.Empty,
                    TrackSongwriter = track.Songwriter ?? cueSheet.Songwriter ?? string.Empty,
                };

                albumTracks.Add(albumTrack);
            }
        }

        string toc = CTDBTocCalculator.GetTocFromCue(cueSheet, folder);

        return new BinCueAlbumMedia(cueSheet, folder, albumTracks, toc);
    }

    private IAlbumMedia OpenChd(string filePath)
    {
        var chdFile = new ChdFile(filePath);
        var cdRomFile = new CdRomFile(chdFile);
        var albumTracks = new List<IAlbumTrack>();
        var sectors = new List<int>();
        var absoluteTrackStartSector = 0;

        for (int i = 0; i < cdRomFile.Toc.NumTracks; i++)
        {
            var track = cdRomFile.Toc.Tracks[i];
            if (track.TrackType != CdRomTrackType.Audio)
            {
                continue;
            }

            var fileSectors = (int)track.Frames;

            var trackStartSector = (int)(absoluteTrackStartSector + track.Pregap);
            sectors.Add(trackStartSector);

            var albumTrack = new ChdAlbumTrack(
                cdRomFile,
                Path.GetFileNameWithoutExtension(filePath),
                i,
                track.TrackType,
                (int)(track.Frames * track.DataSize));

            albumTracks.Add(albumTrack);

            absoluteTrackStartSector += fileSectors;
        }

        // Length of album is total of every file
        sectors.Add(absoluteTrackStartSector);

        string toc = string.Join(':', sectors);

        return new ChdAlbumMedia(cdRomFile, albumTracks, toc);
    }
}
