﻿using System.Collections.Concurrent;
using System.Diagnostics;
using BlockEngine.Client.Logging;
using BlockEngine.Client.Threading.Jobs;
using BlockEngine.Client.Threading.Threads;

namespace BlockEngine.Client.Threading.Pooling;

/// <summary>
/// Custom thread pool with progressive throttling.
/// </summary>
public sealed class ThreadPool
{
    private readonly BlockingCollection<IVektorJob> _workQueue;
    private readonly List<WorkerThread> _workers;


    /// <summary>
    /// Creates a new thread pool with the desired number of worker threads.
    /// </summary>
    /// <param name="threadCount">Number of workers threads to allocate.</param>
    /// <param name="config">The thread config to use for the workers threads.</param>
    public ThreadPool(uint threadCount, ThreadConfig config)
    {
        if (threadCount == 0)
            throw new ArgumentException("Thread count must be greater than zero!");

        _workQueue = new BlockingCollection<IVektorJob>(new ConcurrentQueue<IVektorJob>());
        _workers = new List<WorkerThread>();

        for (int i = 0; i < threadCount; i++)
            _workers.Add(new WorkerThread(_workQueue, config));
    }


    public void EnqueueWorkItem(IVektorJob vektorJob)
    {
        if (_workQueue.IsAddingCompleted)
            throw new InvalidOperationException("Cannot queue a work item if the pool is shutting down.");

        _workQueue.Add(vektorJob);
    }


    /// <summary>
    /// Disables adding of work items and waits for all workers to shutdown.
    /// You should only call this once you are done adding any work items.
    /// </summary>
    public void Shutdown()
    {
        _workQueue.CompleteAdding();

        // Wait for workers to terminate.
        while (true)
        {
            foreach (WorkerThread worker in _workers)
            {
                if (worker.Status != ThreadStatus.Offline)
                    continue;
                return;
            }
        }
    }


    /// <summary>
    /// Disables adding of work items and informs all workers to shutdown immediately.
    /// If any jobs are queued, they will be ignored.
    /// If the desired wait interval is exceeded, the worker threads will be aborted.
    /// </summary>
    /// <param name="waitInterval">How many cycles to wait before aborting the threads.</param>
    public void ShutdownNow(uint waitInterval = 2000)
    {
        _workQueue.CompleteAdding();

        foreach (WorkerThread worker in _workers)
            worker.Shutdown();

        // Wait some period for workers to terminate.
        uint count = 0u;
        while (true)
        {
            if (count >= waitInterval)
            {
                Logger.LogWarning("Thread pool shutdown wait interval exceeded, aborting threads.");
                break;
            }

            foreach (WorkerThread worker in _workers)
            {
                if (worker.Status != ThreadStatus.Offline) continue;
                return;
            }

            count++;
        }

        foreach (WorkerThread worker in _workers)
        {
            if (worker.Status == ThreadStatus.Offline) continue;
            worker.Abort();
        }
    }
}