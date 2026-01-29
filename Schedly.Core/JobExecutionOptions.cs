public class JobExecutionOptions
{
    public TimeSpan ExecutionInterval { get; set; }
    public bool ShouldExecuteOnStartup { get; set; }

    public JobExecutionOptions()
    {
        ExecutionInterval = TimeSpan.FromDays(1);
        ShouldExecuteOnStartup = false;
    }
}
