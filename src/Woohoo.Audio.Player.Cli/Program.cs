// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Cli;

using System.CommandLine;
using System.IO;
using System.Linq;
using Woohoo.Audio.Core;
using Woohoo.Audio.Core.Lyrics;
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
                Console.Error.WriteLine("No file specified. Please provide a .cue or .zip file name.");
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
            var tracks = new AlbumLoader().LoadFrom(fileInfo.FullName);
            ConsolePlayer.ClearScreen();
            ConsolePlayer.PrintCopyright();

            if (fetchMetadata && tracks.Count > 0)
            {
                Console.Write("Fetching metadata... ");

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

                        Console.WriteLine("success");
                    }
                    else
                    {
                        Console.WriteLine("not available");
                    }
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("timed out");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error: " + ex.Message);
                }
            }

            if (fetchLyrics && tracks.Count > 0)
            {
                Console.Write("Fetching lyrics... ");

                var lyricsOptions = new LyricsProviderOptions
                {
                    UseWeb = true,
                    UseWebExternalSources = true,
                    UseDatabase = lrclibDbFileInfo?.Exists == true,
                    DatabaseFilePath = lrclibDbFileInfo?.FullName,
                };

                var lyricsProvider = new LyricsProvider(lyricsOptions);

                int lyricsCount = 0;

                foreach (var track in tracks)
                {
                    if (string.IsNullOrEmpty(track.TrackTitle) ||
                        string.IsNullOrEmpty(track.AlbumPerformer) ||
                        string.IsNullOrEmpty(track.AlbumTitle))
                    {
                        continue;
                    }

                    try
                    {
                        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                        var lyrics = await lyricsProvider.QueryAsync(
                            track.AlbumTitle,
                            track.TrackPerformer,
                            track.TrackTitle,
                            TimeConversion.FromPosition(track.TrackSize),
                            cancellationTokenSource.Token);

                        if (lyrics is not null)
                        {
                            track.Lyrics = lyrics;
                            lyricsCount++;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Console.WriteLine("timed out");
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("error: " + ex.Message);
                        break;
                    }
                }

                if (lyricsCount > 0)
                {
                    Console.WriteLine($"success ({lyricsCount} / {tracks.Count})");
                }
                else
                {
                    Console.WriteLine("not available");
                }
            }

            Console.WriteLine();
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
