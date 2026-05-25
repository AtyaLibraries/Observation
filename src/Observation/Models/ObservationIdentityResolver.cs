// <copyright file="ObservationIdentityResolver.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using Atya.Diagnostics.Observation.Options;
using Atya.Foundation.Guards;

namespace Atya.Diagnostics.Observation.Models;

/// <summary>
/// Resolves observation identity values from configured observation options.
/// </summary>
public static class ObservationIdentityResolver
{
    /// <summary>
    /// Resolves the effective observation identity.
    /// </summary>
    /// <param name="options">The observation options.</param>
    /// <returns>The resolved observation identity.</returns>
    public static ObservationIdentity Resolve(ObservationOptions options)
    {
        options = Guard.AgainstNull(options);

        var serviceName = Guard.AgainstNullOrWhiteSpace(options.ServiceName, nameof(options.ServiceName)).Trim();
        var activitySourceName = string.IsNullOrWhiteSpace(options.ActivitySourceName)
            ? serviceName
            : options.ActivitySourceName.Trim();
        var meterName = string.IsNullOrWhiteSpace(options.MeterName)
            ? serviceName
            : options.MeterName.Trim();
        var serviceVersion = string.IsNullOrWhiteSpace(options.ServiceVersion)
            ? null
            : options.ServiceVersion.Trim();

        return new ObservationIdentity(serviceName, serviceVersion, activitySourceName, meterName);
    }
}
