// <copyright file="ObservationIdentityResolver.cs" company="Atya">
// Copyright (c) Atya. All rights reserved.
// </copyright>
using Atya.Diagnostics.Observation.Models;
using Atya.Diagnostics.Observation.Options;
using Atya.Foundation.Guards;

namespace Atya.Diagnostics.Observation.Internal;

internal static class ObservationIdentityResolver
{
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
