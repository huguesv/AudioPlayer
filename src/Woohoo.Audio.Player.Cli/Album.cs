// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

#define PLAY_USING_STREAM

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

    public string Performer { get; }

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
            foreach (var cueTrack in cueFile.Tracks)
            {
                if (cueTrack.TrackMode != KnownTrackModes.Audio)
                {
                    continue;
                }

                if (!container.FileExists(cueFile.FileName))
                {
                    continue;
                }

                tracks.Add(new Track
                {
                    FileName = cueFile.FileName,
                    TrackNumber = cueTrack.TrackNumber,
                    Title = cueTrack.Title ?? string.Empty,
                    Performer = cueTrack.Performer ?? cueSheet.Performer ?? string.Empty,
                    FileSize = container.GetFileSize(cueFile.FileName),
                });
            }
        }

        return new Album(container, cueSheetName, cueSheetFileName, cueSheetContainerPath, cueSheet.Title ?? string.Empty, cueSheet.Performer ?? string.Empty, tracks);
    }
}
