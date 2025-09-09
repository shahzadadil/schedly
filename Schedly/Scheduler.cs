using System;
using System.Threading;
using System.Threading.Tasks;

namespace Schedly;

/// <summary>
/// Static API for quick and easy job scheduling
/// </summary>
public static class Scheduler
{
    private static readonly Lazy<LightweightScheduler> _defaultScheduler = 
        new(() => new LightweightScheduler(new SchedulerOptions { EnableLogging = true }));

    /// <summary>
    /// Gets the default scheduler instance
    /// </summary>
    public static LightweightScheduler Default => _defaultScheduler.Value;

    /// <summary>
    /// Schedules a job to run immediately using the default scheduler
    /// </summary>
    /// <param name="job">The job to execute</param>
    /// <param name="jobName">Optional name for the job</param>
    /// <returns>Job ID for tracking</returns>
    public static string Run(IJob job, string jobName = "")
    {
        return Default.Schedule(job, jobName);
    }

    /// <summary>
    /// Schedules an action to run immediately using the default scheduler
    /// </summary>
    /// <param name="action">The action to execute</param>
    /// <param name="jobName">Optional name for the job</param>
    /// <returns>Job ID for tracking</returns>
    public static string Run(Action action, string jobName = "")
    {
        return Default.Schedule(action, jobName);
    }

    /// <summary>
    /// Schedules an async action to run immediately using the default scheduler
    /// </summary>
    /// <param name="asyncAction">The async action to execute</param>
    /// <param name="jobName">Optional name for the job</param>
    /// <returns>Job ID for tracking</returns>
    public static string Run(Func<Task> asyncAction, string jobName = "")
    {
        return Default.Schedule(asyncAction, jobName);
    }

    /// <summary>
    /// Schedules a job to run after a delay using the default scheduler
    /// </summary>
    /// <param name="job">The job to execute</param>
    /// <param name="delay">Delay before execution</param>
    /// <param name="jobName">Optional name for the job</param>
    /// <returns>Job ID for tracking</returns>
    public static string RunIn(IJob job, TimeSpan delay, string jobName = "")
    {
        return Default.ScheduleIn(job, delay, jobName);
    }

    /// <summary>
    /// Schedules an action to run after a delay using the default scheduler
    /// </summary>
    /// <param name="action">The action to execute</param>
    /// <param name="delay">Delay before execution</param>
    /// <param name="jobName">Optional name for the job</param>
    /// <returns>Job ID for tracking</returns>
    public static string RunIn(Action action, TimeSpan delay, string jobName = "")
    {
        return Default.ScheduleIn(new SimpleActionJob(action), delay, jobName);
    }

    /// <summary>
    /// Schedules an async action to run after a delay using the default scheduler
    /// </summary>
    /// <param name="asyncAction">The async action to execute</param>
    /// <param name="delay">Delay before execution</param>
    /// <param name="jobName">Optional name for the job</param>
    /// <returns>Job ID for tracking</returns>
    public static string RunIn(Func<Task> asyncAction, TimeSpan delay, string jobName = "")
    {
        return Default.ScheduleIn(new SimpleAsyncActionJob(asyncAction), delay, jobName);
    }

    /// <summary>
    /// Schedules a job to run at a specific time using the default scheduler
    /// </summary>
    /// <param name="job">The job to execute</param>
    /// <param name="scheduledTime">When to execute the job</param>
    /// <param name="jobName">Optional name for the job</param>
    /// <returns>Job ID for tracking</returns>
    public static string RunAt(IJob job, DateTime scheduledTime, string jobName = "")
    {
        return Default.Schedule(job, scheduledTime, jobName);
    }

    /// <summary>
    /// Schedules an action to run at a specific time using the default scheduler
    /// </summary>
    /// <param name="action">The action to execute</param>
    /// <param name="scheduledTime">When to execute the job</param>
    /// <param name="jobName">Optional name for the job</param>
    /// <returns>Job ID for tracking</returns>
    public static string RunAt(Action action, DateTime scheduledTime, string jobName = "")
    {
        return Default.Schedule(new SimpleActionJob(action), scheduledTime, jobName);
    }

    /// <summary>
    /// Schedules an async action to run at a specific time using the default scheduler
    /// </summary>
    /// <param name="asyncAction">The async action to execute</param>
    /// <param name="scheduledTime">When to execute the job</param>
    /// <param name="jobName">Optional name for the job</param>
    /// <returns>Job ID for tracking</returns>
    public static string RunAt(Func<Task> asyncAction, DateTime scheduledTime, string jobName = "")
    {
        return Default.Schedule(new SimpleAsyncActionJob(asyncAction), scheduledTime, jobName);
    }
}

/// <summary>
/// Helper job implementation for simple actions (exposed for static API)
/// </summary>
public class SimpleActionJob : IJob
{
    private readonly Action _action;

    /// <summary>
    /// Creates a new simple action job
    /// </summary>
    /// <param name="action">The action to execute</param>
    public SimpleActionJob(Action action)
    {
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    /// <summary>
    /// Executes the action
    /// </summary>
    public Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default)
    {
        _action();
        return Task.CompletedTask;
    }
}

/// <summary>
/// Helper job implementation for simple async actions (exposed for static API)
/// </summary>
public class SimpleAsyncActionJob : IJob
{
    private readonly Func<Task> _asyncAction;

    /// <summary>
    /// Creates a new simple async action job
    /// </summary>
    /// <param name="asyncAction">The async action to execute</param>
    public SimpleAsyncActionJob(Func<Task> asyncAction)
    {
        _asyncAction = asyncAction ?? throw new ArgumentNullException(nameof(asyncAction));
    }

    /// <summary>
    /// Executes the async action
    /// </summary>
    public Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default)
    {
        return _asyncAction();
    }
}