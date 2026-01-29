using Microsoft.Extensions.DependencyInjection;

using Schedly.Core;

using System.Reflection;

public static class RegistrationExtensions
{
    /// <summary>
    /// Registers the scheduler and all jobs in the specified assemblies. Scans the provided assemblies for types deriving from <see cref="Job"/> and registers them as singletons.
    /// </summary>
    /// <param name="services">The service collection to register the scheduler and jobs in.</param>
    /// <param name="assembliesToScan">Assemblies to search for defined jobs</param>
    public static void ScheduleAllJobsFromSpecifiedAssemblies(
        this IServiceCollection services,
        IEnumerable<Assembly> assembliesToScan)
    {
        if (!assembliesToScan.Any())
        {
            return;
        }

        _ = services.AddHostedService<HostedScheduler>();

        var jobs = assembliesToScan
            .DistinctBy(a => a.GetName())
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsSubclassOf(typeof(Job)));

        foreach (var job in jobs)
        {
            services.AddSingleton(typeof(Job), job);
        }
    }
}
