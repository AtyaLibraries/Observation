using System.Diagnostics.CodeAnalysis;
using Atya.Diagnostics.Logging.DependencyInjection;
using Atya.Diagnostics.Metrics.DependencyInjection;
using Atya.Diagnostics.Metrics.Options;
using Atya.Diagnostics.Observation.DependencyInjection;
using Atya.Diagnostics.Observation.Models;
using Atya.Diagnostics.Tracing.DependencyInjection;
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

public static class Program
{
    public static void Main(string[] args)
    {
        _ = BenchmarkRunner.Run<ObservationRegistrationBenchmarks>(args: args);
    }
}

[MemoryDiagnoser]
[Config(typeof(BenchmarkConfig))]
[SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "BenchmarkDotNet requires benchmark methods to be instance methods.")]
public class ObservationRegistrationBenchmarks
{
    private const string ServiceName = "Samples.OrderProcessor";
    private const string ServiceVersion = "1.0.0";

    private ServiceProvider _provider = null!;

    [GlobalSetup]
    public void Setup()
    {
        this._provider = CreateObservationServices().BuildServiceProvider();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        this._provider.Dispose();
    }

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

    [Benchmark]
    public int ObservationRegistration()
    {
        var services = CreateObservationServices();

        return services.Count;
    }

    [Benchmark]
    public ObservationIdentity BuildProviderAndResolveIdentity()
    {
        using var provider = CreateObservationServices().BuildServiceProvider();

        return provider.GetRequiredService<ObservationIdentity>();
    }

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
