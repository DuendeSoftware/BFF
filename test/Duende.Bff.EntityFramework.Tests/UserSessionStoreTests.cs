// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Duende.Bff.EntityFramework.Tests
{
    public class UserSessionStoreTests
    {
        private readonly IUserSessionStore _subject;
        private readonly SessionDbContext _database;

        public UserSessionStoreTests()
        {
            var services = new ServiceCollection();
            services.AddBff()
                .AddEntityFrameworkServerSideSessions(options=> options.UseInMemoryDatabase(Guid.NewGuid().ToString()));
            var provider = services.BuildServiceProvider();
            
            _subject = provider.GetRequiredService<IUserSessionStore>();
            _database = provider.GetRequiredService<SessionDbContext>();
        }

        [Fact]
        public async Task CreateUserSessionAsync_should_succeed()
        {
            _database.UserSessions.Count().Should().Be(0);

            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = "key123",
                Scheme = "scheme",
                SessionId = "sid",
                SubjectId = "sub",
                Created = new DateTime(2020, 3, 1, 9, 12, 33, DateTimeKind.Utc),
                Renewed = new DateTime(2021, 4, 2, 10, 13, 34, DateTimeKind.Utc),
                Expires = new DateTime(2022, 5, 3, 11, 14, 35, DateTimeKind.Utc),
                Ticket = "ticket"
            });

            _database.UserSessions.Count().Should().Be(1);
        }


        [Fact]
        public async Task GetUserSessionAsync_for_valid_key_should_succeed()
        {
            await _subject.CreateUserSessionAsync(new UserSession
            {
                Key = "key123",
                Scheme = "scheme",
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
            item.Scheme.Should().Be("scheme");
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
                Scheme = "scheme",
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
            item.Scheme.Should().Be("scheme");
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
            _database.UserSessions.Count().Should().Be(1);

            await _subject.DeleteUserSessionAsync("key123");
            
            _database.UserSessions.Count().Should().Be(0);
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
                SubjectId = "sub1",
                SessionId = "sid1_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub1",
                SessionId = "sid1_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_3",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
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
                SubjectId = "sub1",
                SessionId = "sid1_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub1",
                SessionId = "sid1_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_3",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
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
                SubjectId = "sub1",
                SessionId = "sid1_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub1",
                SessionId = "sid1_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_3",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
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
                SubjectId = "sub1",
                SessionId = "sid1_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub1",
                SessionId = "sid1_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_3",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
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
                SubjectId = "sub1",
                SessionId = "sid1_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub1",
                SessionId = "sid1_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_3",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
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
                SubjectId = "sub1",
                SessionId = "sid1_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub1",
                SessionId = "sid1_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_3",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
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
                SubjectId = "sub1",
                SessionId = "sid1_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub1",
                SessionId = "sid1_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_3",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub3",
                SessionId = "sid3_1",
            });

            await _subject.DeleteUserSessionsAsync(new UserSessionsFilter { SubjectId = "sub2" });
            _database.UserSessions.Count().Should().Be(3);
            _database.UserSessions.Count(x => x.SubjectId == "sub2").Should().Be(0);
        }
        [Fact]
        public async Task DeleteUserSessionsAsync_for_invalid_sub_should_do_nothing()
        {
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub1",
                SessionId = "sid1_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub1",
                SessionId = "sid1_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_3",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub3",
                SessionId = "sid3_1",
            });

            await _subject.DeleteUserSessionsAsync(new UserSessionsFilter { SubjectId = "invalid" });
            _database.UserSessions.Count().Should().Be(6);
        }
        [Fact]
        public async Task DeleteUserSessionsAsync_for_valid_sid_should_succeed()
        {
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub1",
                SessionId = "sid1_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub1",
                SessionId = "sid1_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_3",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub3",
                SessionId = "sid3_1",
            });

            await _subject.DeleteUserSessionsAsync(new UserSessionsFilter { SessionId = "sid2_2" });
            _database.UserSessions.Count().Should().Be(5);
            _database.UserSessions.Count(x => x.SessionId == "sid2_2").Should().Be(0);
        }
        [Fact]
        public async Task DeleteUserSessionsAsync_for_invalid_sid_should_do_nothing()
        {
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub1",
                SessionId = "sid1_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub1",
                SessionId = "sid1_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_3",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub3",
                SessionId = "sid3_1",
            });

            await _subject.DeleteUserSessionsAsync(new UserSessionsFilter { SessionId = "invalid" });
            _database.UserSessions.Count().Should().Be(6);
        }
        [Fact]
        public async Task DeleteUserSessionsAsync_for_valid_sub_and_sid_should_succeed()
        {
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub1",
                SessionId = "sid1_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub1",
                SessionId = "sid1_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_3",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub3",
                SessionId = "sid3_1",
            });

            await _subject.DeleteUserSessionsAsync(new UserSessionsFilter { SubjectId = "sub2", SessionId = "sid2_2" });
            _database.UserSessions.Count().Should().Be(5);
            _database.UserSessions.Count(x => x.SubjectId == "sub2" && x.SessionId == "sid2_2").Should().Be(0);
        }
        [Fact]
        public async Task DeleteUserSessionsAsync_for_invalid_sub_and_sid_should_succeed()
        {
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub1",
                SessionId = "sid1_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub1",
                SessionId = "sid1_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_1",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_2",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub2",
                SessionId = "sid2_3",
            });
            await _subject.CreateUserSessionAsync(new UserSession
            {
                SubjectId = "sub3",
                SessionId = "sid3_1",
            });

            {
                await _subject.DeleteUserSessionsAsync(new UserSessionsFilter { SubjectId = "invalid", SessionId = "invalid" });
                _database.UserSessions.Count().Should().Be(6);
            }
            {
                await _subject.DeleteUserSessionsAsync(new UserSessionsFilter { SubjectId = "sub1", SessionId = "invalid" });
                _database.UserSessions.Count().Should().Be(6);
            }
            {
                await _subject.DeleteUserSessionsAsync(new UserSessionsFilter { SubjectId = "invalid", SessionId = "sid1_1" });
                _database.UserSessions.Count().Should().Be(6);
            }
        }
        [Fact]
        public void DeleteUserSessionsAsync_for_missing_sub_and_sid_should_throw()
        {
            Func<Task> f = () => _subject.DeleteUserSessionsAsync(new UserSessionsFilter());
            f.Should().Throw<Exception>();
        }
    }
}
