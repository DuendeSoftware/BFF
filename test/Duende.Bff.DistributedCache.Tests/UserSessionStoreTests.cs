// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Xunit;

namespace Duende.Bff.DistributedCache.Tests
{
    public class UserSessionStoreTests
    {
        private readonly IUserSessionStore _subject;
        private readonly IDistributedCache _cache;

        public UserSessionStoreTests()
        {
            var services = new ServiceCollection();
            services
                .PostConfigureAll<DataProtectionOptions>(x => x.ApplicationDiscriminator = "app")
                .AddDistributedMemoryCache()
                .AddBff()
                .AddDistributedCacheServerSideSessions();
            var provider = services.BuildServiceProvider();

            _subject = provider.GetRequiredService<IUserSessionStore>();
            _cache = provider.GetRequiredService<IDistributedCache>();
        }

        [Fact]
        public async Task CreateUserSessionAsync_should_succeed()
        {
            // arrange
            var session = new UserSession
            {
                Key = "key123",
                SessionId = "sid",
                SubjectId = "sub",
                Created = new DateTime(2020, 3, 1, 9, 12, 33, DateTimeKind.Utc),
                Renewed = new DateTime(2021, 4, 2, 10, 13, 34, DateTimeKind.Utc),
                Expires = new DateTime(2022, 5, 3, 11, 14, 35, DateTimeKind.Utc),
                Ticket = "ticket"
            };

            // act
            await _subject.CreateUserSessionAsync(session);

            // assert
            Assert.NotNull(await _cache.GetStringAsync("app:Key:key123"));
            Assert.Equal("app:Key:key123", await _cache.GetStringAsync("app:Session:sid"));
            Assert.Equal("app:Key:key123", await _cache.GetStringAsync("app:Subject:sub"));
            UserSession? storedSession =
                JsonSerializer.Deserialize<UserSession>(
                    await _cache.GetStringAsync("app:Key:key123"));
            Assert.Equal(storedSession!.Created, session.Created);
            Assert.Equal(storedSession!.Key, session.Key);
            Assert.Equal(storedSession!.SessionId, session.SessionId);
            Assert.Equal(storedSession!.SubjectId, session.SubjectId);
            Assert.Equal(storedSession!.Renewed, session.Renewed);
            Assert.Equal(storedSession!.Expires, session.Expires);
            Assert.Equal(storedSession!.Ticket, session.Ticket);
        }


        [Fact]
        public async Task GetUserSessionAsync_for_valid_key_should_succeed()
        {
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = "key123",
                SessionId = "sid",
                SubjectId = "sub",
                Created = new DateTime(2020, 3, 1, 9, 12, 33, DateTimeKind.Utc),
                Renewed = new DateTime(2021, 4, 2, 10, 13, 34, DateTimeKind.Utc),
                Expires = new DateTime(2022, 5, 3, 11, 14, 35, DateTimeKind.Utc),
                Ticket = "ticket"
            });

            var item = await _subject.GetUserSessionAsync("key123");

