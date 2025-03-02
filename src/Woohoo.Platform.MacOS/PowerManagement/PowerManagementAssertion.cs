// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Platform.MacOS.PowerManagement;

using System;
using System.Runtime.Versioning;
using Woohoo.Platform.MacOS.Internal.CoreFoundation;
using static Woohoo.Platform.MacOS.Internal.IOKit.NativeMethods;

[SupportedOSPlatform("macos")]
public sealed class PowerManagementAssertion : IDisposable
{
    private uint? assertionId;

    private PowerManagementAssertion(uint assertionId)
    {
        this.assertionId = assertionId;
    }

    /// <summary>
    /// Creates an assertion which allows an application to return a brief
    /// string to the user explaining why that application is preventing sleep.
    /// </summary>
    /// <param name="reason">
    /// Descriptive string used by the system whenever it needs to tell the
    /// user why the system is not sleeping. Limited to 128 characters.
    /// </param>
    /// <param name="type">The type of sleep to prevent.</param>
    /// <returns>
    /// Assertion object that should be disposed to release the assertion and
    /// be able to sleep again.
    /// </returns>
    /// <exception cref="InvalidOperationException">Error from level api.</exception>
    public static PowerManagementAssertion Create(string reason, PowerManagementAssertionType type)
    {
        ArgumentException.ThrowIfNullOrEmpty(reason);

        using CFString assertionType = type switch
        {
            PowerManagementAssertionType.PreventSystemSleep => CFString.Create(AssertionTypeNames.PreventUserIdleSystemSleep),
            PowerManagementAssertionType.PreventDisplaySleep => CFString.Create(AssertionTypeNames.PreventUserIdleDisplaySleep),
            _ => throw new ArgumentException("Invalid assertion type.", nameof(type)),
        };

        using CFString assertionName = CFString.Create(reason);

        IOReturn result = IOPMAssertionCreateWithName(
            assertionType,
            IOPMAssertionLevel.On,
            assertionName,
            out uint assertionId);

        if (result != IOReturn.Success)
        {
            throw new InvalidOperationException($"Error {result} from IOPMAssertionCreateWithName.");
        }

        return new PowerManagementAssertion(assertionId);
    }

    public void Dispose()
    {
        if (this.assertionId.HasValue)
        {
            IOReturn result = IOPMAssertionRelease(this.assertionId.Value);
            this.assertionId = null;
            if (result != IOReturn.Success)
            {
                throw new InvalidOperationException($"Error {result} from IOPMAssertionRelease.");
            }
        }
    }
}
