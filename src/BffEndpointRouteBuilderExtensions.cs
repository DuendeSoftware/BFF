using Duende.Bff;
using Microsoft.AspNetCore.Routing;

namespace Microsoft.AspNetCore.Builder
{
    public static class BffEndpointRouteBuilderExtensions
    {
        public static void MapBffManagementEndpoints(
            this IEndpointRouteBuilder endpoints,
            string basePath)
        {
            endpoints.MapGet(basePath + "/login", BffManagementEndoints.MapLogin());
            endpoints.MapGet(basePath + "/logout", BffManagementEndoints.MapLogout());
            endpoints.MapGet(basePath + "/user", BffManagementEndoints.MapUser());
            endpoints.MapGet(basePath + "/xsrf", BffManagementEndoints.MapXsrfToken());
            endpoints.MapPost(basePath + "/backchannel", BffManagementEndoints.BackchannelLogout);
        }

        public static IEndpointConventionBuilder MapBffApiEndpoint(
            this IEndpointRouteBuilder endpoints,
            string localPath, 
            string apiAddress)
        {
            return endpoints.Map(
                    localPath + "/{**catch-all}",
                    BffApiEndpoint.Map(localPath, apiAddress))
                .WithMetadata(new BffApiEndointMetadata());

        }
    }
}