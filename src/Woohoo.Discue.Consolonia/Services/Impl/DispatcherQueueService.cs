// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Audio.Player.Tui.Services;

using System;
using Avalonia.Threading;
using Woohoo.Audio.Services;

internal class DispatcherQueueService : IDispatcherQueueService
{
    public IDispatcherQueue GetDispatcherQueue()
    {
        return new DispatcherQueue(Dispatcher.UIThread);
    }

    private sealed class DispatcherQueue : IDispatcherQueue
    {
        private readonly Dispatcher uIThread;

        public DispatcherQueue(Dispatcher uIThread)
        {
            this.uIThread = uIThread;
        }

        public IDispatcherQueueTimer CreateTimer()
        {
            throw new NotImplementedException();
        }

        public bool TryEnqueue(Action action)
        {
            this.uIThread.Post(action);
            return true;
        }
    }
}
