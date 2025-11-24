// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.UnitTest;

using System;
using System.Text;
using Woohoo.Audio.Core.Lyrics;
using Woohoo.Audio.Core.Lyrics.Serialization;
using Xunit;

public class LrcReaderUnitTest
{
    [Fact]
    public void Parse_EmptyContent_ReturnsEmptySyncedLines()
    {
        // Arrange
        var content = string.Empty;

        // Act
        var actual = LrcReader.Parse(content);

        // Assert
        actual.Should().NotBeNull();
        actual.SyncedLines.Should().BeEmpty();
    }

    [Fact]
    public void Parse_LineWithHundredths_ParsesTimestampAndText()
    {
        // Arrange
        var lrc = "[00:12.34]Hello world";

        // Act
        var actual = LrcReader.Parse(lrc);

        // Assert
        actual.SyncedLines.Should().BeEquivalentTo(
        [
            new LyricsLine { Text = "Hello world", Timestamp = new TimeSpan(0, 0, 0, 12, 340) },
        ]);
    }

    [Fact]
    public void Parse_LineWithoutHundredths_ParsesTimestampAndText()
    {
        // Arrange
        var lrc = "[01:02]Another line"; // 1 minute 2 seconds

        // Act
        var actual = LrcReader.Parse(lrc);

        // Assert
        actual.SyncedLines.Should().BeEquivalentTo(
        [
            new LyricsLine { Text = "Another line", Timestamp = new TimeSpan(0, 0, 0, 62) },
        ]);
    }

    [Fact]
    public void Parse_InvalidLines_AreIgnored()
    {
        // Arrange
        var lrc = new StringBuilder()
            .AppendLine("Not a valid lrc line")
            .AppendLine("[xx:yy]bad")
            .AppendLine("[00:10]Good")
            .ToString();

        // Act
        var actual = LrcReader.Parse(lrc);

        // Assert
        actual.SyncedLines.Should().BeEquivalentTo(
        [
            new LyricsLine { Text = "Good", Timestamp = new TimeSpan(0, 0, 0, 10) },
        ]);
    }

    [Fact]
    public void Parse_MultipleValidLines_ParsesAllLines()
    {
        // Arrange
        var lrc = new StringBuilder()
            .AppendLine("[00:05]First")
            .AppendLine("[00:10]")
            .AppendLine("[00:15]Third")
            .ToString();

        // Act
        var actual = LrcReader.Parse(lrc);

        // Assert
        actual.SyncedLines.Should().BeEquivalentTo(
        [
            new LyricsLine { Text = "First", Timestamp = new TimeSpan(0, 0, 0, 5) },
            new LyricsLine { Text = string.Empty, Timestamp = new TimeSpan(0, 0, 0, 10) },
            new LyricsLine { Text = "Third", Timestamp = new TimeSpan(0, 0, 0, 15) },
        ]);
    }
}
