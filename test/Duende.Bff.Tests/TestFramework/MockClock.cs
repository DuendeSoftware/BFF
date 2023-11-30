// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authentication;
using System;

#if !NET8_0
namespace Duende.Bff.Tests.TestFramework
{
    public class MockClock : ISystemClock
    {
        public DateTimeOffset UtcNow { get; set; }
    }
}
#endif