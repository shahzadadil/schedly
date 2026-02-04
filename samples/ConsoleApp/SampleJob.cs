namespace ConsoleApp;

using Microsoft.Extensions.Logging;

using Schedly.Core;

using System;
using System.Threading.Tasks;

internal class SampleJob(ILogger<SampleJob> _logger, TimeProvider _timeProvider) : Job(_logger, _timeProvider)
{
    protected override JobExecutionOptions Options => new()
    {
        ExecutionInterval = TimeSpan.FromMinutes(1),
        ShouldExecuteOnStartup = true
    };

    protected override string Name => nameof(SampleJob);

    protected override Task OnExecute() => Task.CompletedTask;
}
