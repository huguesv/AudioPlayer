// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Cli;

using System.CommandLine;
using System.IO;
using Woohoo.Audio.Core.Cue.Serialization;
using Woohoo.Audio.Core.Metadata;

internal class Program
{
    public static async Task<int> Main(string[] args)
    {
        ConsolePlayer.PrintCopyright();

        var fileArgument = new Argument<FileInfo>("file")
        {
            Description = "The .cue or .zip file to play.",
            Arity = ArgumentArity.ZeroOrOne,
        };

        var fetchOption = new Option<bool>(name: "--metadata", "-m")
        {
            Description = "Fetch metadata from online databases.",
        };

        var rootCommand = new RootCommand("Play tracks from a CD dump in bin/cue format.")
        {
            fileArgument,
            fetchOption,
        };

        rootCommand.SetAction(async (ParseResult parseResult) =>
        {
            var fileInfo = parseResult.GetValue(fileArgument);
            if (fileInfo is null)
            {
                Console.Error.WriteLine("No file specified. Please provide a .cue or .zip file name.");
                return 1;
            }

            if (!fileInfo.Exists)
            {
                Console.Error.WriteLine($"File '{fileInfo.FullName}' does not exist.");
                return 1;
            }

            var fetchMetadata = parseResult.GetValue(fetchOption);

            return await PlayCommandHandler(fileInfo, fetchMetadata);
        });

        var parseResult = rootCommand.Parse(args);
        return await parseResult.InvokeAsync();
    }

    private static async Task<int> PlayCommandHandler(FileInfo fileInfo, bool fetchMetadata)
    {
        var album = Album.LoadFrom(fileInfo.FullName);
        if (album is null)
        {
            Console.WriteLine($"Unable to load: {fileInfo.FullName}.\nOnly .cue or .zip files are supported.");
            return 1;
        }

        ConsolePlayer.ClearScreen();
        ConsolePlayer.PrintCopyright();

        if (fetchMetadata)
        {
            try
            {
                var metadataProvider = new MetadataProvider();
                var cueSheet = new CueSheetReader().Parse(album.Container.ReadFileText(album.CueSheetFileName));
                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var metadata = await metadataProvider.QueryAsync(cueSheet, album.Container, cancellationTokenSource.Token);
                if (metadata is not null)
                {
                    album.Title = BestString(metadata.Album, album.Title);
                    album.Performer = BestString(metadata.Artist, album.Performer);

                    for (int i = 0; i < album.Tracks.Count; i++)
                    {
                        var track = album.Tracks[i];
                        var dbTrack = metadata.Tracks[i];
                        if (dbTrack is not null)
                        {
                            track.Title = BestString(dbTrack.Name, track.Title);
                            track.Performer = BestString(dbTrack.Artist, metadata.Artist, track.Performer);
                        }
                    }

                    Console.WriteLine("Metadata fetched successfully.");
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("Metadata not available.");
                    Console.WriteLine();
                }
            }
            catch (OperationCanceledException)
            {
                Console.Error.WriteLine("Metadata fetch timed out.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Metadata fetch error: " + ex.Message);
            }
        }

        ConsolePlayer.PrintCommands();

        Console.CursorVisible = false;
        try
        {
            var player = new ConsolePlayer();
            player.LoadAlbum(album);
            player.PlayAll();

            while (player.HandleKey(Console.ReadKey(intercept: true).Key))
            {
            }
        }
        finally
        {
            Console.CursorVisible = true;
        }

        return 0;

        static string BestString(params string[] values)
        {
            return values.FirstOrDefault(val => !string.IsNullOrEmpty(val)) ?? string.Empty;
        }
    }
}
