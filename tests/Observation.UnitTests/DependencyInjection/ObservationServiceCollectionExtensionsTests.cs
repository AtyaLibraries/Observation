using System.Reflection;
using Atya.Diagnostics.Logging.Context;
using Atya.Diagnostics.Metrics.Abstractions;
using Atya.Diagnostics.Metrics.Options;
using Atya.Diagnostics.Observation.DependencyInjection;
using Atya.Diagnostics.Observation.Models;
using Atya.Diagnostics.Observation.Options;
using Atya.Diagnostics.Tracing.Abstractions;
using Atya.Diagnostics.Tracing.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Observation.UnitTests.DependencyInjection;

public sealed class ObservationServiceCollectionExtensionsTests
{
    [Fact]
    public void AddAtyaObservation_Should_Return_Same_ServiceCollection()
    {
        var services = new ServiceCollection();

        var returned = services.AddAtyaObservation(options =>
        {
            options.ServiceName = "Samples.OrderProcessor";
        });

        _ = returned.Should().BeSameAs(services);
    }

    [Fact]
    public void AddAtyaObservation_Should_Throw_When_ServiceCollection_Is_Null()
    {
        IServiceCollection? services = null;

        var act = () => services!.AddAtyaObservation(options =>
        {
            options.ServiceName = "Samples.OrderProcessor";
        });

        _ = act.Should().Throw<ArgumentNullException>()
            .WithParameterName(nameof(services));
    }

    [Fact]
    public void AddAtyaObservation_With_ServiceName_Should_Register_ObservationIdentity()
    {
        var services = new ServiceCollection();

        var returned = services.AddAtyaObservation("Samples.OrderProcessor", "1.2.3");

        using var provider = services.BuildServiceProvider();

        var identity = provider.GetRequiredService<ObservationIdentity>();
        var tracingOptions = provider.GetRequiredService<IOptions<TracingOptions>>().Value;
        var metricsOptions = provider.GetRequiredService<IOptions<MetricsOptions>>().Value;

        _ = returned.Should().BeSameAs(services);
        _ = identity.ServiceName.Should().Be("Samples.OrderProcessor");
        _ = identity.ActivitySourceName.Should().Be("Samples.OrderProcessor");
        _ = identity.MeterName.Should().Be("Samples.OrderProcessor");
        _ = identity.ServiceVersion.Should().Be("1.2.3");
        _ = tracingOptions.ActivitySourceName.Should().Be("Samples.OrderProcessor");
        _ = tracingOptions.ActivitySourceVersion.Should().Be("1.2.3");
        _ = metricsOptions.MeterName.Should().Be("Samples.OrderProcessor");
        _ = metricsOptions.MeterVersion.Should().Be("1.2.3");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddAtyaObservation_With_ServiceName_Should_Throw_When_ServiceName_Is_Invalid(string? serviceName)
    {
        var services = new ServiceCollection();

        var act = () => services.AddAtyaObservation(serviceName!);

        _ = act.Should().Throw<ArgumentException>()
            .WithParameterName(nameof(serviceName));
    }

    [Fact]
    public void AddAtyaObservation_Should_Register_ObservationIdentity_And_Map_Names()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaObservation(options =>
        {
            options.ServiceName = "Samples.OrderProcessor";
            options.ServiceVersion = "1.2.3";
        });

        using var provider = services.BuildServiceProvider();

        var identity = provider.GetRequiredService<ObservationIdentity>();
        var tracingOptions = provider.GetRequiredService<IOptions<TracingOptions>>().Value;
        var metricsOptions = provider.GetRequiredService<IOptions<MetricsOptions>>().Value;
        var observationOptions = provider.GetRequiredService<IOptions<ObservationOptions>>().Value;

        _ = identity.ServiceName.Should().Be("Samples.OrderProcessor");
        _ = identity.ActivitySourceName.Should().Be("Samples.OrderProcessor");
        _ = identity.MeterName.Should().Be("Samples.OrderProcessor");
        _ = identity.ServiceVersion.Should().Be("1.2.3");

        _ = tracingOptions.ActivitySourceName.Should().Be("Samples.OrderProcessor");
        _ = tracingOptions.ActivitySourceVersion.Should().Be("1.2.3");

        _ = metricsOptions.MeterName.Should().Be("Samples.OrderProcessor");
        _ = metricsOptions.MeterVersion.Should().Be("1.2.3");

        _ = observationOptions.ConfigureLogging.Should().BeTrue();
        _ = observationOptions.ConfigureTracing.Should().BeTrue();
        _ = observationOptions.ConfigureMetrics.Should().BeTrue();
    }

