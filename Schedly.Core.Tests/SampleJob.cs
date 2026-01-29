namespace Schedly.Core.Tests;

using Microsoft.Extensions.Logging;

using System;
using System.Threading.Tasks;

public class SampleJob(ILogger<SampleJob> logger) : Job(logger)
{
    protected override JobExecutionOptions Options => new()
    {
        ExecutionInterval = TimeSpan.FromMinutes(1),
        ShouldExecuteOnStartup = true
    };

    protected override string Name => nameof(SampleJob);

    protected override Task OnExecute() => Task.CompletedTask;
}
