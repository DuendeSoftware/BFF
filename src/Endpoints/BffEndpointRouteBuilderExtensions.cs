using Duende.Bff;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Extension methods for the BFF endpoints
    /// </summary>
    public static class BffEndpointRouteBuilderExtensions
    {
        /// <summary>
        /// Adds the BFF management endpoints (login, logout, logout notifications)
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="basePath"></param>
        public static void MapBffManagementEndpoints(
            this IEndpointRouteBuilder endpoints,
            string basePath = "/bff")
        {
            endpoints.MapGet(basePath + "/login", BffManagementEndoints.MapLogin);
            endpoints.MapGet(basePath + "/logout", BffManagementEndoints.MapLogout);
            endpoints.MapGet(basePath + "/user", BffManagementEndoints.MapUser);
            
            endpoints.MapPost(basePath + "/backchannel", BffManagementEndoints.MapBackchannelLogout);
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