using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Schedly;

/// <summary>
/// A lightweight scheduler for executing jobs with built-in retry logic and exponential backoff
/// </summary>
public class LightweightScheduler : IDisposable
{
    private readonly SchedulerOptions _options;
    private readonly ConcurrentQueue<ScheduledJob> _jobQueue = new();
    private readonly SemaphoreSlim _concurrencyLimit;
    private readonly Timer _schedulerTimer;
    private volatile bool _disposed = false;

    /// <summary>
    /// Creates a new lightweight scheduler with default options
    /// </summary>
    public LightweightScheduler() : this(new SchedulerOptions()) { }

    /// <summary>
    /// Creates a new lightweight scheduler with the specified options
    /// </summary>
    /// <param name="options">Configuration options for the scheduler</param>
    public LightweightScheduler(SchedulerOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _concurrencyLimit = new SemaphoreSlim(_options.MaxConcurrentJobs, _options.MaxConcurrentJobs);
        
        // Start the scheduler timer
        _schedulerTimer = new Timer(ProcessScheduledJobs, null, 
            TimeSpan.FromMilliseconds(_options.SchedulerIntervalMs),
            TimeSpan.FromMilliseconds(_options.SchedulerIntervalMs));
    }

    /// <summary>
    /// Schedules a job to run immediately
    /// </summary>
    /// <param name="job">The job to execute</param>
    /// <param name="jobName">Optional name for the job</param>
    /// <returns>Job ID for tracking</returns>
    public string Schedule(IJob job, string jobName = "")
    {
        return Schedule(job, DateTime.UtcNow, jobName);
    }

    /// <summary>
    /// Schedules a job to run at a specific time
    /// </summary>
    /// <param name="job">The job to execute</param>
    /// <param name="scheduledTime">When to execute the job</param>
    /// <param name="jobName">Optional name for the job</param>
    /// <param name="maxRetries">Maximum retry attempts (uses default if not specified)</param>
    /// <returns>Job ID for tracking</returns>
    public string Schedule(IJob job, DateTime scheduledTime, string jobName = "", int? maxRetries = null)
    {
        if (job == null) throw new ArgumentNullException(nameof(job));
        if (_disposed) throw new ObjectDisposedException(nameof(LightweightScheduler));

        var scheduledJob = new ScheduledJob
        {
            Name = string.IsNullOrEmpty(jobName) ? job.GetType().Name : jobName,
            Job = job,
            ScheduledTime = scheduledTime,
            MaxRetries = maxRetries ?? _options.DefaultMaxRetries,
            Context = new JobExecutionContext
            {
                JobName = string.IsNullOrEmpty(jobName) ? job.GetType().Name : jobName,
                ScheduledTime = scheduledTime,
                MaxRetries = maxRetries ?? _options.DefaultMaxRetries
            }
        };

        scheduledJob.Context.JobId = scheduledJob.Id;
        _jobQueue.Enqueue(scheduledJob);

        if (_options.EnableLogging)
        {
            Console.WriteLine($"[Schedly] Scheduled job '{scheduledJob.Name}' (ID: {scheduledJob.Id}) for {scheduledTime:yyyy-MM-dd HH:mm:ss} UTC");
        }

        return scheduledJob.Id;
    }

    /// <summary>
    /// Schedules a job to run after a delay
    /// </summary>
    /// <param name="job">The job to execute</param>
    /// <param name="delay">Delay before execution</param>
    /// <param name="jobName">Optional name for the job</param>
    /// <param name="maxRetries">Maximum retry attempts (uses default if not specified)</param>
    /// <returns>Job ID for tracking</returns>
    public string ScheduleIn(IJob job, TimeSpan delay, string jobName = "", int? maxRetries = null)
    {
        return Schedule(job, DateTime.UtcNow.Add(delay), jobName, maxRetries);
    }

    /// <summary>
    /// Schedules a simple action as a job
    /// </summary>
    /// <param name="action">The action to execute</param>
    /// <param name="jobName">Optional name for the job</param>
    /// <returns>Job ID for tracking</returns>
    public string Schedule(Action action, string jobName = "")
    {
        return Schedule(new SimpleActionJob(action), jobName);
    }

