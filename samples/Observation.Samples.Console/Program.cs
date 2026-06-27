using System.Diagnostics;
using System.Diagnostics.Metrics;
using Atya.Diagnostics.Metrics.Abstractions;
using Atya.Diagnostics.Tracing.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var services = new ServiceCollection();

services.AddLogging(builder =>
{
    _ = builder.AddSimpleConsole(options =>
    {
        options.SingleLine = true;
        options.TimestampFormat = "HH:mm:ss ";
    });
});

services.AddAtyaObservation("Samples.OrderProcessor", serviceVersion: "1.0.0");

services.AddTransient<OrderProcessor>();

using var provider = services.BuildServiceProvider();

var processor = provider.GetRequiredService<OrderProcessor>();
await processor.ProcessAsync(Guid.NewGuid());

internal sealed class OrderProcessor(
    ILogger<OrderProcessor> logger,
    IActivitySourceAccessor activitySourceAccessor,
    IMeterAccessor meterAccessor)
{
    private static readonly Action<ILogger, Guid, Exception?> StartingOrderProcessing =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(1000, nameof(StartingOrderProcessing)),
            "Starting order processing for {OrderId}.");

    private static readonly Action<ILogger, Guid, Exception?> OrderProcessingCompleted =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(1001, nameof(OrderProcessingCompleted)),
            "Order processing completed for {OrderId}.");

    private static readonly Action<ILogger, Guid, Exception?> OrderProcessingFailed =
        LoggerMessage.Define<Guid>(
            LogLevel.Error,
            new EventId(1002, nameof(OrderProcessingFailed)),
            "Order processing failed for {OrderId}.");

    private readonly ILogger<OrderProcessor> _logger = logger;
    private readonly IActivitySourceAccessor _activitySourceAccessor = activitySourceAccessor;
    private readonly Counter<long> _requests = meterAccessor.CreateCounter<long>("orders.processed", description: "Total number of processed orders.");
    private readonly Histogram<double> _duration = meterAccessor.CreateHistogram<double>("orders.duration", unit: "ms", description: "Order processing duration.");

    public async Task ProcessAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        using var scope = _logger.BeginScope(new Dictionary<string, object?>
        {
            ["OperationName"] = "ProcessOrder",
            ["OrderId"] = orderId
        });

        using var activity = _activitySourceAccessor.StartInternalActivity("ProcessOrder");
        var startedAt = Stopwatch.GetTimestamp();

        StartingOrderProcessing(_logger, orderId, null);
        _requests.Add(1, new KeyValuePair<string, object?>("operation", "ProcessOrder"));

        try
        {
            await Task.Delay(150, cancellationToken);
            _duration.Record(
                Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds,
                new KeyValuePair<string, object?>("operation", "ProcessOrder"),
                new KeyValuePair<string, object?>("outcome", "success"));

            OrderProcessingCompleted(_logger, orderId, null);
        }
        catch (Exception exception)
        {
            _duration.Record(
                Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds,
                new KeyValuePair<string, object?>("operation", "ProcessOrder"),
                new KeyValuePair<string, object?>("outcome", "failure"));

            OrderProcessingFailed(_logger, orderId, exception);
            throw;
        }
    }
}
