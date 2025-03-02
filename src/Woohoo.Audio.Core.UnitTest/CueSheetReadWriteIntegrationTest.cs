// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.UnitTest;

using Woohoo.Audio.Core.Cue.Serialization;

public class CueSheetReadWriteIntegrationTest
{
    private readonly ITestOutputHelper outputHelper;

    public CueSheetReadWriteIntegrationTest(ITestOutputHelper outputHelper)
    {
        this.outputHelper = outputHelper;
    }

    [Fact]
    public void ReadWriteCompareAll()
    {
        string? folder = Environment.GetEnvironmentVariable("CUESHEET_INTEGRATION_TEST_ROOT_FOLDER");

        Assert.SkipWhen(string.IsNullOrEmpty(folder), "Environment variable CUESHEET_INTEGRATION_TEST_ROOT_FOLDER not set");

        Assert.SkipUnless(Directory.Exists(folder), $"Folder not found: {folder}");

        string[] files = Directory.GetFiles(folder, "*.cue", SearchOption.AllDirectories);
        Assert.SkipWhen(files.Length == 0, "No .cue files found in folder");

        this.outputHelper.WriteLine($"Found {files.Length} .cue files in folder: {folder}");

        foreach (var file in files)
        {
            var expected = File.ReadAllText(file);
            var sheet = new CueSheetReader().Parse(expected);
            var actual = new CueSheetWriter().Write(sheet);
            actual.Should().Be(expected);
        }
    }
}
