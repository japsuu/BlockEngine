﻿using System.Runtime.CompilerServices;

namespace BlockEngine.Client.Threading.Jobs;

public interface IAwaitable : INotifyCompletion
{
    public bool IsCompleted { get; }

    public void GetResult();

    public IAwaitable GetAwaiter();
}

public interface IAwaitable<out T> : INotifyCompletion
{
    public bool IsCompleted { get; }

    public T GetResult();

    public IAwaitable<T> GetAwaiter();
}