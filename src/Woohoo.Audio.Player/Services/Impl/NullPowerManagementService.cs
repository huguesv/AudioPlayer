// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Services;

internal class NullPowerManagementService : IPowerManagementService
{
    public void PreventSleep()
    {
    }

    public void RestoreSleep()
    {
    }
}
