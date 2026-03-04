// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.IO.Compression.Chd.UnitTest.Infrastructure;

internal sealed class DisposableTempFolder : IDisposable
{
    public DisposableTempFolder()
    {
        this.Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(this.Path);
    }

    public string Path { get; }

    public void Dispose()
    {
        if (Directory.Exists(this.Path))
        {
            Directory.Delete(this.Path, true);
        }
    }
}
