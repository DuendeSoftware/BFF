// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.JsonWebTokens;

namespace Duende.Bff.Endpoints
{
    internal class LicenseValidator
    {
        static readonly string[] LicenseFileNames = new[]
        {
            "Duende_License.key",
            "Duende_IdentityServer_License.key",
        };

        static ILogger? _logger;
        static License? _license;

        public static void Initalize(ILoggerFactory loggerFactory, BffOptions options)
        {
            _logger = loggerFactory.CreateLogger("Duende.Bff");

            var key = options.LicenseKey ?? LoadFromFile();
            _license = ValidateKey(key);
        }

        private static string? LoadFromFile()
        {
            foreach (var name in LicenseFileNames)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), name);
                if (File.Exists(path))
                {
                    return File.ReadAllText(path).Trim();
                }
            }

            return null;
        }

        public static void ValidateLicense()
        {
            var errors = new List<string>();

            if (_license == null)
            {
                var message = "You do not have a valid license key for the Duende software. " +
                              "This is allowed for development and testing scenarios. " +
                              "If you are running in production you are required to have a licensed version. Please start a conversation with us: https://duendesoftware.com/contact";

                _logger.LogWarning(message);
                return;
            }
            else if (!_license.BffFeature)
            {
                errors.Add($"Your Duende software license does not include the BFF feature.");
            }
            else
            {
                Action<string, object[]> func = _license.ISVFeature ? _logger.LogTrace : _logger.LogDebug;
                func.Invoke("The validated licence key details: {@license}", new[] { _license });

                if (_license.Expiration.HasValue)
                {
                    var diff = DateTime.UtcNow.Date.Subtract(_license.Expiration.Value.Date).TotalDays;
                    if (diff > 0 && !_license.ISVFeature)
                    {
                        errors.Add($"Your license for the Duende software expired {diff} days ago.");
                    }
                }
            }

            if (errors.Count > 0)
            {
                foreach (var err in errors)
                {
                    _logger.LogError(err);
                }

                if (_license != null)
                {
                    _logger.LogError(
                        "Please contact {licenceContact} from {licenseCompany} to obtain a valid license for the Duende software.",
                        _license.ContactInfo, _license.CompanyName);
                }
            }
            else
            {
                if (_license.Expiration.HasValue)
                {
                    Action<string, object[]> log = _license.ISVFeature ? _logger.LogTrace : _logger.LogInformation;
                    log.Invoke("You have a valid license key for the Duende software {edition} edition for use at {licenseCompany}. The license expires on {licenseExpiration}.",
                        new object[] { _license.Edition, _license.CompanyName, _license.Expiration.Value.ToLongDateString() });
                }
                else
                {
                    Action<string, object[]> log = _license.ISVFeature ? _logger.LogTrace : _logger.LogInformation;
                    log.Invoke(
                        "You have a valid license key for the Duende software {edition} edition for use at {licenseCompany}.",
                        new object[] { _license.Edition, _license.CompanyName });
                }
            }
        }

        internal static License? ValidateKey(string? licenseKey)
        {
            if (!String.IsNullOrWhiteSpace(licenseKey))
            {
                var handler = new JsonWebTokenHandler();
                
                var rsa = new RSAParameters
                {
                    Exponent = Convert.FromBase64String("AQAB"),
                    Modulus = Convert.FromBase64String(
                        "tAHAfvtmGBng322TqUXF/Aar7726jFELj73lywuCvpGsh3JTpImuoSYsJxy5GZCRF7ppIIbsJBmWwSiesYfxWxBsfnpOmAHU3OTMDt383mf0USdqq/F0yFxBL9IQuDdvhlPfFcTrWEL0U2JsAzUjt9AfsPHNQbiEkOXlIwtNkqMP2naynW8y4WbaGG1n2NohyN6nfNb42KoNSR83nlbBJSwcc3heE3muTt3ZvbpguanyfFXeoP6yyqatnymWp/C0aQBEI5kDahOU641aDiSagG7zX1WaF9+hwfWCbkMDKYxeSWUkQOUOdfUQ89CQS5wrLpcU0D0xf7/SrRdY2TRHvQ=="),
                };

                var key = new RsaSecurityKey(rsa)
                {
                    KeyId = "IdentityServerLicensekey/7ceadbb78130469e8806891025414f16"
                };

                var parms = new TokenValidationParameters
                {
                    ValidIssuer = "https://duendesoftware.com",
                    ValidAudience = "IdentityServer",
                    IssuerSigningKey = key,
                    ValidateLifetime = false
                };

                var validateResult = handler.ValidateToken(licenseKey, parms);
                if (validateResult.IsValid)
                {
                    return new License(new ClaimsPrincipal(validateResult.ClaimsIdentity));
                }

                _logger.LogCritical(validateResult.Exception, "Error validating Duende license key");
            }

            return null;
        }
    }
}