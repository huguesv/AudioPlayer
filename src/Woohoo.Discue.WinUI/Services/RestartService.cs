// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Services;

using System.Threading.Tasks;
using Microsoft.Windows.AppLifecycle;
using Woohoo.Discue.Contracts.Services;

internal class RestartService : IRestartService
{
    public RestartService()
    {
    }

    public Task RestartAsync()
    {
        _ = AppInstance.Restart(string.Empty);
        return Task.CompletedTask;
    }
}
