using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Host5
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBff()
                .AddServerSideSessions();

            // local APIs
            services.AddControllers();

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "cookie";
                    options.DefaultChallengeScheme = "oidc";
                    options.DefaultSignOutScheme = "oidc";
                })
                .AddCookie("cookie", options =>
                {
                    options.Cookie.Name = "__Host-spa5";
                    options.Cookie.SameSite = SameSiteMode.Strict;
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = "https://localhost:5001";
                    options.ClientId = "spa";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";
                    options.ResponseMode = "query";

                    options.MapInboundClaims = false;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;

                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("scope1");
                    options.Scope.Add("offline_access");
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseSerilogRequestLogging();
            app.UseDeveloperExceptionPage();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseRouting();
            
            // adds antiforgery protection for local APIs
            app.UseBff();
            
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // local APIs
                endpoints.MapControllers()
                    .RequireAuthorization()
                    .AsLocalBffApiEndpoint();

                // login, logout, user, backchannel logout...
                endpoints.MapBffManagementEndpoints();

                // proxy endpoint for cross-site APIs
                endpoints.MapRemoteBffApiEndpoint("/api", "https://localhost:5010")
                    .RequireAccessToken();
            });
        }
    }
}