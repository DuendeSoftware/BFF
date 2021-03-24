using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Duende.Bff.Tests.TestFramework
{
    public class IdentityServerHost : GenericHost
    {
        public IdentityServerHost(string baseAddress = "https://identityserver") 
            : base(baseAddress)
        {
            OnConfigureServices += ConfigureServices;
            OnConfigure += Configure;
        }

        public List<Client> Clients { get; set; } = new List<Client>();
        public List<IdentityResource> IdentityResources { get; set; } = new List<IdentityResource>()
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
        };
        public List<ApiScope> ApiScopes { get; set; } = new List<ApiScope>();

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddAuthorization();

            services.AddLogging(logging => {
                logging.AddFilter("Duende", LogLevel.Debug);
            });

            services.AddIdentityServer()
                .AddInMemoryClients(Clients)
                .AddInMemoryIdentityResources(IdentityResources)
                .AddInMemoryApiScopes(ApiScopes);
        }

        private void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/account/login", context =>
                {
                    return Task.CompletedTask;
                });
                endpoints.MapGet("/account/logout", context =>
                {
                    return Task.CompletedTask;
                });
            });
        }
    }
}
