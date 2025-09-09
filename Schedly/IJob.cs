using System;
using System.Threading;
using System.Threading.Tasks;

namespace Schedly;

/// <summary>
/// Represents a job that can be executed by the scheduler
/// </summary>
public interface IJob
{
    /// <summary>
    /// Executes the job with the given context
    /// </summary>
    /// <param name="context">The execution context containing job information and retry state</param>
    /// <param name="cancellationToken">Token to cancel the job execution</param>
    /// <returns>A task representing the job execution</returns>
    Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default);
}