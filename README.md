# Schedly

A lightweight scheduler designed for fast onboarding — drop it in, register jobs, and let it handle retries, backoff, and execution without tedious configuration.

## Features

✨ **Lightweight & Fast** - Minimal dependencies, quick setup  
🚀 **Easy to Use** - Simple API for immediate productivity  
🔄 **Automatic Retries** - Built-in exponential backoff  
⚙️ **Configurable** - Sensible defaults, customizable when needed  
🎯 **Thread-Safe** - Concurrent job execution with limits  
📊 **Context-Aware** - Rich execution context for jobs  

## Quick Start

### Installation

```bash
# Add to your project (when published to NuGet)
dotnet add package Schedly
```

### Simple Usage

```csharp
using Schedly;

// Schedule an immediate task
Scheduler.Run(() => Console.WriteLine("Hello World!"));

// Schedule a delayed task
Scheduler.RunIn(() => SendEmail(), TimeSpan.FromMinutes(5));

// Schedule at specific time
Scheduler.RunAt(() => GenerateReport(), DateTime.Today.AddHours(9));
```

### Custom Jobs

```csharp
public class EmailJob : IJob
{
    public async Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default)
    {
        // Your job logic here
        await SendEmailAsync();
        
        // Access execution context
        Console.WriteLine($"Job {context.JobId} executed at {context.ExecutionStartTime}");
    }
}

// Schedule the custom job
Scheduler.Run(new EmailJob(), "send-welcome-email");
```

### Advanced Configuration

```csharp
var options = new SchedulerOptions
{
    DefaultMaxRetries = 5,
    RetryBaseDelayMs = 2000,
    MaxConcurrentJobs = 10,
    EnableLogging = true
};

using var scheduler = new LightweightScheduler(options);

scheduler.Schedule(new EmailJob(), DateTime.UtcNow.AddHours(1), "scheduled-email", maxRetries: 3);
```

## API Reference

### Static Scheduler (Quick Use)

```csharp
// Immediate execution
Scheduler.Run(action, jobName?)
Scheduler.Run(asyncAction, jobName?)
Scheduler.Run(job, jobName?)

// Delayed execution
Scheduler.RunIn(action, delay, jobName?)
Scheduler.RunAt(action, scheduledTime, jobName?)

// Custom scheduler instance
Scheduler.Default // Access the default scheduler
```

### LightweightScheduler

```csharp
// Create scheduler
var scheduler = new LightweightScheduler(options?);

// Schedule jobs
string Schedule(IJob job, string jobName = "")
string Schedule(IJob job, DateTime scheduledTime, string jobName = "", int? maxRetries = null)
string ScheduleIn(IJob job, TimeSpan delay, string jobName = "", int? maxRetries = null)

// Helper methods for actions
string Schedule(Action action, string jobName = "")
string Schedule(Func<Task> asyncAction, string jobName = "")
```

### Job Implementation

```csharp
public interface IJob
{
    Task ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken = default);
}
```

### Configuration Options

```csharp
public class SchedulerOptions
{
    public int DefaultMaxRetries { get; set; } = 3;
    public int RetryBaseDelayMs { get; set; } = 1000;
    public int MaxConcurrentJobs { get; set; } = Environment.ProcessorCount;
    public bool EnableLogging { get; set; } = false;
    public int SchedulerIntervalMs { get; set; } = 1000;
}
```

## How It Works

1. **Drop it in** - Add the package and you're ready to go
2. **Register jobs** - Use simple static methods or create custom IJob implementations  
3. **Automatic handling** - Built-in retry logic with exponential backoff
4. **No configuration** - Sensible defaults work out of the box

### Retry Logic

- Jobs automatically retry on failure
- Exponential backoff: 1s, 2s, 4s, 8s... (up to 30s max)
- Configurable retry limits
- Failed jobs are logged and dropped after max retries

### Concurrency

- Configurable concurrent job limit (defaults to CPU count)
- Thread-safe job queue
- Non-blocking scheduler loop

## Examples

See the [Schedly.Examples](./Schedly.Examples/) project for comprehensive usage examples.

## License

Licensed under the Apache License, Version 2.0. See [LICENSE](./LICENSE) for details.
