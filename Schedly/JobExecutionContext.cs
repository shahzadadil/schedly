using System;
using System.Collections.Generic;

namespace Schedly;

/// <summary>
/// Provides context information during job execution
/// </summary>
public class JobExecutionContext
{
    /// <summary>
    /// Unique identifier for this job execution
    /// </summary>
    public string JobId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Name of the job for identification
    /// </summary>
    public string JobName { get; set; } = string.Empty;

    /// <summary>
    /// Current retry attempt (0 for first execution)
    /// </summary>
    public int RetryAttempt { get; set; } = 0;

    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// When the job was scheduled to run
    /// </summary>
    public DateTime ScheduledTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the job execution started
    /// </summary>
    public DateTime ExecutionStartTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Additional data that can be passed to the job
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();

    /// <summary>
    /// Whether this is a retry execution
    /// </summary>
    public bool IsRetry => RetryAttempt > 0;

    /// <summary>
    /// Calculate the delay for the next retry using exponential backoff
    /// </summary>
    /// <param name="baseDelayMs">Base delay in milliseconds</param>
    /// <returns>Delay for the next retry</returns>
    public TimeSpan CalculateRetryDelay(int baseDelayMs = 1000)
    {
        // Exponential backoff: baseDelay * 2^retryAttempt
        var delayMs = baseDelayMs * Math.Pow(2, RetryAttempt);
        return TimeSpan.FromMilliseconds(Math.Min(delayMs, 30000)); // Cap at 30 seconds
    }
}