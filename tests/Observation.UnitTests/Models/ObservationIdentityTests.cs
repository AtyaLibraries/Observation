using Atya.Diagnostics.Observation.Models;

namespace Observation.UnitTests.Models;

public sealed class ObservationIdentityTests
{
    [Fact]
    public void Constructor_Should_Trim_Values()
    {
        var identity = new ObservationIdentity(
            "  Samples.OrderProcessor  ",
            "  1.0.0  ",
            "  Samples.OrderProcessor.Tracing  ",
            "  Samples.OrderProcessor.Metrics  ");

        _ = identity.ServiceName.Should().Be("Samples.OrderProcessor");
        _ = identity.ServiceVersion.Should().Be("1.0.0");
        _ = identity.ActivitySourceName.Should().Be("Samples.OrderProcessor.Tracing");
        _ = identity.MeterName.Should().Be("Samples.OrderProcessor.Metrics");
    }

    [Fact]
    public void Constructor_Should_Normalize_Whitespace_ServiceVersion_To_Null()
    {
        var identity = new ObservationIdentity(
            "Samples.OrderProcessor",
            " ",
            "Samples.OrderProcessor",
            "Samples.OrderProcessor");

        _ = identity.ServiceVersion.Should().BeNull();
    }

    [Theory]
    [InlineData(null, "Samples.OrderProcessor", "Samples.OrderProcessor", "serviceName")]
    [InlineData(" ", "Samples.OrderProcessor", "Samples.OrderProcessor", "serviceName")]
    [InlineData("Samples.OrderProcessor", null, "Samples.OrderProcessor", "activitySourceName")]
    [InlineData("Samples.OrderProcessor", " ", "Samples.OrderProcessor", "activitySourceName")]
    [InlineData("Samples.OrderProcessor", "Samples.OrderProcessor", null, "meterName")]
    [InlineData("Samples.OrderProcessor", "Samples.OrderProcessor", " ", "meterName")]
    public void Constructor_Should_Throw_When_Required_Name_Is_Invalid(
        string? serviceName,
        string? activitySourceName,
        string? meterName,
        string expectedParameterName)
    {
        var act = () => new ObservationIdentity(serviceName!, null, activitySourceName!, meterName!);

        _ = act.Should().Throw<ArgumentException>()
            .WithParameterName(expectedParameterName);
    }
}
