namespace Schedly.Core.Tests;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;

using Moq;

using System.Threading;
using System.Threading.Tasks;

using Xunit;

public class HostedSchedulerTests
{
    private readonly Mock<SampleJob> _jobMock;
    private readonly ServiceProvider _serviceProvider;
    private readonly HostedScheduler _hostedScheduler;
    private readonly FakeTimeProvider _fakeTimeProvider;

    public HostedSchedulerTests()
    {
        _jobMock = new Mock<SampleJob>(NullLogger<SampleJob>.Instance, new FakeTimeProvider());

        var services = new ServiceCollection();
        _ = services.AddSingleton<Job>(_jobMock.Object);

        _serviceProvider = services.BuildServiceProvider();

        _fakeTimeProvider = new FakeTimeProvider();

        _hostedScheduler = new HostedScheduler(
            NullLogger<HostedScheduler>.Instance,
            _fakeTimeProvider,
            _serviceProvider);
    }

    [Fact]
    public async Task Can_Execute_Jobs_On_Startup()
    {
        // Arrange
        _ = _jobMock.Setup(job => job.ShouldExecute(true)).Returns(true);
        _ = _jobMock.Setup(job => job.Execute()).Returns(Task.CompletedTask);

        // Act
        await _hostedScheduler.StartAsync(CancellationToken.None);

        // Assert
        _jobMock.Verify(job => job.Execute(), Times.Once);
    }

    [Fact]
    public async Task Can_Execute_Jobs_On_Schedule()
    {
        // Arrange
        _ = _jobMock.Setup(job => job.ShouldExecute(true)).Returns(false);
        _ = _jobMock.Setup(job => job.ShouldExecute(false)).Returns(true);
        _ = _jobMock.Setup(job => job.Execute()).Returns(Task.CompletedTask);

        // Act
        await _hostedScheduler.StartAsync(CancellationToken.None);
        _fakeTimeProvider.Advance(TimeSpan.FromSeconds(70));

        // Assert
        _jobMock.Verify(job => job.Execute(), Times.Once);
    }

    [Fact]
    public async Task Should_Complete_Gracefully_When_No_Jobs_To_Execute()
    {
        // Arrange
        var emptyJobMock = new Mock<SampleJob>(NullLogger<SampleJob>.Instance, new FakeTimeProvider());
        var services = new ServiceCollection();
        _ = services.AddSingleton<Job>(emptyJobMock.Object);
        var emptyServiceProvider = services.BuildServiceProvider();

        var emptyHostedScheduler = new HostedScheduler(
            NullLogger<HostedScheduler>.Instance,
            _fakeTimeProvider,
            emptyServiceProvider);

        // Act
        await emptyHostedScheduler.StartAsync(CancellationToken.None);

        // Assert
        emptyJobMock.Verify(job => job.Execute(), Times.Never);
    }


}
