# Atya.Diagnostics.Observation

Atya.Diagnostics.Observation is a thin composition package for the diagnostics building blocks.

It gives you one clean registration point for:
- logging
- tracing
- metrics

This package does not replace the lower-level packages.
It sits above them and helps you apply shared service identity and consistent naming.

## What this package provides

- `AddAtyaObservation(...)`
- shared `ObservationOptions`
- shared service identity mapping
- consistent defaults for tracing and metrics names
- optional enable/disable flags for logging, tracing, and metrics

## What this package does not provide

This package intentionally does **not** configure:
- OpenTelemetry SDK
- OTLP / Jaeger / Zipkin / Prometheus exporters
- Serilog
- ASP.NET Core middleware
- dashboards
- sinks
- vendor-specific integrations

## Installation

```bash
dotnet add package Atya.Diagnostics.Observation
```

## Basic usage

```csharp
using Atya.Diagnostics.Observation.DependencyInjection;

var services = new ServiceCollection();

services.AddAtyaObservation(options =>
{
    options.ServiceName = "Samples.OrderProcessor";
    options.ServiceVersion = "1.0.0";
});
```

For the common case, pass the service identity directly:

```csharp
services.AddAtyaObservation("Samples.OrderProcessor", serviceVersion: "1.0.0");
```

## Shared naming behavior

```csharp
services.AddAtyaObservation(options =>
{
    options.ServiceName = "Orders.Service";
});
```

Defaults:
- `ActivitySourceName = ServiceName`
- `MeterName = ServiceName`

Override when needed:

```csharp
services.AddAtyaObservation(options =>
{
    options.ServiceName = "Orders.Service";
    options.ActivitySourceName = "Orders.Tracing";
    options.MeterName = "Orders.Metrics";
});
```

## Selective registration

```csharp
services.AddAtyaObservation(options =>
{
    options.ServiceName = "Orders.Service";
    options.ConfigureLogging = true;
    options.ConfigureTracing = true;
    options.ConfigureMetrics = false;
});
```

## Example with lower-level packages

```csharp
public sealed class OrderProcessor
{
    private readonly ILogger<OrderProcessor> _logger;
    private readonly IActivitySourceAccessor _activitySourceAccessor;
    private readonly IMeterAccessor _meterAccessor;
    private readonly Counter<long> _requests;
    private readonly Histogram<double> _duration;

    public OrderProcessor(
        ILogger<OrderProcessor> logger,
        IActivitySourceAccessor activitySourceAccessor,
        IMeterAccessor meterAccessor)
    {
        _logger = logger;
        _activitySourceAccessor = activitySourceAccessor;
        _meterAccessor = meterAccessor;

        _requests = _meterAccessor.CreateCounter<long>("orders.processed");
        _duration = _meterAccessor.CreateHistogram<double>("orders.duration", unit: "ms");
    }

    public async Task ProcessAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["OperationName"] = "ProcessOrder",
            ["OrderId"] = orderId
        });

        using var activity = _activitySourceAccessor.StartInternalActivity("ProcessOrder");
        var startedAt = Stopwatch.GetTimestamp();

        _requests.Add(1);

        try
        {
            await Task.Delay(50, cancellationToken);
            _duration.Record(Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);
        }
        catch
        {
            _duration.Record(Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds);
            throw;
        }
    }
}
```

## When to use this package

Use this package when you want:
- one diagnostics registration entry point
- shared service identity across diagnostics packages
- a simpler onboarding experience for new services
- consistent diagnostics naming across your applications

Use the lower-level packages directly when you need more control over one specific area.

## Validation

`ServiceName` is required and cannot be null, empty, or whitespace.
Names and versions are trimmed before they are mapped to tracing and metrics options.
