// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Shared.Avalonia.Services.Impl;

using global::Avalonia.Threading;
using Woohoo.Audio.Services;

public sealed class DispatcherQueueService : IDispatcherQueueService
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
