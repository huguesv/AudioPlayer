// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.UnitTest;

using Woohoo.Audio.Core.Cue;
using Woohoo.Audio.Core.Cue.Serialization;

public class CueSheetWriterUnitTest
{
    [Fact]
    public void Write()
    {
        // Arrange
        var cueSheet = new CueSheet
        {
            Files =
            [
                new CueFile
                {
                    FileName = "Drama CD (Japan) (PS3 Game Bundle).bin",
                    FileFormat = "BINARY",
                    Tracks =
                    [
                        new CueTrack
                        {
                            TrackNumber = 1,
                            TrackMode = "AUDIO",
                            Indexes =
                            [
                                new CueIndex
                                {
                                    IndexNumber = 1,
                                    Time = new CueTime { Minutes = 0, Seconds = 0, Frames = 0 },
                                },
                            ],
                        },
                    ],
                },
            ],
        };

        var writer = new CueSheetWriter();

        // Act
        var result = writer.Write(cueSheet);

        // Assert
        string expected = """
FILE "Drama CD (Japan) (PS3 Game Bundle).bin" BINARY
  TRACK 01 AUDIO
    INDEX 01 00:00:00

""".Replace("\r\n", "\n").Replace("\n", "\r\n"); // newline normalization

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void WriteCdText()
    {
        // Arrange
        var cueSheet = new CueSheet
        {
            Arranger = "Album Arranger",
            Composer = "Album Composer",
            Performer = "Album Performer",
            Songwriter = "Album Songwriter",
            Title = "Album Title",
            Files =
            [
                new CueFile
                {
                    FileName = "Drama CD (Japan) (PS3 Game Bundle).bin",
                    FileFormat = "BINARY",
                    Tracks =
                    [
                        new CueTrack
                        {
                            TrackNumber = 1,
                            TrackMode = "AUDIO",
                            Title = "Track Title",
                            Arranger = "Track Arranger",
                            Composer = "Track Composer",
                            Performer = "Track Performer",
                            Songwriter = "Track Songwriter",
                            ISRC = "QM9AA1871046",
                            Indexes =
                            [
                                new CueIndex
                                {
                                    IndexNumber = 1,
                                    Time = new CueTime { Minutes = 0, Seconds = 0, Frames = 0 },
                                },
                            ],
                        },
                    ],
                },
            ],
        };

        var writer = new CueSheetWriter();

        // Act
        var result = writer.Write(cueSheet);

        // Assert
        string expected = """
TITLE "Album Title"
ARRANGER "Album Arranger"
COMPOSER "Album Composer"
PERFORMER "Album Performer"
SONGWRITER "Album Songwriter"
FILE "Drama CD (Japan) (PS3 Game Bundle).bin" BINARY
  TRACK 01 AUDIO
    TITLE "Track Title"
    ARRANGER "Track Arranger"
    COMPOSER "Track Composer"
    PERFORMER "Track Performer"
    SONGWRITER "Track Songwriter"
    ISRC QM9AA1871046
    INDEX 01 00:00:00

""".Replace("\r\n", "\n").Replace("\n", "\r\n"); // newline normalization

        result.Should().BeEquivalentTo(expected);
    }
}