    /// <summary>
    /// Schedules a simple async action as a job
    /// </summary>
    /// <param name="asyncAction">The async action to execute</param>
    /// <param name="jobName">Optional name for the job</param>
    /// <returns>Job ID for tracking</returns>
    public string Schedule(Func<Task> asyncAction, string jobName = "")
    {
        return Schedule(new SimpleAsyncActionJob(asyncAction), jobName);
    }

    private void ProcessScheduledJobs(object? state)
    {
        if (_disposed) return;

        var readyJobs = ExtractReadyJobs();
        
        foreach (var job in readyJobs)
        {
            if (_disposed) break;
            
            // Don't block the timer thread - execute jobs in background
            _ = Task.Run(async () => await ExecuteJobAsync(job));
        }
    }

    private ScheduledJob[] ExtractReadyJobs()
    {
        var readyJobs = new List<ScheduledJob>();
        var tempQueue = new List<ScheduledJob>();

        // Extract all jobs from the queue
        while (_jobQueue.TryDequeue(out var job))
        {
            if (job.IsReady && !job.HasExceededRetries)
            {
                readyJobs.Add(job);
            }
            else if (!job.HasExceededRetries)
            {
                tempQueue.Add(job); // Re-queue jobs that aren't ready yet
            }
            // Drop jobs that have exceeded retries
        }

        // Re-queue jobs that weren't ready
        foreach (var job in tempQueue)
        {
            _jobQueue.Enqueue(job);
        }

        return readyJobs.ToArray();
    }

    private async Task ExecuteJobAsync(ScheduledJob scheduledJob)
    {
        await _concurrencyLimit.WaitAsync();
        
        try
        {
            if (_options.EnableLogging)
            {
                var retryInfo = scheduledJob.Context.IsRetry ? $" (Retry {scheduledJob.RetryAttempt}/{scheduledJob.MaxRetries})" : "";
                Console.WriteLine($"[Schedly] Executing job '{scheduledJob.Name}' (ID: {scheduledJob.Id}){retryInfo}");
            }

            scheduledJob.Context.ExecutionStartTime = DateTime.UtcNow;
            scheduledJob.Context.RetryAttempt = scheduledJob.RetryAttempt;

            await scheduledJob.Job.ExecuteAsync(scheduledJob.Context);

            if (_options.EnableLogging)
            {
                Console.WriteLine($"[Schedly] Job '{scheduledJob.Name}' (ID: {scheduledJob.Id}) completed successfully");
            }
        }
        catch (Exception ex)
        {
            if (_options.EnableLogging)
            {
                Console.WriteLine($"[Schedly] Job '{scheduledJob.Name}' (ID: {scheduledJob.Id}) failed: {ex.Message}");
            }

            // Schedule retry if we haven't exceeded the limit
            if (scheduledJob.RetryAttempt < scheduledJob.MaxRetries)
            {
                var retryDelay = scheduledJob.Context.CalculateRetryDelay(_options.RetryBaseDelayMs);
                var retryTime = DateTime.UtcNow.Add(retryDelay);

                var retryJob = new ScheduledJob
                {
                    Id = scheduledJob.Id, // Keep the same ID for tracking
                    Name = scheduledJob.Name,
                    Job = scheduledJob.Job,
                    ScheduledTime = retryTime,
                    MaxRetries = scheduledJob.MaxRetries,
                    RetryAttempt = scheduledJob.RetryAttempt + 1,
                    Context = scheduledJob.Context
                };

                _jobQueue.Enqueue(retryJob);

                if (_options.EnableLogging)
                {
                    Console.WriteLine($"[Schedly] Scheduled retry for job '{scheduledJob.Name}' (ID: {scheduledJob.Id}) in {retryDelay.TotalSeconds:F1} seconds");
                }
            }
            else if (_options.EnableLogging)
            {
                Console.WriteLine($"[Schedly] Job '{scheduledJob.Name}' (ID: {scheduledJob.Id}) exceeded retry limit and will not be retried");
            }
        }
        finally
        {
            _concurrencyLimit.Release();
        }
    }

    /// <summary>
    /// Gets the current number of scheduled jobs
    /// </summary>
    public int ScheduledJobCount => _jobQueue.Count;

    /// <summary>
    /// Stops the scheduler and releases resources
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        
        _disposed = true;
        _schedulerTimer?.Dispose();
        _concurrencyLimit?.Dispose();
    }
}