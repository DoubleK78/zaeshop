using System.Collections.Concurrent;
using Microsoft.Data.SqlClient;

namespace Portal.Infrastructure.Helpers;

public static class HangfireCircuitBreaker
{
    private static bool _isOpen = false;
    private static DateTime _lastAttempt = DateTime.UtcNow;
    private static readonly TimeSpan _resetTimeout = TimeSpan.FromMinutes(5);
    private static readonly ConcurrentQueue<Action> _backlogQueue = new ConcurrentQueue<Action>();
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public static async Task<bool> IsOpenAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_isOpen && DateTime.UtcNow - _lastAttempt > _resetTimeout)
            {
                _isOpen = false;

                if (_semaphore.CurrentCount == 0)
                {
                    _semaphore.Release();
                }

                await ProcessBacklogAsync(); // Fire and forget
            }
            return _isOpen;
        }
        finally
        {
            if (_semaphore.CurrentCount == 0)
            {
                _semaphore.Release();
            }
        }
    }

    public static async Task OpenAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            _isOpen = true;
            _lastAttempt = DateTime.UtcNow;
        }
        finally
        {
            if (_semaphore.CurrentCount == 0)
            {
                _semaphore.Release();
            }
        }
    }

    public static async Task CloseAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            _isOpen = false;
            _lastAttempt = DateTime.UtcNow;
        }
        finally
        {
            if (_semaphore.CurrentCount == 0)
            {
                _semaphore.Release();
            }
        }
    }

    private static async Task ProcessBacklogAsync()
    {
        const int maxAttempts = 1000; // Limit the number of attempts to prevent long-running loops

        for (int i = 0; i < maxAttempts && _backlogQueue.TryDequeue(out var job); i++)
        {
            try
            {
                job();

                // Delay to avoid throttling, Delay for 0.5 second
                await Task.Delay(500);
            }
            catch (SqlException)
            {
                await OpenAsync();
                _backlogQueue.Enqueue(job);
                break;
            }
        }

        // Close the circuit if there are no more jobs in the backlog
        if (_backlogQueue.IsEmpty)
        {
            await CloseAsync();
        }
    }

    public static async Task EnqueueAsync(Action job)
    {
        if (await IsOpenAsync())
        {
            _backlogQueue.Enqueue(job);
        }
        else
        {
            try
            {
                job();
            }
            catch (SqlException)
            {
                await OpenAsync();
                _backlogQueue.Enqueue(job);
            }
        }
    }
}
