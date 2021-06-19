// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Net.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Duende.Bff
{
    // todo: check with latest - should be public in YARP?
    internal static class RequestUtilities
    {
        // Note: HttpClient.SendAsync will end up sending the union of
        // HttpRequestMessage.Headers and HttpRequestMessage.Content.Headers.
        // We don't really care where the proxied headers appear among those 2,
        // as long as they appear in one (and only one, otherwise they would be duplicated).
        internal static void AddHeader(HttpRequestMessage request, string headerName, StringValues value)
        {
            // HttpClient wrongly uses comma (",") instead of semi-colon (";") as a separator for Cookie headers.
            // To mitigate this, we concatenate them manually and put them back as a single header value.
            // A multi-header cookie header is invalid, but we get one because of
            // https://github.com/dotnet/aspnetcore/issues/26461
            if (string.Equals(headerName, HeaderNames.Cookie, StringComparison.OrdinalIgnoreCase) && value.Count > 1)
            {
                value = string.Join("; ", value);
            }

            if (value.Count == 1)
            {
                string headerValue = value;
                if (!request.Headers.TryAddWithoutValidation(headerName, headerValue))
                {
                    var added = request.Content?.Headers.TryAddWithoutValidation(headerName, headerValue);
                    // TODO: Log. Today this assert fails for a POST request with Content-Length: 0 header which is valid.
                    // https://github.com/microsoft/reverse-proxy/issues/618
                    // Debug.Assert(added.GetValueOrDefault(), $"A header was dropped; {headerName}: {headerValue}");
                }
            }
            else
            {
                string[] headerValues = value;
                if (!request.Headers.TryAddWithoutValidation(headerName, headerValues))
                {
                    var added = request.Content?.Headers.TryAddWithoutValidation(headerName, headerValues);
                    // TODO: Log. Today this assert fails for a POST request with Content-Length: 0 header which is valid.
                    // https://github.com/microsoft/reverse-proxy/issues/618
                    // Debug.Assert(added.GetValueOrDefault(), $"A header was dropped; {headerName}: {string.Join(", ", headerValues)}");
                }
            }
        }
    }
}