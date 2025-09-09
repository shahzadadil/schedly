using System;

namespace Schedly;

/// <summary>
/// Configuration options for the scheduler
/// </summary>
public class SchedulerOptions
{
    /// <summary>
    /// Default maximum number of retries for failed jobs
    /// </summary>
    public int DefaultMaxRetries { get; set; } = 3;

    /// <summary>
    /// Base delay in milliseconds for retry exponential backoff
    /// </summary>
    public int RetryBaseDelayMs { get; set; } = 1000;

    /// <summary>
    /// Maximum number of concurrent job executions
    /// </summary>
    public int MaxConcurrentJobs { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// Whether to log job execution details (for debugging)
    /// </summary>
    public bool EnableLogging { get; set; } = false;

    /// <summary>
    /// How often to check for scheduled jobs (in milliseconds)
    /// </summary>
    public int SchedulerIntervalMs { get; set; } = 1000;
}