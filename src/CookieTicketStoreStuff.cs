using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Duende.Bff
{
    public class CookieTicketStore : ITicketStore
    {
        private readonly IUserSessionStore _store;
        private readonly ILogger<CookieTicketStore> _logger;

        public CookieTicketStore(
            IUserSessionStore store,
            ILogger<CookieTicketStore> logger)
        {
            _store = store;
            _logger = logger;
        }

        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var key = CryptoRandom.CreateUniqueId(format: CryptoRandom.OutputFormat.Hex);

            var session = new UserSession
            {
                Key = key,
                Created = ticket.GetIssued(),
                Renewed = ticket.GetIssued(),
                Expires = ticket.GetExpiration(),
                SubjectId = ticket.GetSubjectId(),
                SessionId = ticket.GetSessionId(),
                Scheme = ticket.AuthenticationScheme,
                Ticket = ticket.Serialize(),
            };

            await _store.CreateUserSessionAsync(session);

            return key;
        }

        public async Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            var session = await _store.GetUserSessionAsync(key);
            if (session != null)
            {
                var ticket = session.Deserialize();
                if (ticket == null)
                {
                    // if we failed to get a ticket, then remove DB record 
                    _logger.LogWarning("Failed to deserialize authentication ticket from store, deleting record for key {key}", key);
                    await RemoveAsync(key);
                }

                return ticket;
            }

            return null;
        }

        public async Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            var session = await _store.GetUserSessionAsync(key);
            if (session != null)
            {
                session.Renewed = ticket.GetIssued();
                session.Expires = ticket.GetExpiration();
                session.Ticket = ticket.Serialize();

                // todo: discuss updating sub and sid?
                
                await _store.UpdateUserSessionAsync(session);
            }
            else
            {
                _logger.LogWarning("No record found in user session store when trying to renew authentication ticket for key {key} and subject {subjectId}", key, ticket.GetSubjectId());
            }
        }

        public Task RemoveAsync(string key)
        {
            return _store.DeleteUserSessionAsync(key);
        }
    }

    public static class AuthenticationTicketExtensions
    {
        public static string GetSubjectId(this AuthenticationTicket ticket)
        {
            return ticket.Principal.FindFirst(JwtClaimTypes.Subject)?.Value ??
                   ticket.Principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                   // for the mfa remember me cookie, ASP.NET Identity uses the 'name' claim for the subject id (for some reason)
                   ticket.Principal.FindFirst(ClaimTypes.Name)?.Value;
        }

        public static string GetSessionId(this AuthenticationTicket ticket)
        {
            return ticket.Principal.FindFirst(JwtClaimTypes.SessionId)?.Value;
        }
        public static DateTime GetIssued(this AuthenticationTicket ticket)
        {
            return ticket.Properties.IssuedUtc.HasValue ?
                ticket.Properties.IssuedUtc.Value.UtcDateTime : DateTime.UtcNow;
        }
        public static DateTime? GetExpiration(this AuthenticationTicket ticket)
        {
            return ticket.Properties.ExpiresUtc.HasValue ?
                ticket.Properties.ExpiresUtc.Value.UtcDateTime : default(DateTime?);
        }

        public static ClaimsPrincipal ToClaimsPrincipal(this ClaimsPrincipalLite principal)
        {
            var claims = principal.Claims.Select(x => new Claim(x.Type, x.Value, x.ValueType ?? ClaimValueTypes.String)).ToArray();
            var id = new ClaimsIdentity(claims, principal.AuthenticationType);
            return new ClaimsPrincipal(id);
        }
        public static ClaimsPrincipalLite ToClaimsPrincipalLite(this ClaimsPrincipal principal)
        {
            var claims = principal.Claims.Select(
                    x => new ClaimLite
                    {
                        Type = x.Type,
                        Value = x.Value,
                        ValueType = x.ValueType == ClaimValueTypes.String ? null : x.ValueType
                    }).ToArray();

            return new ClaimsPrincipalLite
            {
                AuthenticationType = principal.Identity.AuthenticationType,
                Claims = claims
            };
        }

        
        static JsonSerializerOptions __jsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public static string Serialize(this AuthenticationTicket ticket)
        {
            var data = new AuthenticationTicketLite
            {
                Scheme = ticket.AuthenticationScheme,
                User = ticket.Principal.ToClaimsPrincipalLite(),
                Items = ticket.Properties.Items,
            };
            
            // todo: data protect? PII, etc?
            var value = JsonSerializer.Serialize(data, __jsonOptions);
            return value;
        }
        public static AuthenticationTicket Deserialize(this UserSession session)
        {
            try
            {
                var ticket = JsonSerializer.Deserialize<AuthenticationTicketLite>(session.Ticket, __jsonOptions);

                var user = ticket.User.ToClaimsPrincipal();
                var properties = new AuthenticationProperties(ticket.Items);

                // this allows us to extend the session from the DB column rather than from the payload
                if (session.Expires.HasValue)
                {
                    properties.ExpiresUtc = new DateTimeOffset(session.Expires.Value, TimeSpan.Zero);
                }
                else
                {
                    properties.ExpiresUtc = null;
                }

                return new AuthenticationTicket(user, properties, ticket.Scheme);
            }
            catch 
            {
                // failed deserialize
            }
            
            return null;
        }

        public class AuthenticationTicketLite
        {
            public string Scheme { get; set; }
            public ClaimsPrincipalLite User { get; set; }
            public IDictionary<string, string> Items { get; set; }
        }
        public class ClaimLite
        {
            public string Type { get; set; }
            public string Value { get; set; }
            public string ValueType { get; set; }
        }
        public class ClaimsPrincipalLite
        {
            public string AuthenticationType { get; set; }
            public ClaimLite[] Claims { get; set; }
        }
    }

    // this shim class is needed since ITicketStore is not configure in DI, rather it's a property 
    // of the cookie options and coordinated with PostConfigureApplicationCookie. #lame
    // https://github.com/aspnet/AspNetCore/issues/6946
    public class TicketStoreShim : ITicketStore
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TicketStoreShim(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public ITicketStore Inner
        {
            get
            {
                return _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<ITicketStore>();
            }
        }

        public Task RemoveAsync(string key)
        {
            return Inner.RemoveAsync(key);
        }

        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            return Inner.RenewAsync(key, ticket);
        }

        public Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            return Inner.RetrieveAsync(key);
        }

        public Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            return Inner.StoreAsync(ticket);
        }
    }

    public class PostConfigureApplicationCookie : IPostConfigureOptions<CookieAuthenticationOptions>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PostConfigureApplicationCookie(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void PostConfigure(string name, CookieAuthenticationOptions options)
        {
            options.SessionStore = new TicketStoreShim(_httpContextAccessor);
        }
    }


    //public class TicketCleanupService : IHostedService
    //{
    //    private readonly IServiceProvider _serviceProvider;
    //    private readonly SessionManagementOptions _options;
    //    private readonly ILogger<TicketCleanupService> _logger;

    //    private CancellationTokenSource _source;

    //    public TicketCleanupService(
    //        IServiceProvider serviceProvider,
    //        SessionManagementOptions options,
    //        ILogger<TicketCleanupService> logger)
    //    {
    //        _serviceProvider = serviceProvider;
    //        _options = options;
    //        _logger = logger;
    //    }

    //    public Task StartAsync(CancellationToken cancellationToken)
    //    {
    //        if (_options.EnableSessionCleanupInterval)
    //        {
    //            if (_source != null) throw new InvalidOperationException("Already started. Call Stop first.");

    //            _logger.LogDebug("Starting ticket cleanup");

    //            _source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

    //            Task.Factory.StartNew(() => StartInternalAsync(_source.Token));
    //        }

    //        return Task.CompletedTask;
    //    }

    //    public Task StopAsync(CancellationToken cancellationToken)
    //    {
    //        if (_options.EnableSessionCleanupInterval)
    //        {
    //            if (_source == null) throw new InvalidOperationException("Not started. Call Start first.");

    //            _logger.LogDebug("Stopping ticket cleanup");

    //            _source.Cancel();
    //            _source = null;
    //        }

    //        return Task.CompletedTask;
    //    }

    //    private async Task StartInternalAsync(CancellationToken cancellationToken)
    //    {
    //        while (true)
    //        {
    //            if (cancellationToken.IsCancellationRequested)
    //            {
    //                _logger.LogDebug("CancellationRequested. Exiting.");
    //                break;
    //            }

    //            try
    //            {
    //                await Task.Delay(_options.SessionCleanupInterval, cancellationToken);
    //            }
    //            catch (TaskCanceledException)
    //            {
    //                _logger.LogDebug("TaskCanceledException. Exiting.");
    //                break;
    //            }
    //            catch (Exception ex)
    //            {
    //                _logger.LogError("Task.Delay exception: {0}. Exiting.", ex.Message);
    //                break;
    //            }

    //            if (cancellationToken.IsCancellationRequested)
    //            {
    //                _logger.LogDebug("CancellationRequested. Exiting.");
    //                break;
    //            }

    //            await RemoveExpiredTicketsAsync();
    //        }
    //    }

    //    private async Task RemoveExpiredTicketsAsync()
    //    {
    //        try
    //        {
    //            _logger.LogTrace("Querying for expired tickets to remove");

    //            using (var serviceScope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
    //            {
    //                using (var context = serviceScope.ServiceProvider.GetService<SessionManagementDbContext>())
    //                {
    //                    await RemoveExpiredTicketsAsync(context);
    //                }
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError("Exception removing expired tickets: {exception}", ex.Message);
    //        }
    //    }

    //    private async Task RemoveExpiredTicketsAsync(SessionManagementDbContext context)
    //    {
    //        var found = Int32.MaxValue;

    //        while (found >= _options.SessionCleanupBatchSize)
    //        {
    //            var expiredItems = await context.UserSessions
    //                .Where(x => x.Expires < DateTime.UtcNow)
    //                .OrderBy(x => x.Id)
    //                .Take(_options.SessionCleanupBatchSize)
    //                .ToArrayAsync();

    //            found = expiredItems.Length;
    //            _logger.LogInformation("Removing {expiredItems} tickets", found);

    //            if (found > 0)
    //            {
    //                context.UserSessions.RemoveRange(expiredItems);
    //                try
    //                {
    //                    await context.SaveChangesAsync();
    //                }
    //                catch (DbUpdateConcurrencyException ex)
    //                {
    //                    // we get this if/when someone else already deleted the records
    //                    // we want to essentially ignore this, and keep working
    //                    _logger.LogDebug("Concurrency exception removing expired tickets: {exception}", ex.Message);
    //                }
    //            }
    //        }
    //    }
    //}
}
