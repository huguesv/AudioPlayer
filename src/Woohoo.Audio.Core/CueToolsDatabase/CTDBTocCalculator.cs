// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.CueToolsDatabase;

using System.Collections.Generic;
using System.Linq;
using Woohoo.Audio.Core.Cue;
using Woohoo.Audio.Core.IO;

public static class CTDBTocCalculator
{
    public static string GetTocFromCue(CueSheet cueSheet, IMusicContainer container)
    {
        return GetTocFromCue(cueSheet, container.GetFileSize);
    }

    public static string GetTocFromCue(CueSheet cueSheet, string workingFolder)
    {
        return GetTocFromCue(cueSheet, filePath => GetFileSize(workingFolder, filePath));

        static long GetFileSize(string workingFolder, string fileName)
        {
            var filePath = Path.IsPathRooted(fileName) ? fileName : Path.Combine(workingFolder, fileName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("CUE file references missing file.", filePath);
            }

            return new FileInfo(filePath).Length;
        }
    }

    public static string GetTocFromCue(CueSheet cueSheet, Func<string, long> fileSizeProvider)
    {
        List<int> sectors = [];

        int absoluteTrackStartSector = 0;
        foreach (var file in cueSheet.Files)
        {
            long fileSizeBytes = fileSizeProvider(file.FileName);
            if (fileSizeBytes % 2352 != 0)
            {
                throw new InvalidDataException("Invalid file size for CD audio.");
            }

            int fileSectors = (int)(fileSizeBytes / 2352);
            foreach (var track in file.Tracks)
            {
                var firstIndex = track.Indexes.Where(idx => idx.IndexNumber == 1).FirstOrDefault();
                if (firstIndex is not null)
                {
                    int trackStartSector = absoluteTrackStartSector + firstIndex.Time.ToSectors();
                    sectors.Add(trackStartSector);
                }
            }

            absoluteTrackStartSector += fileSectors;
        }

        // Length of album is total of every file
        sectors.Add(absoluteTrackStartSector);

        return string.Join(':', sectors);
    }
}