    [Fact]
    public void AddAtyaObservation_Should_Trim_Identity_Values()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaObservation(options =>
        {
            options.ServiceName = "  Samples.OrderProcessor  ";
            options.ServiceVersion = "  2.0.0  ";
            options.ActivitySourceName = "  Samples.OrderProcessor.Tracing  ";
            options.MeterName = "  Samples.OrderProcessor.Metrics  ";
        });

        using var provider = services.BuildServiceProvider();

        var identity = provider.GetRequiredService<ObservationIdentity>();
        var tracingOptions = provider.GetRequiredService<IOptions<TracingOptions>>().Value;
        var metricsOptions = provider.GetRequiredService<IOptions<MetricsOptions>>().Value;

        _ = identity.ServiceName.Should().Be("Samples.OrderProcessor");
        _ = identity.ServiceVersion.Should().Be("2.0.0");
        _ = identity.ActivitySourceName.Should().Be("Samples.OrderProcessor.Tracing");
        _ = identity.MeterName.Should().Be("Samples.OrderProcessor.Metrics");
        _ = tracingOptions.ActivitySourceName.Should().Be("Samples.OrderProcessor.Tracing");
        _ = tracingOptions.ActivitySourceVersion.Should().Be("2.0.0");
        _ = metricsOptions.MeterName.Should().Be("Samples.OrderProcessor.Metrics");
        _ = metricsOptions.MeterVersion.Should().Be("2.0.0");
    }

    [Fact]
    public void AddAtyaObservation_Should_Use_Default_Names_And_Null_Version_When_Optional_Values_Are_Whitespace()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaObservation(options =>
        {
            options.ServiceName = "Samples.OrderProcessor";
            options.ServiceVersion = " ";
            options.ActivitySourceName = " ";
            options.MeterName = " ";
        });

        using var provider = services.BuildServiceProvider();

        var identity = provider.GetRequiredService<ObservationIdentity>();
        var tracingOptions = provider.GetRequiredService<IOptions<TracingOptions>>().Value;
        var metricsOptions = provider.GetRequiredService<IOptions<MetricsOptions>>().Value;

        _ = identity.ActivitySourceName.Should().Be("Samples.OrderProcessor");
        _ = identity.MeterName.Should().Be("Samples.OrderProcessor");
        _ = identity.ServiceVersion.Should().BeNull();
        _ = tracingOptions.ActivitySourceName.Should().Be("Samples.OrderProcessor");
        _ = tracingOptions.ActivitySourceVersion.Should().BeNull();
        _ = metricsOptions.MeterName.Should().Be("Samples.OrderProcessor");
        _ = metricsOptions.MeterVersion.Should().BeNull();
    }

    [Fact]
    public void AddAtyaObservation_Should_Register_Tracing_And_Metrics_Services_By_Default()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaObservation(options =>
        {
            options.ServiceName = "Samples.OrderProcessor";
        });

        using var provider = services.BuildServiceProvider();

        _ = provider.GetService<ILogScopeStateFactory>().Should().NotBeNull();
        _ = provider.GetService<IActivitySourceAccessor>().Should().NotBeNull();
        _ = provider.GetService<IMeterAccessor>().Should().NotBeNull();
    }

    [Fact]
    public void AddAtyaObservation_Should_Allow_Disabling_Tracing_And_Metrics()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaObservation(options =>
        {
            options.ServiceName = "Samples.OrderProcessor";
            options.ConfigureTracing = false;
            options.ConfigureMetrics = false;
        });

        using var provider = services.BuildServiceProvider();

        _ = provider.GetService<IActivitySourceAccessor>().Should().BeNull();
        _ = provider.GetService<IMeterAccessor>().Should().BeNull();
    }

    [Fact]
    public void AddAtyaObservation_Should_Allow_Disabling_Logging()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaObservation(options =>
        {
            options.ServiceName = "Samples.OrderProcessor";
            options.ConfigureLogging = false;
        });

        using var provider = services.BuildServiceProvider();

        _ = provider.GetService<ILogScopeStateFactory>().Should().BeNull();
        _ = provider.GetService<IActivitySourceAccessor>().Should().NotBeNull();
        _ = provider.GetService<IMeterAccessor>().Should().NotBeNull();
    }

    [Fact]
    public void AddAtyaObservation_Should_Be_Idempotent_For_Core_Identity_Service()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaObservation(options =>
        {
            options.ServiceName = "Samples.OrderProcessor";
        });

        _ = services.AddAtyaObservation(options =>
        {
            options.ServiceName = "Samples.OrderProcessor";
        });

        _ = services.Count(descriptor => descriptor.ServiceType == typeof(ObservationIdentity)).Should().Be(1);
    }

    [Fact]
    public void AddAtyaObservation_Should_Throw_When_ServiceName_Is_Missing()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaObservation();

        using var provider = services.BuildServiceProvider();

        var act = () => provider.GetRequiredService<IOptions<ObservationOptions>>().Value;

        _ = act.Should().Throw<OptionsValidationException>()
            .WithMessage("*ServiceName*");
    }

    [Fact]
    public void ObservationOptionsValidator_Should_Return_Success_For_Valid_ServiceName()
    {
        var validator = CreateObservationOptionsValidator();

        var result = validator.Validate(null, new ObservationOptions
        {
            ServiceName = "Samples.OrderProcessor"
        });

        _ = result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void ObservationOptionsValidator_Should_Return_Failure_For_Whitespace_ServiceName()
    {
        var validator = CreateObservationOptionsValidator();

        var result = validator.Validate(null, new ObservationOptions
        {
            ServiceName = " "
        });

        _ = result.Failed.Should().BeTrue();
        _ = result.Failures.Should().ContainSingle()
            .Which.Should().Contain("ServiceName");
    }

    [Fact]
    public void ObservationOptionsValidator_Should_Throw_When_Options_Are_Null()
    {
        var validator = CreateObservationOptionsValidator();

        var act = () => validator.Validate(null, null!);

        _ = act.Should().Throw<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Fact]
    public void ObservationIdentityResolver_Should_Throw_When_Options_Are_Null()
    {
        var resolve = GetObservationIdentityResolver();

        var act = () => resolve.Invoke(null, [null]);

        _ = act.Should().Throw<TargetInvocationException>()
            .WithInnerException<ArgumentNullException>()
            .WithParameterName("options");
    }

    [Fact]
    public void ObservationIdentityResolver_Should_Throw_When_ServiceName_Is_Whitespace()
    {
        var resolve = GetObservationIdentityResolver();

        var act = () => resolve.Invoke(null, [new ObservationOptions()]);

        _ = act.Should().Throw<TargetInvocationException>()
            .WithInnerException<ArgumentException>()
            .WithParameterName(nameof(ObservationOptions.ServiceName));
    }

    private static IValidateOptions<ObservationOptions> CreateObservationOptionsValidator()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaObservation(options =>
        {
            options.ServiceName = "Samples.OrderProcessor";
        });

        using var provider = services.BuildServiceProvider();

        return provider.GetServices<IValidateOptions<ObservationOptions>>()
            .Single(static validator => validator.GetType().Name.Contains("ObservationOptionsValidator", StringComparison.Ordinal));
    }

    private static MethodInfo GetObservationIdentityResolver()
    {
        return typeof(ObservationOptions)
            .Assembly
            .GetType("Atya.Diagnostics.Observation.Internal.ObservationIdentityResolver", throwOnError: true)!
            .GetMethod("Resolve", BindingFlags.Public | BindingFlags.Static)!;
    }
}
