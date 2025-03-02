// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Services;

using System.Runtime.Versioning;
using Woohoo.Platform.MacOS.PowerManagement;

[SupportedOSPlatform("macos")]
internal class MacOSPowerManagementService : IPowerManagementService
{
    private PowerManagementAssertion? assertion;

    public void PreventSleep()
    {
        if (this.assertion is null)
        {
            this.assertion = PowerManagementAssertion.Create("Playing Music", PowerManagementAssertionType.PreventSystemSleep);
        }
    }

    public void RestoreSleep()
    {
        if (this.assertion is not null)
        {
            this.assertion.Dispose();
            this.assertion = null;
        }
    }
}
