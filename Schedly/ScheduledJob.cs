using System;

namespace Schedly;

/// <summary>
/// Represents a scheduled job with its execution details
/// </summary>
public class ScheduledJob
{
    /// <summary>
    /// Unique identifier for the job
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Human-readable name for the job
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The job implementation to execute
    /// </summary>
    public IJob Job { get; set; } = null!;

    /// <summary>
    /// When the job should be executed
    /// </summary>
    public DateTime ScheduledTime { get; set; }

    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Current retry attempt
    /// </summary>
    public int RetryAttempt { get; set; } = 0;

    /// <summary>
    /// Job execution context
    /// </summary>
    public JobExecutionContext Context { get; set; } = new();

    /// <summary>
    /// Whether this job is ready to be executed
    /// </summary>
    public bool IsReady => DateTime.UtcNow >= ScheduledTime;

    /// <summary>
    /// Whether this job has exceeded its retry limit
    /// </summary>
    public bool HasExceededRetries => RetryAttempt >= MaxRetries;
}