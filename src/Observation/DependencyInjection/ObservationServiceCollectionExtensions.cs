// <copyright file="ObservationServiceCollectionExtensions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using Atya.Diagnostics.Logging.DependencyInjection;
using Atya.Diagnostics.Metrics.DependencyInjection;
using Atya.Diagnostics.Metrics.Options;
using Atya.Diagnostics.Observation.Internal;
using Atya.Diagnostics.Observation.Options;
using Atya.Diagnostics.Tracing.DependencyInjection;
using Atya.Diagnostics.Tracing.Options;
using Atya.Foundation.Guards;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Atya.Diagnostics.Observation.DependencyInjection;

/// <summary>
/// Provides dependency injection extensions for registering the observation package.
/// </summary>
public static class ObservationServiceCollectionExtensions
{
    /// <summary>
    /// Registers observation services with the required service identity.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceName">The logical service name used across diagnostics components.</param>
    /// <param name="serviceVersion">The optional service version used for telemetry identity.</param>
    /// <returns>The same <see cref="IServiceCollection" /> instance.</returns>
    public static IServiceCollection AddAtyaObservation(
        this IServiceCollection services,
        string serviceName,
        string? serviceVersion = null)
    {
        _ = Guard.AgainstNull(services);
        _ = Guard.AgainstNullOrWhiteSpace(serviceName);

        return services.AddAtyaObservation(options =>
        {
            options.ServiceName = serviceName;
            options.ServiceVersion = serviceVersion;
        });
    }

    /// <summary>
    /// Registers observation services and composes logging, tracing, and metrics packages.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">An optional delegate used to configure <see cref="ObservationOptions" />.</param>
    /// <returns>The same <see cref="IServiceCollection" /> instance.</returns>
    public static IServiceCollection AddAtyaObservation(
        this IServiceCollection services,
        Action<ObservationOptions>? configure = null)
    {
        _ = Guard.AgainstNull(services);

        _ = services.AddOptions<ObservationOptions>()
            .Configure(options => configure?.Invoke(options))
            .ValidateDataAnnotations()
            .Validate(
                static options => !string.IsNullOrWhiteSpace(options.ServiceName),
                "ObservationOptions.ServiceName cannot be null or whitespace.")
            .ValidateOnStart();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<ObservationOptions>, ObservationOptionsValidator>());
        services.TryAddSingleton(static serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<ObservationOptions>>().Value;
            return ObservationIdentityResolver.Resolve(options);
        });

        _ = services.AddOptions<TracingOptions>()
            .Configure<IOptions<ObservationOptions>>((tracingOptions, observationOptionsAccessor) =>
            {
                var identity = ObservationIdentityResolver.Resolve(observationOptionsAccessor.Value);
                tracingOptions.ActivitySourceName = identity.ActivitySourceName;
                tracingOptions.ActivitySourceVersion = identity.ServiceVersion;
            });

        _ = services.AddOptions<MetricsOptions>()
            .Configure<IOptions<ObservationOptions>>((metricsOptions, observationOptionsAccessor) =>
            {
                var identity = ObservationIdentityResolver.Resolve(observationOptionsAccessor.Value);
                metricsOptions.MeterName = identity.MeterName;
                metricsOptions.MeterVersion = identity.ServiceVersion;
            });

        var bootstrapOptions = new ObservationOptions();
        configure?.Invoke(bootstrapOptions);

        // Defer validation errors to the options validation layer.
        // If ServiceName is missing, skip the eager child-package registration;
        // the OptionsValidationException will fire when IOptions<ObservationOptions>.Value is accessed.
        if (string.IsNullOrWhiteSpace(bootstrapOptions.ServiceName))
        {
            return services;
        }

        var bootstrapIdentity = ObservationIdentityResolver.Resolve(bootstrapOptions);

        if (bootstrapOptions.ConfigureLogging)
        {
            _ = services.AddAtyaLogging();
        }

        if (bootstrapOptions.ConfigureTracing)
        {
            _ = services.AddAtyaTracing(options =>
            {
                options.ActivitySourceName = bootstrapIdentity.ActivitySourceName;
                options.ActivitySourceVersion = bootstrapIdentity.ServiceVersion;
            });
        }

        if (bootstrapOptions.ConfigureMetrics)
        {
            _ = services.AddAtyaMetrics(options =>
            {
                options.MeterName = bootstrapIdentity.MeterName;
                options.MeterVersion = bootstrapIdentity.ServiceVersion;
            });
        }

        return services;
    }

    private sealed class ObservationOptionsValidator : IValidateOptions<ObservationOptions>
    {
        public ValidateOptionsResult Validate(string? name, ObservationOptions options)
        {
            _ = Guard.AgainstNull(options);

            return string.IsNullOrWhiteSpace(options.ServiceName)
                ? ValidateOptionsResult.Fail("ObservationOptions.ServiceName cannot be null or whitespace.")
                : ValidateOptionsResult.Success;
        }
    }
}
