// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Avalonia.Services.DesignTime;

using Woohoo.Discue.Avalonia.Services;

internal class NullPowerManagementService : IPowerManagementService
{
    public void PreventSleep()
    {
    }

    public void RestoreSleep()
    {
    }
}
