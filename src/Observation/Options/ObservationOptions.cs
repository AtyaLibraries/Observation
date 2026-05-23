// <copyright file="ObservationOptions.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using System.ComponentModel.DataAnnotations;

namespace Atya.Diagnostics.Observation.Options;

/// <summary>
/// Represents configuration options for the observation package.
/// </summary>
public sealed class ObservationOptions
{
    /// <summary>
    /// Gets or sets the logical service name used across observation components.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the service version used for telemetry identity.
    /// </summary>
    public string? ServiceVersion { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether logging should be configured.
    /// </summary>
    public bool ConfigureLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether tracing should be configured.
    /// </summary>
    public bool ConfigureTracing { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether metrics should be configured.
    /// </summary>
    public bool ConfigureMetrics { get; set; } = true;

    /// <summary>
    /// Gets or sets the activity source name.
    /// When not specified, <see cref="ServiceName" /> should be used.
    /// </summary>
    public string? ActivitySourceName { get; set; }

    /// <summary>
    /// Gets or sets the meter name.
    /// When not specified, <see cref="ServiceName" /> should be used.
    /// </summary>
    public string? MeterName { get; set; }
}
