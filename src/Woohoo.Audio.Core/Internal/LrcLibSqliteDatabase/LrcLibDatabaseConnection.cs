// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Core.Internal.LrcLibSqliteDatabase;

using Microsoft.EntityFrameworkCore;

public sealed class LrcLibDatabaseConnection
{
    public LrcLibDatabaseConnection(string databaseFilePath)
    {
        this.Context = new LrcLibContext(new DbContextOptions<LrcLibContext>(), databaseFilePath);
    }

    public LrcLibContext Context { get; }
}
