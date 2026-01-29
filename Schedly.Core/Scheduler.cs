using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Schedly.Core;

public sealed class HostedScheduler(
    ILogger<HostedScheduler> _logger,
    TimeProvider _timeProvider,
    IServiceProvider _serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting hosted scheduler");

        try
        {
            _logger.LogInformation("Executing jobs on app startup");

            await ExecuteJobs(true);

            _logger.LogInformation("Executed jobs on app startup");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while executing jobs on startup");
        }

        var periodicTimer = new PeriodicTimer(TimeSpan.FromMinutes(1), _timeProvider);

        while (!stoppingToken.IsCancellationRequested && await periodicTimer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                _logger.LogInformation("Executing jobs with timer");

                await ExecuteJobs();

                _logger.LogInformation("Executed jobs with timer");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while executing jobs on timer");
            }

        }

        _logger.LogInformation("Stopping hosted scheduler");
    }

    private async Task ExecuteJobs(bool isAppStartup = false)
    {
        var services = _serviceProvider.GetServices<Job>();

        if (!services.Any())
        {
            _logger.LogWarning("No jobs found to execute.");
            return;
        }

        var jobExecutionTasks = services
            .Where(job => job.ShouldExecute(isAppStartup))
            .Select(job => job.Execute());

        await Task.WhenAll(jobExecutionTasks);
    }
}