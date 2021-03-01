using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Duende.Bff;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ReverseProxy.Service.Proxy;

namespace Microsoft.AspNetCore.Builder
{
    public static class BffEndpointRouteBuilderExtensions
    {
        public static void MapBffSessionEndpoints(
            this IEndpointRouteBuilder endpoints,
            string basePath)
        {
            endpoints.MapGet(basePath + "/login", BffManagementEndoints.MapLogin());
            endpoints.MapGet(basePath + "/logout", BffManagementEndoints.MapLogout());
            endpoints.MapGet(basePath + "/user", BffManagementEndoints.MapUser());
        }

        public static IEndpointConventionBuilder MapBffApiEndpoint(
            this IEndpointRouteBuilder endpoints,
            string localPath, 
            string apiAddress, 
            AccessTokenRequirement accessTokenRequirement)
        {
            return endpoints.Map(
                localPath + "/{**catch-all}",
                BffApiEndpoint.Map(localPath, apiAddress, AccessTokenRequirement.OptionalUserToken));
        }
    }
}