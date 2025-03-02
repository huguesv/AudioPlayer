// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.UnitTest;

using Woohoo.Audio.Core.Cue;
using Woohoo.Audio.Core.Cue.Serialization;

public class CueSheetReaderUnitTest
{
    [Fact]
    public void Parse()
    {
        // Arrange
        string cueContent = """
FILE "Drama CD (Japan) (PS3 Game Bundle).bin" BINARY
  TRACK 01 AUDIO
    INDEX 01 00:00:00
""";

        var reader = new CueSheetReader();

        // Act
        var result = reader.Parse(cueContent);

        // Assert
        var expected = new CueSheet
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

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ParseCdText()
    {
        // Arrange
        string cueContent = """
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
""";

        var reader = new CueSheetReader();

        // Act
        var result = reader.Parse(cueContent);

        // Assert
        var expected = new CueSheet
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

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void ParseFlags()
    {
        // Arrange
        string cueContent = """
FILE "Track 1.bin" BINARY
  TRACK 01 MODE1/2352
    INDEX 01 00:00:00
FILE "Track 2.bin" BINARY
  TRACK 02 AUDIO
    FLAGS PRE
    INDEX 00 00:00:00
    INDEX 01 00:01:74
""";

        var reader = new CueSheetReader();

        // Act
        var result = reader.Parse(cueContent);

        // Assert
        var expected = new CueSheet
        {
            Files =
            [
                new CueFile
                {
                    FileName = "Track 1.bin",
                    FileFormat = "BINARY",
                    Tracks =
                    [
                        new CueTrack
                        {
                            TrackNumber = 1,
                            TrackMode = "MODE1/2352",
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
                new CueFile
                {
                    FileName = "Track 2.bin",
                    FileFormat = "BINARY",
                    Tracks =
                    [
                        new CueTrack
                        {
                            TrackNumber = 2,
                            TrackMode = "AUDIO",
                            Flags = ["PRE"],
                            Indexes =
                            [
                                new CueIndex
                                {
                                    IndexNumber = 0,
                                    Time = new CueTime { Minutes = 0, Seconds = 0, Frames = 0 },
                                },
                                new CueIndex
                                {
                                    IndexNumber = 1,
                                    Time = new CueTime { Minutes = 0, Seconds = 1, Frames = 74 },
                                },
                            ],
                        },
                    ],
                },
            ],
        };

        result.Should().BeEquivalentTo(expected);
    }
}
