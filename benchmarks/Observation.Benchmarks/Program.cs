using System.Diagnostics.CodeAnalysis;
using Atya.Diagnostics.Metrics.Options;
using Atya.Diagnostics.Observation.Models;
using Atya.Diagnostics.Tracing.Options;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Observation.Benchmarks;

/// <summary>
/// Runs the Atya.Diagnostics.Observation benchmark suite.
/// </summary>
public static class Program
{
    /// <summary>
    /// Executes the benchmark suite.
    /// </summary>
    /// <param name="args">Command-line arguments passed to BenchmarkDotNet.</param>
    public static void Main(string[] args)
    {
        _ = BenchmarkRunner.Run<ObservationRegistrationBenchmarks>(args: args);
    }
}

/// <summary>
/// Benchmarks observation service registration paths.
/// </summary>
[MemoryDiagnoser]
[Config(typeof(BenchmarkConfig))]
[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "BenchmarkDotNet requires benchmark methods to be instance methods.")]
public class ObservationRegistrationBenchmarks
{
    private const string ServiceName = "Samples.OrderProcessor";
    private const string ServiceVersion = "1.0.0";

    private ServiceProvider _provider = null!;

    /// <summary>
    /// Builds the warm service provider used by benchmarks.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        this._provider = CreateObservationServices().BuildServiceProvider();
    }

    /// <summary>
    /// Disposes benchmark services.
    /// </summary>
    [GlobalCleanup]
    public void Cleanup()
    {
        this._provider.Dispose();
    }

    /// <summary>
    /// Registers the lower-level logging, tracing, and metrics services directly.
    /// </summary>
    /// <returns>The service count after registration.</returns>
    [Benchmark(Baseline = true)]
    public int DirectLowerLevelRegistration()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaLogging();
        _ = services.AddAtyaTracing(options =>
        {
            options.ActivitySourceName = ServiceName;
            options.ActivitySourceVersion = ServiceVersion;
        });
        _ = services.AddAtyaMetrics(options =>
        {
            options.MeterName = ServiceName;
            options.MeterVersion = ServiceVersion;
        });

        return services.Count;
    }

    /// <summary>
    /// Registers observation services through the aggregate extension.
    /// </summary>
    /// <returns>The service count after registration.</returns>
    [Benchmark]
    public int ObservationRegistration()
    {
        var services = CreateObservationServices();

        return services.Count;
    }

    /// <summary>
    /// Builds a provider and resolves the observation identity.
    /// </summary>
    /// <returns>The resolved observation identity.</returns>
    [Benchmark]
    public ObservationIdentity BuildProviderAndResolveIdentity()
    {
        using var provider = CreateObservationServices().BuildServiceProvider();

        return provider.GetRequiredService<ObservationIdentity>();
    }

    /// <summary>
    /// Resolves mapped tracing and metrics options from a warm provider.
    /// </summary>
    /// <returns>The mapped activity source and meter names.</returns>
    [Benchmark]
    public (string? ActivitySourceName, string? MeterName) ResolveMappedOptionsFromWarmProvider()
    {
        var tracingOptions = this._provider.GetRequiredService<IOptions<TracingOptions>>().Value;
        var metricsOptions = this._provider.GetRequiredService<IOptions<MetricsOptions>>().Value;

        return (tracingOptions.ActivitySourceName, metricsOptions.MeterName);
    }

    private static ServiceCollection CreateObservationServices()
    {
        var services = new ServiceCollection();

        _ = services.AddAtyaObservation(ServiceName, ServiceVersion);

        return services;
    }

    private sealed class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            _ = this.AddJob(Job.ShortRun.WithToolchain(InProcessNoEmitToolchain.Instance));
            _ = this.AddValidator(InProcessValidator.DontFailOnError);
        }
    }
}
