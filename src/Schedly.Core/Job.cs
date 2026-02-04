using Microsoft.Extensions.Logging;

namespace Schedly.Core;

public abstract class Job(
    ILogger<Job> _logger,
    TimeProvider _timeProvider)
{
    private DateTimeOffset? _lastExecutedAt = null;
    private readonly DateTimeOffset _createdAt = _timeProvider.GetUtcNow();

    private TimeSpan TimeElapsedSinceCreation => _timeProvider.GetUtcNow() - _createdAt;
    private TimeSpan TimeElapsedSinceLastExecution => _lastExecutedAt.HasValue ? _timeProvider.GetUtcNow() - _lastExecutedAt.Value : TimeSpan.MaxValue;

    protected abstract JobExecutionOptions Options { get; }

    protected abstract string Name { get; }

    public virtual bool ShouldExecute(bool isAppStarting)
    {
        if (!_lastExecutedAt.HasValue)
        {
            var hasExecutionIntervalElapsed = TimeElapsedSinceCreation >= Options.ExecutionInterval;
            return (isAppStarting && Options.ShouldExecuteOnStartup) || hasExecutionIntervalElapsed;
        }

        return TimeElapsedSinceLastExecution >= Options.ExecutionInterval;
    }

    public virtual async Task Execute()
    {
        _lastExecutedAt = _timeProvider.GetUtcNow();

        _logger.LogInformation("Executing {Name} at {ExecutionTime}", Name, _lastExecutedAt);

        await OnExecute();

        _logger.LogInformation("Executed {Name} at {ExecutionTime}", Name, _lastExecutedAt);
    }

    protected virtual Task OnExecute() => throw new NotImplementedException();
}