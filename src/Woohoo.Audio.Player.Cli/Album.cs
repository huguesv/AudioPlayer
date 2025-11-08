// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Cli;

using Woohoo.Audio.Core.Cue;
using Woohoo.Audio.Core.Cue.Serialization;
using Woohoo.Audio.Core.IO;

internal class Album
{
    public Album(IMusicContainer container, string cueSheetName, string cueSheetFileName, string cueSheetContainerPath, string title, string performer, IEnumerable<Track> tracks)
    {
        this.Container = container;
        this.CueSheetName = cueSheetName;
        this.CueSheetFileName = cueSheetFileName;
        this.CueSheetContainerPath = cueSheetContainerPath;
        this.Title = title;
        this.Performer = performer;
        this.Tracks.AddRange(tracks);
    }

    public IMusicContainer Container { get; }

    public string CueSheetName { get; set; }

    public string CueSheetFileName { get; set; }

    public string CueSheetContainerPath { get; set; }

    public string Title { get; set; }

    public string Performer { get; set; }

    public List<Track> Tracks { get; } = [];

    public static Album? LoadFrom(string path)
    {
        IMusicContainer? container = null;

        var ext = Path.GetExtension(path);
        if (ext == ".cue")
        {
            string? folder = Path.GetDirectoryName(path);
            if (folder is null)
            {
                return null;
            }

            container = new MusicFolderContainer(folder);
        }
        else if (ext == ".zip")
        {
            container = new MusicZipContainer(path);
        }

        if (container is null)
        {
            return null;
        }

        var cueSheetFilePath = container.EnumerateFilesByExtension("cue").FirstOrDefault();
        if (cueSheetFilePath is null)
        {
            return null;
        }

        var cueSheetName = Path.GetFileNameWithoutExtension(cueSheetFilePath);
        var cueSheetFileName = cueSheetFilePath;
        var cueSheetContainerPath = container.ContainerPath;

        var cueSheetData = container.ReadFileText(cueSheetFileName);

        CueSheetReader parser = new();
        CueSheet cueSheet = parser.Parse(cueSheetData);

        List<Track> tracks = [];
        foreach (var cueFile in cueSheet.Files)
        {
            var fileExists = container.FileExists(cueFile.FileName);
            long fileSize = fileExists ? container.GetFileSize(cueFile.FileName) : 0;

            var tracksInfo = new (int StartOffset, int PlayOffset)[cueFile.Tracks.Count + 1];
            for (int i = 0; i < cueFile.Tracks.Count; i++)
            {
                var firstIndexOffset = cueFile.Tracks[i].Indexes.FirstOrDefault()?.Time.ToBytes();
                var firstIndex1Offset = cueFile.Tracks[i].Indexes.Where(idx => idx.IndexNumber == 1).FirstOrDefault()?.Time.ToBytes();

                tracksInfo[i] = (StartOffset: firstIndexOffset ?? 0, PlayOffset: firstIndex1Offset ?? firstIndexOffset ?? 0);
            }

            tracksInfo[cueFile.Tracks.Count] = (StartOffset: (int)fileSize, PlayOffset: (int)fileSize);

            for (int i = 0; i < cueFile.Tracks.Count; i++)
            {
                var cueTrack = cueFile.Tracks[i];

                if (cueTrack.TrackMode != KnownTrackModes.Audio)
                {
                    continue;
                }

                tracks.Add(new Track
                {
                    FileName = cueFile.FileName,
                    FileNotFound = !fileExists,
                    TrackNumber = cueTrack.TrackNumber,
                    Title = cueTrack.Title ?? $"Track {cueTrack.TrackNumber}",
                    Performer = cueTrack.Performer ?? cueSheet.Performer ?? string.Empty,
                    Songwriter = cueTrack.Songwriter ?? cueSheet.Songwriter ?? string.Empty,
                    TrackOffset = fileExists ? tracksInfo[i].PlayOffset : 0,
                    TrackSize = fileExists ? tracksInfo[i + 1].StartOffset - tracksInfo[i].PlayOffset : 0,
                });
            }
        }

        return new Album(container, cueSheetName, cueSheetFileName, cueSheetContainerPath, cueSheet.Title ?? string.Empty, cueSheet.Performer ?? string.Empty, tracks);
    }
}
