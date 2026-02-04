# schedly
A lightweight scheduler designed for fast onboarding — drop it in, register jobs, and let it handle retries, backoff, and execution without tedious configuration


## Job Execution Options
The `JobExecutionOptions` class allows you to configure the execution interval and whether the job should execute on application startup.

### Properties
- `ExecutionInterval`: The time span between job executions.
- `ShouldExecuteOnStartup`: A boolean indicating if the job should run when the application starts.

## Job Class
The abstract `Job` class provides the base functionality for creating scheduled jobs. It includes methods to determine if a job should execute and to log execution details.

### Methods
- `ShouldExecute(bool isAppStarting)`: Determines if the job should execute based on the application state and execution interval.
- `Execute()`: Executes the job and logs the execution time.

## Implementation 

- To create a new job, inherit from the `Job` class and implement the `OnExecute` method with the desired functionality.
- Provide a value for the `Name` property to identify the job.
- Override the `Options` property to configure the job execution options as per your need.
- Register the scheduler in tha app startup. You need to pass the assemblies where the jobs are defined. The scheduler will only schedule jobs from those assemblies.

    ```csharp
    builder.Services.ScheduleAllJobsFromSpecifiedAssemblies(new[] { typeof(SampleJob).Assembly });
    ```

## Sample Jobs
Two sample jobs, `SampleJob` and `SampleJob1`, demonstrate how to implement the `Job` class.

### SampleJob
- Executes every 1 minute.
- Does not run on application startup.

```csharp
public class SampleJob : Job
{
    public SampleJob(ILogger<Job> logger) : base(logger)
    {
    }

    protected override string Name => nameof(SampleJob);

    protected override JobExecutionOptions Options => new() { ExecutionInterval = TimeSpan.FromMinutes(1) };

    protected override async Task OnExecute()
    {
        // Do something
        return;
    }
}
```

### SampleJob1
- Executes every 3 minutes.
- Runs on application startup.

```csharp
public class SampleJob1 : Job
{
    public SampleJob1(ILogger<Job> logger) : base(logger)
    {
    }

    protected override string Name => nameof(SampleJob1);

    protected override JobExecutionOptions Options => new() 
    { 
        ExecutionInterval = TimeSpan.FromMinutes(3), 
        ShouldExecuteOnStartup = true 
    };

    protected override async Task OnExecute()
    {
        // Do something
        return;
    }
}
```

## References
- [Background tasks with hosted services in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-6.0&tabs=visual-studio)

