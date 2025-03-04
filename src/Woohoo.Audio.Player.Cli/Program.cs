// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Cli;

internal class Program
{
    public static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            ConsolePlayer.PrintCopyright();
            ConsolePlayer.PrintUsage();
            return;
        }

        var path = args[0];
        if (!File.Exists(path))
        {
            Console.WriteLine($"File not found: {path}");
            return;
        }

        var album = Album.LoadFrom(path);
        if (album is not null)
        {
            ConsolePlayer.ClearScreen();
            ConsolePlayer.PrintCopyright();
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
        }
        else
        {
            Console.WriteLine($"Unable to load: {path}.\nOnly .cue or .zip files are supported.");
        }
    }
}
