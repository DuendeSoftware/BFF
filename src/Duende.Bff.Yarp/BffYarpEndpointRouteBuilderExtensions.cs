// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.Bff;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Duende.Bff.Endpoints;
using Duende.Bff.Yarp;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extension methods for the BFF endpoints
    /// </summary>
    public static class BffYarpEndpointRouteBuilderExtensions
    {
        // todo
        //internal static bool _licenseChecked;
        
        /// <summary>
        /// Adds a remote BFF API endpoint
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="localPath"></param>
        /// <param name="apiAddress"></param>
        /// <param name="requireAntiForgeryCheck"></param>
        /// <returns></returns>
        public static IEndpointConventionBuilder MapRemoteApiEndpoint(
            this IEndpointRouteBuilder endpoints,
            PathString localPath, 
            string apiAddress, 
            bool requireAntiForgeryCheck = true)
        {
            endpoints.CheckLicense();
            
            return endpoints.Map(
                    localPath.Add("/{**catch-all}").Value,
                    RemoteApiEndpoint.Map(localPath, apiAddress))
                .WithMetadata(new BffRemoteApiEndpointMetadata { RequireAntiForgeryHeader =  requireAntiForgeryCheck });

        }
    }
}