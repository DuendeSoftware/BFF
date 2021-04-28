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
        private static Task ProcessWith<T>(HttpContext context)
            where T : IBffEndpointService
        {
            var service = context.RequestServices.GetRequiredService<T>();
            return service.ProcessRequequestAsync(context);
        }

        /// <summary>
        /// Adds the BFF management endpoints (login, logout, logout notifications)
        /// </summary>
        /// <param name="endpoints"></param>
        public static void MapBffManagementEndpoints(
            this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapBffManagementLoginEndpoint();
            endpoints.MapBffManagementLogoutEndpoint();
            endpoints.MapBffManagementUserEndpoint();
            endpoints.MapBffManagementBackchannelEndpoint();
        }

        /// <summary>
        /// Adds the login BFF management endpoint
        /// </summary>
        /// <param name="endpoints"></param>
        public static void MapBffManagementLoginEndpoint(
            this IEndpointRouteBuilder endpoints)
        {
            var options = endpoints.ServiceProvider.GetRequiredService<BffOptions>();

            endpoints.MapGet(options.ManagementBasePath.Add("/login"), ProcessWith<ILoginService>);
        }

        /// <summary>
        /// Adds the logout BFF management endpoint
        /// </summary>
        /// <param name="endpoints"></param>
        public static void MapBffManagementLogoutEndpoint(
            this IEndpointRouteBuilder endpoints)
        {
            var options = endpoints.ServiceProvider.GetRequiredService<BffOptions>();

            endpoints.MapGet(options.ManagementBasePath.Add("/logout"), ProcessWith<ILogoutService>);
        }

        /// <summary>
        /// Adds the user BFF management endpoint
        /// </summary>
        /// <param name="endpoints"></param>
        public static void MapBffManagementUserEndpoint(
            this IEndpointRouteBuilder endpoints)
        {
            var options = endpoints.ServiceProvider.GetRequiredService<BffOptions>();

            endpoints.MapGet(options.ManagementBasePath.Add("/user"), ProcessWith<IUserService>);
        }

        /// <summary>
        /// Adds the back channel BFF management endpoint
        /// </summary>
        /// <param name="endpoints"></param>
        public static void MapBffManagementBackchannelEndpoint(
            this IEndpointRouteBuilder endpoints)
        {
            var options = endpoints.ServiceProvider.GetRequiredService<BffOptions>();

            endpoints.MapPost(options.ManagementBasePath.Add("/backchannel"), ProcessWith<IBackchannelLogoutService>);
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