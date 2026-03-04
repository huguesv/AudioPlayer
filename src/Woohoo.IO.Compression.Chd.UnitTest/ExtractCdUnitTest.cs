// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.Chd.UnitTest;

using AwesomeAssertions;
using Woohoo.IO.Compression.Chd.UnitTest.Infrastructure;
using Woohoo.Security.Cryptography;

public class ExtractCdUnitTest
{
    private static string TestDataFolder
        => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Data");

    [Theory]
    [InlineData("audio_silence_wav_20_tracks")]
    [InlineData("audio_silence_wav_20_tracks_cdfl")]
    [InlineData("audio_silence_wav_20_tracks_cdlz")]
    [InlineData("audio_silence_wav_20_tracks_cdzl")]
    [InlineData("audio_silence_wav_20_tracks_cdzs")]
    [InlineData("audio_silence_wav_20_tracks_none")]
    public void ExtractCdTracksAndValidateChecksum(string folderName)
    {
        // Arrange
        using var tempFolder = new DisposableTempFolder();
        var chdFilePath = Path.Combine(TestDataFolder, folderName, "in.chd");
        using var chdFile = new ChdFile(chdFilePath);
        var cdRomFile = new CdRomFile(chdFile);

        // Act
        var trackFileNames = cdRomFile.ExtractTracksTo(tempFolder.Path);

        // Assert
        var hashCalc = new HashCalculator();
        var expectedSha1 = "9c7afd195159f02e92b16397f17fb602f7096ae3";

        trackFileNames.Should().HaveCount(20);
        foreach (var trackFilePath in trackFileNames)
        {
            File.Exists(trackFilePath).Should().BeTrue(trackFilePath);

            var hashResult = hashCalc.Calculate(["sha1"], trackFilePath);
            var actualSha1 = HashCalculator.HexToString(hashResult.Checksums["sha1"]);
            actualSha1.Should().Be(expectedSha1, $"{trackFilePath} sha1 does not match.");
        }
    }
}
