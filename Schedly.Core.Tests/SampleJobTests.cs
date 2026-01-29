using Microsoft.Extensions.Logging;

using Moq;

using Xunit;

namespace Schedly.Core.Tests;

public class SampleJobTests
{
    private readonly Mock<ILogger<SampleJob>> _loggerMock;
    private readonly SampleJob _sampleJob;

    public SampleJobTests()
    {
        _loggerMock = new Mock<ILogger<SampleJob>>();
        _sampleJob = new SampleJob(_loggerMock.Object);
    }

    [Fact]
    public void Options_ShouldReturnCorrectExecutionInterval()
    {
        // Arrange
        var expectedInterval = TimeSpan.FromMinutes(1);

        // Act
        var options = _sampleJob.Options;

        // Assert
        Assert.Equal(expectedInterval, options.ExecutionInterval);
        Assert.True(options.ShouldExecuteOnStartup);
    }

    [Fact]
    public void Name_ShouldReturnCorrectJobName()
    {
        // Act
        var name = _sampleJob.Name;

        // Assert
        Assert.Equal(nameof(SampleJob), name);
    }
}
