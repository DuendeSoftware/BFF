// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.Bff;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extension methods for the BFF endpoints
    /// </summary>
    public static class BffEndpointRouteBuilderExtensions
    {
        static Task ProcessWith<T>(HttpContext context)
            where T : IBffEndpointService
        {
            var service = context.RequestServices.GetRequiredService<T>();
            return service.ProcessRequequestAsync(context);
        }

        /// <summary>
        /// Adds the BFF management endpoints (login, logout, logout notifications)
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="basePath"></param>
        public static void MapBffManagementEndpoints(
            this IEndpointRouteBuilder endpoints,
            string basePath = BffDefaults.ManagementBasePath)
        {
            endpoints.MapGet(basePath + "/login", ProcessWith<ILoginService>);
            endpoints.MapGet(basePath + "/logout", ProcessWith<ILogoutService>);
            endpoints.MapGet(basePath + "/user", ProcessWith<IUserService>);
            endpoints.MapPost(basePath + "/backchannel", ProcessWith<IBackchannelLogoutService>);
        }

        /// <summary>
        /// Adds a remote BFF API endpoint
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="localPath"></param>
        /// <param name="apiAddress"></param>
        /// <returns></returns>
        public static IEndpointConventionBuilder MapRemoteBffApiEndpoint(
            this IEndpointRouteBuilder endpoints,
            string localPath, 
            string apiAddress)
        {
            return endpoints.Map(
                    localPath + "/{**catch-all}",
                    BffRemoteApiEndpoint.Map(localPath, apiAddress))
                .WithMetadata(new BffRemoteApiEndpointMetadata());

        }
    }
}