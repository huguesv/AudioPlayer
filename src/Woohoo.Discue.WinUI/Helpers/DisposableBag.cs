// Copyright (c) Hugues Valois. All rights reserved.
// Licensed under the MIT license. See LICENSE in the project root for license information.

namespace Woohoo.Discue.Helpers;

using System;
using System.Collections.Concurrent;
using System.Threading;

internal sealed class DisposableBag
{
    private readonly string exceptionObjectName;
    private readonly string? exceptionMessage;
    private ConcurrentStack<Action>? disposables;

    public DisposableBag(string exceptionObjectName, string? exceptionMessage = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(exceptionObjectName);

        this.exceptionObjectName = exceptionObjectName;
        this.exceptionMessage = exceptionMessage;
        this.disposables = new ConcurrentStack<Action>();
    }

    public static DisposableBag Create(IDisposable instance)
        => Create(instance.GetType());

    public static DisposableBag Create<T>()
        where T : IDisposable
        => Create(typeof(T));

    public DisposableBag Add(IDisposable disposable)
        => this.Add(disposable.Dispose);

    public DisposableBag Add(Action action)
    {
        this.disposables?.Push(action);
        this.ThrowIfDisposed();
        return this;
    }

    public bool TryAdd(IDisposable disposable)
        => this.TryAdd(disposable.Dispose);

    public bool TryAdd(Action action)
    {
        this.disposables?.Push(action);
        return this.disposables != null;
    }

    public void ThrowIfDisposed()
    {
        if (this.disposables is not null)
        {
            return;
        }

        if (this.exceptionMessage is not null)
        {
            throw new ObjectDisposedException(this.exceptionObjectName, this.exceptionMessage);
        }
        else
        {
            throw new ObjectDisposedException(this.exceptionObjectName);
        }
    }

    public bool TryDispose()
    {
        var disposables = Interlocked.Exchange(ref this.disposables, null);
        if (disposables == null)
        {
            return false;
        }

        foreach (var disposable in disposables)
        {
            disposable();
        }

        return true;
    }

    private static DisposableBag Create(Type type)
        => new(type.Name, FormattableString.Invariant($"{type.Name} instance is disposed"));
}
