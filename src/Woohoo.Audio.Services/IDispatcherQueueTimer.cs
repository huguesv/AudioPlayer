// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Services;

public interface IDispatcherQueueTimer
{
    event EventHandler Tick;

    TimeSpan Interval { get; set; }

    bool IsRepeating { get; set; }

    bool IsRunning { get; }

    void Start();

    void Stop();
}
