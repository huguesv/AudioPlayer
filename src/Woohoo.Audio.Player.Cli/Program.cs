// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Cli;

using System.CommandLine;
using System.IO;
using System.Linq;
using Woohoo.Audio.Core;
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
        try
        {
            var tracks = new AlbumLoader().LoadFrom(fileInfo.FullName);
            ConsolePlayer.ClearScreen();
            ConsolePlayer.PrintCopyright();

            if (fetchMetadata && tracks.Count > 0)
            {
                try
                {
                    var metadataProvider = new MetadataProvider();
                    var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    var metadata = await metadataProvider.QueryAsync(tracks[0].CueSheet, tracks[0].Container, cancellationTokenSource.Token);
                    if (metadata is not null)
                    {
                        var albumTitle = BestString(metadata.Album, tracks[0].AlbumTitle);
                        var albumPerformer = BestString(metadata.Artist, tracks[0].AlbumPerformer);

                        for (int i = 0; i < tracks.Count; i++)
                        {
                            var track = tracks[i];
                            track.AlbumTitle = albumTitle;
                            track.AlbumPerformer = albumPerformer;

                            var dbTrack = metadata.Tracks[i];
                            if (dbTrack is not null)
                            {
                                track.TrackTitle = BestString(dbTrack.Name, track.TrackTitle);
                                track.TrackPerformer = BestString(dbTrack.Artist, metadata.Artist, track.TrackPerformer);
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
                player.LoadAlbum(tracks);
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
        }
        catch
        {
            Console.WriteLine($"Unable to load: {fileInfo.FullName}.\nOnly .cue or .zip files are supported.");
            return 1;
        }

        static string BestString(params string[] values)
        {
            return values.FirstOrDefault(val => !string.IsNullOrEmpty(val)) ?? string.Empty;
        }
    }
}
