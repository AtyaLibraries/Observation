// <copyright file="ObservationIdentity.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using Atya.Foundation.Guards;

namespace Atya.Diagnostics.Observation.Models;

/// <summary>
/// Represents the resolved observation identity used by logging, tracing, and metrics.
/// </summary>
public sealed record ObservationIdentity
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObservationIdentity" /> class.
    /// </summary>
    /// <param name="serviceName">The logical service name.</param>
    /// <param name="serviceVersion">The optional service version.</param>
    /// <param name="activitySourceName">The resolved activity source name.</param>
    /// <param name="meterName">The resolved meter name.</param>
    public ObservationIdentity(
        string serviceName,
        string? serviceVersion,
        string activitySourceName,
        string meterName)
    {
        ServiceName = Guard.AgainstNullOrWhiteSpace(serviceName).Trim();
        ServiceVersion = string.IsNullOrWhiteSpace(serviceVersion) ? null : serviceVersion.Trim();
        ActivitySourceName = Guard.AgainstNullOrWhiteSpace(activitySourceName).Trim();
        MeterName = Guard.AgainstNullOrWhiteSpace(meterName).Trim();
    }

    /// <summary>
    /// Gets the logical service name.
    /// </summary>
    public string ServiceName { get; }

    /// <summary>
    /// Gets the optional service version.
    /// </summary>
    public string? ServiceVersion { get; }

    /// <summary>
    /// Gets the resolved activity source name.
    /// </summary>
    public string ActivitySourceName { get; }

    /// <summary>
    /// Gets the resolved meter name.
    /// </summary>
    public string MeterName { get; }
}
