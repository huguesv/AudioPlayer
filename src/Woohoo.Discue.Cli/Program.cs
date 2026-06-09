// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Cli;

using System.CommandLine;
using System.IO;
using Woohoo.Audio.Services.Impl;
using Woohoo.Discue.Cli.Services;

internal class Program
{
    public static async Task<int> Main(string[] args)
    {
        ConsolePlayer.PrintCopyright();

        var fileArgument = new Argument<FileInfo>("file")
        {
            Description = "The .cue, .zip, or .chd file to play.",
            Arity = ArgumentArity.ZeroOrOne,
        };

        var fetchOption = new Option<bool>(name: "--metadata", "-m")
        {
            Description = "Fetch metadata from online database.",
        };

        var lyricsOption = new Option<bool>(name: "--lyrics", "-l")
        {
            Description = "Fetch lyrics from online or local database.",
        };

        var lyricsDatabaseOption = new Option<FileInfo?>(name: "--lyrics-db", "-ldb")
        {
            Description = "Path to the LRCLIB lyrics database local .sqlite3 file.",
        };

        var rootCommand = new RootCommand("Play tracks from a CD dump in bin/cue format.")
        {
            fileArgument,
            fetchOption,
            lyricsOption,
            lyricsDatabaseOption,
        };

        rootCommand.SetAction(async (ParseResult parseResult) =>
        {
            var fileInfo = parseResult.GetValue(fileArgument);
            if (fileInfo is null)
            {
                Console.Error.WriteLine("No file specified. Please provide a .cue, .zip or .chd file name.");
                return 1;
            }

            if (!fileInfo.Exists)
            {
                Console.Error.WriteLine($"File '{fileInfo.FullName}' does not exist.");
                return 1;
            }

            var fetchMetadata = parseResult.GetValue(fetchOption);
            var fetchLyrics = parseResult.GetValue(lyricsOption);
            var lrclibDbFilePath = parseResult.GetValue(lyricsDatabaseOption);

            return await PlayCommandHandler(fileInfo, fetchMetadata, fetchLyrics, lrclibDbFilePath);
        });

        var parseResult = rootCommand.Parse(args);
        return await parseResult.InvokeAsync();
    }

    private static async Task<int> PlayCommandHandler(FileInfo fileInfo, bool fetchMetadata, bool fetchLyrics, FileInfo? lrclibDbFileInfo)
    {
        try
        {
            ConsolePlayer.ClearScreen();
            ConsolePlayer.PrintCopyright();

            Console.WriteLine();
            ConsolePlayer.PrintCommands();

            Console.CursorVisible = false;
            try
            {
                var mediaPlayerService = new MediaPlayerService(
                    new FixedMruService(),
                    new FixedBitmapCacheService(),
                    new FixedLocalSettingsService()
                    {
                        QueryMetadataOnline = fetchMetadata,
                        QueryLyricsOnline = fetchLyrics,
                        QueryLyricsOffline = fetchLyrics && lrclibDbFileInfo is not null,
                    },
                    new FixedAudioPlayerProvider());

                var player = new ConsolePlayer(mediaPlayerService);

                await player.LoadAlbumAsync(fileInfo.FullName);

                while (player.HandleKey(Console.ReadKey(intercept: true).Key))
                {
                }
            }
            finally
            {
                Console.CursorVisible = true;
            }

            return 0;
        }
        catch
        {
            Console.WriteLine($"Unable to load: {fileInfo.FullName}.\nOnly .cue, .zip, or .chd files are supported.");
            return 1;
        }
    }
}
