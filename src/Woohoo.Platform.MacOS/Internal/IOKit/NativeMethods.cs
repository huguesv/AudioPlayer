// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Platform.MacOS.Internal.IOKit;

using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

[SupportedOSPlatform("macos")]
internal static partial class NativeMethods
{
    private const string IOKitLibrary = "/System/Library/Frameworks/IOKit.framework/IOKit";

    public enum IOReturn : uint
    {
        Success = 0,
    }

    public enum IOPMAssertionLevel : uint
    {
        Off = 0,
        On = 255,
    }

    [LibraryImport(IOKitLibrary)]
    public static partial IOReturn IOPMAssertionCreateWithName(IntPtr assertionType, IOPMAssertionLevel assertionLevel, IntPtr assertionName, out uint assertionId);

    [LibraryImport(IOKitLibrary)]
    public static partial IOReturn IOPMAssertionRelease(uint assertionId);

    public static class AssertionTypeNames
    {
        public const string PreventUserIdleSystemSleep = "PreventUserIdleSystemSleep";
        public const string PreventUserIdleDisplaySleep = "PreventUserIdleDisplaySleep";
    }
}