            item.Should().NotBeNull();
            item.Key.Should().Be("key123");
            item.SubjectId.Should().Be("sub");
            item.SessionId.Should().Be("sid");
            item.Ticket.Should().Be("ticket");
            item.Created.Should().Be(new DateTime(2020, 3, 1, 9, 12, 33, DateTimeKind.Utc));
            item.Renewed.Should().Be(new DateTime(2021, 4, 2, 10, 13, 34, DateTimeKind.Utc));
            item.Expires.Should().Be(new DateTime(2022, 5, 3, 11, 14, 35, DateTimeKind.Utc));
        }

        [Fact]
        public async Task GetUserSessionAsync_for_invalid_key_should_return_null()
        {
            var item = await _subject.GetUserSessionAsync("invalid");
            item.Should().BeNull();
        }

        [Fact]
        public async Task UpdateUserSessionAsync_should_succeed()
        {
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = "key123",
                SessionId = "sid",
                SubjectId = "sub",
                Created = new DateTime(2020, 3, 1, 9, 12, 33, DateTimeKind.Utc),
                Renewed = new DateTime(2021, 4, 2, 10, 13, 34, DateTimeKind.Utc),
                Expires = new DateTime(2022, 5, 3, 11, 14, 35, DateTimeKind.Utc),
                Ticket = "ticket"
            });

            await _subject.UpdateUserSessionAsync("key123", new UserSessionUpdate {
                Ticket = "ticket2",
                Renewed = new DateTime(2024, 1, 3, 5, 7, 9, DateTimeKind.Utc),
                Expires = new DateTime(2025, 2, 4, 6, 8, 10, DateTimeKind.Utc)
            });

            var item = await _subject.GetUserSessionAsync("key123");
            item.Should().NotBeNull();
            item.Key.Should().Be("key123");
            item.SubjectId.Should().Be("sub");
            item.SessionId.Should().Be("sid");
            item.Ticket.Should().Be("ticket2");
            item.Created.Should().Be(new DateTime(2020, 3, 1, 9, 12, 33, DateTimeKind.Utc));
            item.Renewed.Should().Be(new DateTime(2024, 1, 3, 5, 7, 9, DateTimeKind.Utc));
            item.Expires.Should().Be(new DateTime(2025, 2, 4, 6, 8, 10, DateTimeKind.Utc));
        }

        [Fact]
        public async Task UpdateUserSessionAsync_for_invalid_key_should_succeed()
        {
            await _subject.UpdateUserSessionAsync("key123", new UserSessionUpdate
            {
                Ticket = "ticket2",
                Renewed = new DateTime(2024, 1, 3, 5, 7, 9, DateTimeKind.Utc),
                Expires = new DateTime(2025, 2, 4, 6, 8, 10, DateTimeKind.Utc)
            });

            var item = await _subject.GetUserSessionAsync("key123");
            item.Should().BeNull();
        }

        [Fact]
        public async Task DeleteUserSessionAsync_for_valid_key_should_succeed()
        {
            await _subject.CreateUserSessionAsync(new UserSession {
                Key = "key123"
            });

            Assert.NotNull(await _cache.GetStringAsync("app:Key:key123"));

            await _subject.DeleteUserSessionAsync("key123");

            Assert.Null(await _cache.GetStringAsync("app:Key:key123"));
        }

        [Fact]
        public async Task DeleteUserSessionAsync_for_invalid_key_should_succeed()
        {
            await _subject.DeleteUserSessionAsync("invalid");
        }

        [Fact]
        public async Task GetUserSessionsAsync_for_valid_sub_should_succeed()
        {
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub1",
                SessionId = "sid1_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub1",
                SessionId = "sid1_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_3",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub3",
                SessionId = "sid3_1",
            });

            var items = await _subject.GetUserSessionsAsync(new UserSessionsFilter { SubjectId = "sub2" });
            items.Count().Should().Be(3);
            items.Select(x => x.SubjectId).Distinct().Should().BeEquivalentTo(new[] { "sub2" });
            items.Select(x => x.SessionId).Should().BeEquivalentTo(new[] { "sid2_1", "sid2_2", "sid2_3", });
        }

        [Fact]
        public async Task GetUserSessionsAsync_for_invalid_sub_should_return_empty()
        {
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub1",
                SessionId = "sid1_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub1",
                SessionId = "sid1_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_3",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub3",
                SessionId = "sid3_1",
            });

            var items = await _subject.GetUserSessionsAsync(new UserSessionsFilter { SubjectId = "invalid" });
            items.Count().Should().Be(0);
        }

        [Fact]
        public async Task GetUserSessionsAsync_for_valid_sid_should_succeed()
        {
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub1",
                SessionId = "sid1_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub1",
                SessionId = "sid1_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_3",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub3",
                SessionId = "sid3_1",
            });

            var items = await _subject.GetUserSessionsAsync(new UserSessionsFilter { SessionId = "sid2_2" });
            items.Count().Should().Be(1);
            items.Select(x => x.SubjectId).Should().BeEquivalentTo(new[] { "sub2" });
            items.Select(x => x.SessionId).Should().BeEquivalentTo(new[] { "sid2_2" });
        }

        [Fact]
        public async Task GetUserSessionsAsync_for_invalid_sid_should_return_empty()
        {
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub1",
                SessionId = "sid1_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub1",
                SessionId = "sid1_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_3",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub3",
                SessionId = "sid3_1",
            });

            var items = await _subject.GetUserSessionsAsync(new UserSessionsFilter { SessionId = "invalid" });
            items.Count().Should().Be(0);
        }

        [Fact]
        public async Task GetUserSessionsAsync_for_valid_sub_and_sid_should_succeed()
        {
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub1",
                SessionId = "sid1_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub1",
                SessionId = "sid1_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_3",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub3",
                SessionId = "sid3_1",
            });

            var items = await _subject.GetUserSessionsAsync(new UserSessionsFilter { SubjectId = "sub2", SessionId = "sid2_2" });
            items.Count().Should().Be(1);
            items.Select(x => x.SubjectId).Should().BeEquivalentTo(new[] { "sub2" });
            items.Select(x => x.SessionId).Should().BeEquivalentTo(new[] { "sid2_2" });
        }

        [Fact]
        public async Task GetUserSessionsAsync_for_invalid_sub_and_sid_should_succeed()
        {
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub1",
                SessionId = "sid1_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub1",
                SessionId = "sid1_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_3",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub3",
                SessionId = "sid3_1",
            });

            {
                var items = await _subject.GetUserSessionsAsync(new UserSessionsFilter { SubjectId = "invalid", SessionId = "invalid" });
                items.Count().Should().Be(0);
            }
            {
                var items = await _subject.GetUserSessionsAsync(new UserSessionsFilter { SubjectId = "sub1", SessionId = "invalid" });
                items.Count().Should().Be(0);
            }
            {
                var items = await _subject.GetUserSessionsAsync(new UserSessionsFilter { SubjectId = "invalid", SessionId = "sid1_1" });
                items.Count().Should().Be(0);
            }
        }

        [Fact]
        public void GetUserSessionsAsync_for_missing_sub_and_sid_should_throw()
        {
            Func<Task> f = () => _subject.GetUserSessionsAsync(new UserSessionsFilter());
            f.Should().Throw<Exception>();
        }

        [Fact]
        public async Task DeleteUserSessionsAsync_for_valid_sub_should_succeed()
        {
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub1",
                SessionId = "sid1_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub1",
                SessionId = "sid1_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_3",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub3",
                SessionId = "sid3_1",
            });

            await _subject.DeleteUserSessionsAsync(new UserSessionsFilter { SubjectId = "sub2" });
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid1_1"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid1_2"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid3_1"));
            Assert.Null(await _cache.GetStringAsync("app:Session:sid2_1"));
            Assert.Null(await _cache.GetStringAsync("app:Session:sid2_2"));
            Assert.Null(await _cache.GetStringAsync("app:Session:sid2_3"));
        }

        [Fact]
        public async Task DeleteUserSessionsAsync_for_invalid_sub_should_do_nothing()
        {
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub1",
                SessionId = "sid1_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub1",
                SessionId = "sid1_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_3",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub3",
                SessionId = "sid3_1",
            });

            await _subject.DeleteUserSessionsAsync(new UserSessionsFilter { SubjectId = "invalid" });
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid1_1"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid1_2"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid3_1"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid2_1"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid2_2"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid2_3"));
        }

        [Fact]
        public async Task DeleteUserSessionsAsync_for_valid_sid_should_succeed()
        {
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub1",
                SessionId = "sid1_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub1",
                SessionId = "sid1_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_3",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub3",
                SessionId = "sid3_1",
            });

            await _subject.DeleteUserSessionsAsync(new UserSessionsFilter { SessionId = "sid2_2" });
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid1_1"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid1_2"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid3_1"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid2_1"));
            Assert.Null(await _cache.GetStringAsync("app:Session:sid2_2"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid2_3"));
        }

        [Fact]
        public async Task DeleteUserSessionsAsync_for_invalid_sid_should_do_nothing()
        {
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub1",
                SessionId = "sid1_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub1",
                SessionId = "sid1_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_3",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub3",
                SessionId = "sid3_1",
            });

            await _subject.DeleteUserSessionsAsync(new UserSessionsFilter { SessionId = "invalid" });
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid1_1"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid1_2"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid3_1"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid2_1"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid2_2"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid2_3"));
        }

        [Fact]
        public async Task DeleteUserSessionsAsync_for_valid_sub_and_sid_should_succeed()
        {
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub1",
                SessionId = "sid1_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub1",
                SessionId = "sid1_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_3",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub3",
                SessionId = "sid3_1",
            });

            await _subject.DeleteUserSessionsAsync(new UserSessionsFilter { SubjectId = "sub2", SessionId = "sid2_2" });
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid1_1"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid1_2"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid3_1"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid2_1"));
            Assert.Null(await _cache.GetStringAsync("app:Session:sid2_2"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid2_3"));
        }

        [Fact]
        public async Task DeleteUserSessionsAsync_for_invalid_sub_and_sid_should_succeed()
        {
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub1",
                SessionId = "sid1_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub1",
                SessionId = "sid1_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub2",
                SessionId = "sid2_3",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = Guid.NewGuid().ToString(),
                SubjectId = "sub3",
                SessionId = "sid3_1",
            });

            await _subject.DeleteUserSessionsAsync(new UserSessionsFilter { SubjectId = "invalid", SessionId = "invalid" });

            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid1_1"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid1_2"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid3_1"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid2_1"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid2_2"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid2_3"));

            await _subject.DeleteUserSessionsAsync(new UserSessionsFilter { SubjectId = "sub1", SessionId = "invalid" });
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid1_1"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid1_2"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid3_1"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid2_1"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid2_2"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid2_3"));
            await _subject.DeleteUserSessionsAsync(new UserSessionsFilter { SubjectId = "invalid", SessionId = "sid1_1" });
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid1_1"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid1_2"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid3_1"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid2_1"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid2_2"));
            Assert.NotNull(await _cache.GetStringAsync("app:Session:sid2_3"));
        }

        [Fact]
        public void DeleteUserSessionsAsync_for_missing_sub_and_sid_should_throw()
        {
            Func<Task> f = () => _subject.DeleteUserSessionsAsync(new UserSessionsFilter());
            f.Should().Throw<Exception>();
        }
    }
}