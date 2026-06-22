// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.LrcDatabase.Sqlite;

using Microsoft.Data.Sqlite;

public sealed class LrcLibDatabaseConnection
{
    public LrcLibDatabaseConnection(string databaseFilePath)
    {
        this.SqliteConnection = new SqliteConnection($"Data Source={databaseFilePath}");
    }

    internal SqliteConnection SqliteConnection { get; }
}
