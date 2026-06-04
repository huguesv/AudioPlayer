// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Services;

using System;

public interface IDispatcherQueue
{
    bool TryEnqueue(Action action);

    IDispatcherQueueTimer CreateTimer();
}
