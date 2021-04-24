# Securing SPAs using the BFF Pattern

Writing a browser-based application is hard, and when it comes to security the guidance changes every year. It all started with securing your Ajax calls with cookies until we learned that this is prone to CSRF attacks. Then the IETF made JS-based OAuth *official* by introducing the Implicit Flow - until we learned how hard it is to protect against XSS, token leakage and the threat of token exfiltration. Seems you cannot win.

In the meantime the IETF realised that Implicit Flow is an anachronism and will deprecate it. So what's next?

There is on-going work in the [OAuth for browser-based Apps](https://tools.ietf.org/html/draft-ietf-oauth-browser-based-apps) BCP document to give practical guidance on this very topic. Some earlier iterations of this document even came to the conclusion that you should not use OAuth at all in the browser - which is kind of funny for an OAuth working group document (I think this text has been removed since then).

But ultimately the document distinguishes between two architectural approaches: "JavaScript Applications **with** a Backend" and "JavaScript Applications **without** a Backend". If you don't have the luxury of a backend, the more up-to-date recommendation is to use authorization code flow with PKCE and refresh tokens. We think this approach is problematic because it encourages storing your tokens in the browser.

If you have a backend, the backend can help out the frontend with many security related tasks like protocol flow, token storage, token lifetime management, session management etc. With the advent of more modern security features in browsers (e.g. SameSite cookies and CORS), this is our preferred approach and I already detailed this in January 2019 [here](https://leastprivilege.com/2019/01/18/an-alternative-way-to-secure-spas-with-asp-net-core-openid-connect-oauth-2-0-and-proxykit/). This is also often called the BFF (Backend for Frontend) pattern.

Ever since, we helped many of our customers to implement various flavours of the BFF pattern, and we finally decided to take all the lessons learned and distill them into a re-usable library for ASP.NET Core hosts. Before we talk about this, let's have a closer look at all the probems we want to solve.



#### "No tokens in the browser" Policy

This is definitely the elephant in the room. More and more companies are coming to the conclusion that the threat of token exfiltration is too big of an unknown and that no high value access tokens should be stored in JavaScript accessible locations.

It's not only your own code that must be XSS-proof. It's also all the frameworks, libraries and NPM packages you are pulling in (as well as their dependencies). And even worse, you have to worry about other people's code running on your host. The recent work around [Spectre](https://www.securityweek.com/google-releases-poc-exploit-browser-based-spectre-attack) attacks against browsers illustrates nicely that there is more to come.

Storing tokens on the server-side and using encrypted/signed HTTP-only cookies for session management makes that threat model considerably easier. This is not to say that this makes the application automagically secure against content injection, but forcing the attacker through a well defined interface to the backend gives you more leverage.

Since this architecture results in all cross-site API calls being made from the server, there is also SSRF (Server-side request forgery) to be aware of, but again, this is easier to control as opposed to an attacker being able to make arbitrary API calls with an exfiltrated token.



#### React to changes in the browser security models

We wrote about this [before](https://leastprivilege.com/2020/03/31/spas-are-dead/), but in a nutshell browsers are (and will be even more in the future) restricting the usage of cookies across site boundaries to protect users from privacy invasion techniques. The problem is that legitimate OAuth & OpenID Connect protocol interactions are from a browser's point of view indistinguishable from common tracking mechanisms.

This affects:

- front-channel logout notifications (used in pretty much every authentication protocol – like SAML, WS-Fed and OpenID Connect)
- the OpenID Connect JavaScript session management
- the “silent renew” technique that was recommended to give your application session bound token refreshing

To overcome these limitations we need the help of an application backend to bridge the gap to the authentication system, do more robust server-side token management with refresh tokens, and provide support for more future proof mechanisms like back-channel logout notifications.



#### Simplify the JavaScript frontend protocol interactions and make use of advanced features that only exist server-side

And last but not least, writing a robust protocol library for JavaScript is not a trivial task. We are maintaining one of the original OpenID Connect certified JavaScript [libraries](https://github.com/IdentityModel/oidc-client-js), and there is a substantial amount of on-going maintenance necessary due to subtle behaviour changes between browsers and their versions.

On the server-side though (and especially in our case with ASP.NET Core), we have a full featured and stable OpenID Connect client library that supports all the necessary protocol mechanisms and provides an excellent extensibility model for advanced features like Mutual TLS, Proof-of-Possession, JWT secured authorization requests, and JWT-based client authentication.



### Enter Duende.BFF

Duende.BFF is Nuget package that adds all the necessary features required to solve above problems to an ASP.NET Core host. It provides services for session and token management, API endpoint protection and logout notifications to your web-based frontends like SPAs or Blazor WASM applications. Let's have a look at the building blocks.



#### Server-side authentication and session management

Our BFF package relies on ASP.NET Core's excellent authentication handler system to drive all front- and back-channel protocol interactions with an OpenID Connect / OAuth based token service. It also uses the ASP.NET Core cookie plumbing to issue protected, HTTP-only, *secure* and *SameSite* cookies for maintaining the user's session.  We optionally plug into the session storage system to allow server-side session management, which is especially interesting to single logout and session revocation. More on that later.

The BFF package adds three standard endpoints for your frontend to drive session management and interrogation

* */login* to trigger authentication with the configured authentication service
* */logout* to trigger local and upstream logout
* */user* to retrieve the claims of the current user or to inspect session status



#### Protecting local API endpoints

A refactoring process from a SPA without a backend typically involves looking at your API endpoints. Very often the majority of API endpoints used by the frontend are frontend specific - meaning your frontend is the only client calling those APIs. These APIs can be put directly into the BFF host. You can use your favourite endpoint technology (e.g. ASP.NET Core MVC) to provide API endpoints for your frontend. 

The calls to the local APIs will be protected by the session cookie. We recommend using *SameSite* cookies as a first layer of defense against CSRF attacks. Use *strict* mode if possible.

As the name implies, *SameSite* means that the cookies are sandboxed to the same site aka DNS registrar name (e.g. **.mycompany.com*). This means you are effectively trusting *all* applications on your sub-domains. This is a pretty big sandbox, and attacks like [sub-domain takeover](https://blog.cesppa.com/how-subdomain-takeover-attacks-steal-your-customers-and-business-identity/) have shown that this is probably a bit too *lax* (pun intended).

In addition we added plumbing to the BFF host to require an additional static antiforgery header (optional, but on by default). This combination gives you two layers of CSRF protection; the browser's SameSite mechanism for trusting only applications on the same site and in addition isolation to the same origin. The latter is achieved by requiring Ajax calls to have both *credentials* (the cookie) and a custom header. This will always trigger CORS pre-flight request and thus prevent cross-origin callers. 

Additionally we plug into the ASP.NET Core pipeline to make sure that redirects to a login page (in case of an expired session) do not interfere with API/Ajax calls.

#### Calling shared APIs

APIs that are not exclusive to your frontend are hosted in a different backend. These are shared APIs that are typically being used by multiple applications or clients.

To allow your SPA to invoke the shared API, the BFF host will proxy the call. The proxy endpoints are protected just like the local API endpoints above, and will then do a server-to-server call to the remote endpoint. The API call can be anonymous, protected by a client access token (think trusted subsystem) or protected by the user's access token.

You can either create a custom local endpoint to expose some API surface of the remote API, or, if you realize that you would pretty much replicate the remote API surface anyways, can use a reverse proxy to forward the frontend calls.

We embed [YARP](https://github.com/microsoft/reverse-proxy) (the new Microsoft .NET-based reverse proxy) in our BFF package to enable that scenario in a developer friendly way. Again, we automatically protect the reverse proxy endpoints with SameSite cookies and/or anti-forgery protection.

#### Automatic token lifetime management

We also incorporate our [IdentityModel.AspNetCore](https://identitymodel.readthedocs.io/en/latest/aspnetcore/overview.html) library to take care of all token request/refresh needs. This library can manage both client and user tokens and does all the heavy lifting of caching/storing tokens and refreshing them when needed.

The library exposes a super simple API to developers if they want to manually call remote APIs, and is automatically utilized by our reverse proxy endpoints.

#### Integration with single logout and logout notifications

As mentioned above, front-channel logout notification doesn't work reliably anymore. Since this mechanism relies on sending cookies in hidden iframes, it doesn't work anymore with Firefox, Safari or Brave, and other browser will follow soon.

The alternative is called back-channel logout notifications and is frankly a much more robust mechanism. Not relying on browsers doing their "best effort of the day" improves logout tremendously.

Our BFF package provides a spec-compliant implementation of the [OpenID Connect back-channel logout](https://openid.net/specs/openid-connect-backchannel-1_0.html) endpoint and gives you full control to react to logout notifications. This brings us to our last point.

#### Advanced session management features

Our BFF package plugs into the ASP.NET Core authentication session management system to keep your sessions server-side. It also exposes much more information about ongoing sessions (e.g. subject IDs and OpenID Connect session IDs) which allows managing those session more effectively. Our default implementation can automatically destroy sessions based on back-channel logout notifications but you can customize the exact logic.



### Show me the code

Enough talking - what does it look like?

The following is a very typical ASP.NET Core `startup`:

```csharp
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
        // adds BFF services to DI
        // ...also add server-side session management
        // ...also adds access token management
        services.AddBff()
            .AddServerSideSessions();

        // local APIs via MVC controllers
        services.AddControllers();

        // configure server-side authentication and session management
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = "cookie";
                options.DefaultChallengeScheme = "oidc";
                options.DefaultSignOutScheme = "oidc";
            })
            .AddCookie("cookie", options =>
            {
                // host prefixed cookie name
                options.Cookie.Name = "__Host-spa";
                
                // strict SameSite handling
                options.Cookie.SameSite = SameSiteMode.Strict;
            })
            .AddOpenIdConnect("oidc", options =>
            {
                options.Authority = "https://demo.duendesoftware.com";
                
                // confidential client using code flow + PKCE + query response mode
                options.ClientId = "spa";
                options.ClientSecret = "secret";
                options.ResponseType = "code";
                options.ResponseMode = "query";
                options.UsePkce = true;

                options.MapInboundClaims = false;
                options.GetClaimsFromUserInfoEndpoint = true;
                
                // save access and refresh token to enable automatic lifetime management
                options.SaveTokens = true;

                // request scopes
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("api");

                // request refresh token
                options.Scope.Add("offline_access");
            });
    }

    public void Configure(IApplicationBuilder app)
    {
        // static file hosting for SPA frontend
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

            // proxy endpoint for remote APIs
            // all calls to /api/* will be forwarded to the remote API
            // user access token will be attached to API call
            // user access token will be managed automatically using the refresh token
            endpoints.MapRemoteBffApiEndpoint("/api", "https://api.mycompany.com")
                .RequireAccessToken();
        });
    }
}
```

Since all security token and protocol related functions are now managed by the host, the (SPA) front-end simply does local API calls and can safely ignore all the complexities of OAuth and OpenID Connect, e.g.:

```javascript
async function callLocalApi() {
    var req = new Request("/localApi", {
        headers: new Headers({
            // static header to protect against CSRF
            'X-CSRF': '1'
        })
    })
    var resp = await fetch(req);

    // process response
}
```

You can find the full source code of the library and sample JavaScript and Blazor clients [here](https://github.com/DuendeSoftware/BFF).

### How can I use it?

Duende BFF will be part of the [Duende IdentityServer](https://duendesoftware.com/products/identityserver) license. It will be included either in our Business (and up), or Community Edition.

In other words: if you as an individual or your company makes less than one million USD revenue per year, you can use Duende BFF absolutely free of cost. Since this also includes Duende IdentityServer, you can protect up to five SPAs with the free license.

If you make more than one million USD revenue per year - you can get Duende BFF as part of our Business Edition which also includes 15 clients for IdentityServer.

If you have questions about licensing, please [contact](https://duendesoftware.com/contact) us directly.

### Where can I get it?

[Duende.BFF](https://www.nuget.org/packages/Duende.BFF) is a Nuget Package and is currently in Preview 1. We also added a template for the .NET CLI [here](https://github.com/DuendeSoftware/IdentityServer.Templates). Source code and samples can be found [here](https://github.com/DuendeSoftware/BFF).

We would love to get your [feedback](https://github.com/DuendeSoftware/IdentityServer/discussions) and plan to release v1 around May. 


