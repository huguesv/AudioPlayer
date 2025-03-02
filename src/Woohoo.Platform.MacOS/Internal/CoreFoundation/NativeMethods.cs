// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Platform.MacOS.Internal.CoreFoundation;

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

[SupportedOSPlatform("macos")]
internal static partial class NativeMethods
{
    private const string CoreFoundationLibrary = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

    public static IntPtr CreateCFString(string text)
        => CFStringCreateWithCString(IntPtr.Zero, text, 0x08000100); // kCFStringEncodingUTF8

    [LibraryImport(CoreFoundationLibrary, StringMarshalling = StringMarshalling.Utf8)]
    public static partial IntPtr CFStringCreateWithCString(IntPtr alloc, string cStr, uint encoding);

    [LibraryImport(CoreFoundationLibrary)]
    public static partial void CFRelease(IntPtr handle);

    [LibraryImport(CoreFoundationLibrary)]
    public static partial long CFStringGetLength(IntPtr handle);
}
