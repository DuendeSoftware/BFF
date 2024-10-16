// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

#nullable disable

using Duende.Bff;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Duende;

// APIs needed for IdentityServer specific license validation
internal partial class LicenseValidator
{
    public static void Initalize(ILoggerFactory loggerFactory, BffOptions options)
    {
        Initalize(loggerFactory, "Bff", options.LicenseKey);
    }

    // this should just add to the error list
    public static void ValidateProductFeaturesForLicense(IList<string> errors)
    {
        if (!_license.BffFeature)
        {
            errors.Add($"Your Duende software license does not include the BFF feature.");
        }
    }
    static void WarnForProductFeaturesWhenMissingLicense()
    {
        // none
    }
}