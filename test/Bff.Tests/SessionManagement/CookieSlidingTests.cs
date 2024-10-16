﻿// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.Bff.Tests.TestFramework;
using Duende.Bff.Tests.TestHosts;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;


namespace Duende.Bff.Tests.SessionManagement
{
    public class CookieSlidingTests : BffIntegrationTestBase
    {
        InMemoryUserSessionStore _sessionStore = new InMemoryUserSessionStore();
        FakeTimeProvider _clock = new(DateTime.UtcNow);

        public CookieSlidingTests()
        {
            BffHost.OnConfigureServices += services => 
            {
                services.AddSingleton<IUserSessionStore>(_sessionStore);
                services.Configure<CookieAuthenticationOptions>("cookie", options => 
                {
                    options.SlidingExpiration = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
                });
                services.AddSingleton<TimeProvider>(_clock);
            };
            BffHost.InitializeAsync().Wait();
        }

        private void SetClock(TimeSpan t)
        {
            _clock.SetUtcNow(_clock.GetUtcNow().Add(t));
        }

        [Fact]
        public async Task user_endpoint_cookie_should_slide()
        {
            await BffHost.BffLoginAsync("alice");

            var sessions = await _sessionStore.GetUserSessionsAsync(new UserSessionsFilter { SubjectId = "alice" });
            sessions.Count().Should().Be(1);

            var session = sessions.Single();

            var ticketStore = BffHost.Resolve<IServerTicketStore>();
            var firstTicket = await ticketStore.RetrieveAsync(session.Key);
            firstTicket.Should().NotBeNull();

            SetClock(TimeSpan.FromMinutes(8));
            (await BffHost.GetIsUserLoggedInAsync()).Should().BeTrue();

            var secondTicket = await ticketStore.RetrieveAsync(session.Key);
            secondTicket.Should().NotBeNull();

            (secondTicket.Properties.IssuedUtc > firstTicket.Properties.IssuedUtc).Should().BeTrue();
            (secondTicket.Properties.ExpiresUtc > firstTicket.Properties.ExpiresUtc).Should().BeTrue();
        }

        [Fact]
        public async Task user_endpoint_when_sliding_flag_is_passed_cookie_should_not_slide()
        {
            await BffHost.BffLoginAsync("alice");

            var sessions = await _sessionStore.GetUserSessionsAsync(new UserSessionsFilter { SubjectId = "alice" });
            sessions.Count().Should().Be(1);

            var session = sessions.Single();

            var ticketStore = BffHost.Resolve<IServerTicketStore>();
            var firstTicket = await ticketStore.RetrieveAsync(session.Key);
            firstTicket.Should().NotBeNull();

            SetClock(TimeSpan.FromMinutes(8));
            (await BffHost.GetIsUserLoggedInAsync("slide=false")).Should().BeTrue();

            var secondTicket = await ticketStore.RetrieveAsync(session.Key);
            secondTicket.Should().NotBeNull();

            (secondTicket.Properties.IssuedUtc == firstTicket.Properties.IssuedUtc).Should().BeTrue();
            (secondTicket.Properties.ExpiresUtc == firstTicket.Properties.ExpiresUtc).Should().BeTrue();
        }

        [Fact]
        public async Task user_endpoint_when_uservalidate_renews_cookie_should_slide()
        {
            var shouldRenew = false;
            BffHost.OnConfigureServices += services =>
            {
                services.Configure<CookieAuthenticationOptions>("cookie", options =>
                {
                    options.Events.OnValidatePrincipal = ctx =>
                    {
                        ctx.ShouldRenew = shouldRenew;
                        return Task.CompletedTask;
                    };
                });
            };
            await BffHost.InitializeAsync();


            await BffHost.BffLoginAsync("alice");

            var sessions = await _sessionStore.GetUserSessionsAsync(new UserSessionsFilter { SubjectId = "alice" });
            sessions.Count().Should().Be(1);

            var session = sessions.Single();

            var ticketStore = BffHost.Resolve<IServerTicketStore>();
            var firstTicket = await ticketStore.RetrieveAsync(session.Key);
            firstTicket.Should().NotBeNull();

            shouldRenew = true;
            SetClock(TimeSpan.FromSeconds(1));
            (await BffHost.GetIsUserLoggedInAsync()).Should().BeTrue();

            var secondTicket = await ticketStore.RetrieveAsync(session.Key);
            secondTicket.Should().NotBeNull();

            (secondTicket.Properties.IssuedUtc > firstTicket.Properties.IssuedUtc).Should().BeTrue();
            (secondTicket.Properties.ExpiresUtc > firstTicket.Properties.ExpiresUtc).Should().BeTrue();
        }

        [Fact]
        public async Task user_endpoint_when_uservalidate_renews_and_sliding_flag_is_passed_cookie_should_not_slide()
        {
            var shouldRenew = false;
            
            BffHost.OnConfigureServices += services =>
            {
                services.Configure<CookieAuthenticationOptions>("cookie", options =>
                {
                    options.Events.OnCheckSlidingExpiration = ctx =>
                    {
                        ctx.ShouldRenew = shouldRenew;
                        return Task.CompletedTask;
                    };
                });
            };
            
            await BffHost.InitializeAsync();

            await BffHost.BffLoginAsync("alice");

            var sessions = await _sessionStore.GetUserSessionsAsync(new UserSessionsFilter { SubjectId = "alice" });
            sessions.Count().Should().Be(1);

            var session = sessions.Single();

            var ticketStore = BffHost.Resolve<IServerTicketStore>();
            var firstTicket = await ticketStore.RetrieveAsync(session.Key);
            firstTicket.Should().NotBeNull();

            shouldRenew = true;
            SetClock(TimeSpan.FromSeconds(1));
            (await BffHost.GetIsUserLoggedInAsync("slide=false")).Should().BeTrue();

            var secondTicket = await ticketStore.RetrieveAsync(session.Key);
            secondTicket.Should().NotBeNull();

            (secondTicket.Properties.IssuedUtc == firstTicket.Properties.IssuedUtc).Should().BeTrue();
            (secondTicket.Properties.ExpiresUtc == firstTicket.Properties.ExpiresUtc).Should().BeTrue();
        }
    }
}
