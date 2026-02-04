using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

using Moq;

using Xunit;

namespace Schedly.Core.Tests;

public class SampleJobTests
{
    private readonly Mock<ILogger<SampleJob>> _loggerMock;
    private readonly SampleJob _sampleJob;
    private readonly FakeTimeProvider _fakeTimeProvider;

    public SampleJobTests()
    {
        _fakeTimeProvider = new FakeTimeProvider();
        _loggerMock = new Mock<ILogger<SampleJob>>();
        _sampleJob = new SampleJob(_loggerMock.Object, _fakeTimeProvider);
    }

    [Fact]
    public void Can_Execute_Job_Immediately_If_Set_To_Execute_On_Startup()
    {
        // Arrange
        // Act
        var shouldExecuteJob = _sampleJob.ShouldExecute(true);
        
        // Assert
        Assert.True(shouldExecuteJob);
    }

    [Fact]
    public void Can_Not_Execute_Job_Immediately_If_Set_To_Not_Execute_On_Startup()
    {
        // Arrange
        // Act
        var shouldExecuteJob = _sampleJob.ShouldExecute(false);

        // Assert
        Assert.False(shouldExecuteJob);
    }

    [Fact]
    public void Can_Execute_Job_After_Time_Elapsed_For_The_First_Time()
    {
        // Arrange
        _fakeTimeProvider.Advance(TimeSpan.FromMinutes(2));

        // Act
        var shouldExecuteJob = _sampleJob.ShouldExecute(false);

        // Assert
        Assert.True(shouldExecuteJob);
    }

    [Fact]
    public async Task Can_Not_Execute_Job_Immediately_After_Time_Elapsed_For_The_First_Time()
    {
        // Arrange
        _fakeTimeProvider.Advance(TimeSpan.FromSeconds(70));
        await _sampleJob.Execute();

        // Act
        var shouldExecuteJob = _sampleJob.ShouldExecute(false);

        // Assert
        Assert.False(shouldExecuteJob);
    }

    [Fact]
    public async Task Can_Execute_Job_After_Time_Elapsed_After_Subsequent_Calls()
    {
        // Arrange
        _fakeTimeProvider.Advance(TimeSpan.FromSeconds(70));
        await _sampleJob.Execute();
        _fakeTimeProvider.Advance(TimeSpan.FromSeconds(70));

        // Act
        var shouldExecuteJob = _sampleJob.ShouldExecute(false);

        // Assert
        Assert.True(shouldExecuteJob);
    }
}
