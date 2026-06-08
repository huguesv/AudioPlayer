// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Services;

using Windows.System;
using Woohoo.Audio.Services;

internal sealed partial class DispatcherQueueService : IDispatcherQueueService
{
    public IDispatcherQueue GetDispatcherQueue()
    {
        return new WindowsSystemDispatcherQueue(DispatcherQueue.GetForCurrentThread());
    }

    private sealed class WindowsSystemDispatcherQueue : IDispatcherQueue
    {
        private readonly DispatcherQueue dispatcherQueue;

        public WindowsSystemDispatcherQueue(DispatcherQueue dispatcherQueue)
        {
            this.dispatcherQueue = dispatcherQueue;
        }

        public IDispatcherQueueTimer CreateTimer()
        {
            return new WindowsSystemDispatcherQueueTimer(this.dispatcherQueue.CreateTimer());
        }

        public bool TryEnqueue(Action action)
        {
            return this.dispatcherQueue.TryEnqueue(() => action());
        }
    }

    private sealed partial class WindowsSystemDispatcherQueueTimer : IDispatcherQueueTimer, IDisposable
    {
        private readonly DispatcherQueueTimer dispatcherQueueTimer;

        public WindowsSystemDispatcherQueueTimer(DispatcherQueueTimer dispatcherQueueTimer)
        {
            this.dispatcherQueueTimer = dispatcherQueueTimer;
            this.dispatcherQueueTimer.Tick += this.OnTick;
        }

        public event EventHandler? Tick;

        public TimeSpan Interval
        {
            get => this.dispatcherQueueTimer.Interval;
            set => this.dispatcherQueueTimer.Interval = value;
        }

        public bool IsRepeating
        {
            get => this.dispatcherQueueTimer.IsRepeating;
            set => this.dispatcherQueueTimer.IsRepeating = value;
        }

        public bool IsRunning => this.dispatcherQueueTimer.IsRunning;

        public void Start()
        {
            this.dispatcherQueueTimer.Start();
        }

        public void Stop()
        {
            this.dispatcherQueueTimer.Stop();
        }

        public void Dispose()
        {
            this.dispatcherQueueTimer.Tick -= this.OnTick;
        }

        private void OnTick(object? sender, object args)
        {
            this.Tick?.Invoke(this, EventArgs.Empty);
        }
    }
}
