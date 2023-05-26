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

    public static void ValidateLicenseForProduct(IList<string> errors)
    {
        if (!_license.BffFeature)
        {
            errors.Add($"Your Duende software license does not include the BFF feature.");
        }
    }
}