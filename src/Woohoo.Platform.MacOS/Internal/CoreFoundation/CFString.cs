// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Platform.MacOS.Internal.CoreFoundation;

using System;
using System.Runtime.Versioning;

[SupportedOSPlatform("macos")]
internal sealed class CFString : IDisposable
{
    private nint handle;
    private bool disposedValue;

    private CFString(nint handle)
    {
        this.handle = handle;
    }

    ~CFString()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: false);
    }

    public static implicit operator nint(CFString obj)
    {
        return obj.handle;
    }

    public static CFString Create(string text)
    {
        return new CFString(NativeMethods.CreateCFString(text));
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                // Dispose managed state (managed objects)
            }

            // Free unmanaged resources (unmanaged objects) and override finalizer
            if (this.handle != nint.Zero)
            {
                NativeMethods.CFRelease(this.handle);
                this.handle = nint.Zero;
            }

            this.disposedValue = true;
        }
    }
}
