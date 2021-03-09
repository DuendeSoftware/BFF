using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Hosting;
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

namespace Duende.Bff
{
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
