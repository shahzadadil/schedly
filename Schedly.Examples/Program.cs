using Schedly;

Console.WriteLine("🚀 Schedly - Lightweight Scheduler Demo");
Console.WriteLine("=========================================");

// Example 1: Simple action scheduling using static API
Console.WriteLine("\n1. Scheduling simple actions...");
Scheduler.Run(() => Console.WriteLine("✅ Immediate task executed!"), "immediate-task");

Scheduler.RunIn(() => Console.WriteLine("⏰ Delayed task executed!"), TimeSpan.FromSeconds(2), "delayed-task");

Scheduler.RunAt(() => Console.WriteLine("📅 Scheduled task executed!"), DateTime.UtcNow.AddSeconds(1), "scheduled-task");

// Example 2: Custom job implementation
Console.WriteLine("\n2. Scheduling custom jobs...");

// Create a custom job that might fail and retry
var flakyJob = new FlakyJob();
Scheduler.Run(flakyJob, "flaky-job");

// Example 3: Using the scheduler directly for more control
Console.WriteLine("\n3. Using scheduler with custom options...");

var options = new SchedulerOptions
{
    EnableLogging = true,
    DefaultMaxRetries = 2,
    RetryBaseDelayMs = 500
};

using var customScheduler = new LightweightScheduler(options);

customScheduler.Schedule(async () => 
{
    await Task.Delay(100); // Simulate some work
    Console.WriteLine("🔧 Custom scheduler task completed!");
}, "custom-async-task");

// Example 4: Scheduling multiple jobs with data
Console.WriteLine("\n4. Scheduling jobs with context data...");

var dataJob = new DataProcessingJob();
customScheduler.Schedule(dataJob, "data-processing");

Console.WriteLine("\n⏳ Waiting for jobs to complete... (Press any key to exit)");
Console.ReadKey();

Console.WriteLine("\n✨ Demo completed!");

/// <summary>
/// Example job that demonstrates retry behavior
/// </summary>
public class FlakyJob : IJob
{
    private static int _attempts = 0;

    public Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default)
    {
        _attempts++;
        
        Console.WriteLine($"🎯 FlakyJob attempt #{_attempts} (Retry: {context.RetryAttempt})");
        
        // Fail the first two attempts, succeed on the third
        if (_attempts < 3)
        {
            throw new Exception($"Simulated failure #{_attempts}");
        }
        
        Console.WriteLine($"🎉 FlakyJob succeeded after {_attempts} attempts!");
        return Task.CompletedTask;
    }
}

/// <summary>
/// Example job that uses context data
/// </summary>
public class DataProcessingJob : IJob
{
    public async Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"📊 Processing data for job: {context.JobName} (ID: {context.JobId})");
        Console.WriteLine($"   Scheduled: {context.ScheduledTime:HH:mm:ss}");
        Console.WriteLine($"   Started: {context.ExecutionStartTime:HH:mm:ss}");
        
        // Simulate some work
        await Task.Delay(500, cancellationToken);
        
        Console.WriteLine($"✅ Data processing completed!");
    }
}
