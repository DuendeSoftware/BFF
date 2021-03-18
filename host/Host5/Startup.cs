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
            // Add BFF services to DI - also add server-side session management
            services.AddBff()
                .AddServerSideSessions();

            // local APIs
            services.AddControllers();

            // cookie options
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "cookie";
                    options.DefaultChallengeScheme = "oidc";
                    options.DefaultSignOutScheme = "oidc";
                })
                .AddCookie("cookie", options =>
                {
                    // host prefixed cookie name
                    options.Cookie.Name = "__Host-spa5";
                    
                    // strict SameSite handling
                    options.Cookie.SameSite = SameSiteMode.Strict;
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = "https://localhost:5005";
                    
                    // confidential client using code flow + PKCE
                    options.ClientId = "spa";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";
                    options.ResponseMode = "query";

                    options.MapInboundClaims = false;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;

                    // request scopes + refresh tokens
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
            
            // adds authorization for local and remote API endpoints
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
                // all calls to /api/* will be forwarded to the remote API
                // user access token will be attached in API call
                // user access token will be managed automatically using the refresh token
                endpoints.MapRemoteBffApiEndpoint("/api", "https://localhost:5002")
                    .RequireAccessToken();
            });
        }
    }
}