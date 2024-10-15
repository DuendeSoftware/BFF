// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;

namespace Duende.Bff;

/// <summary>
/// Interface for a service that retrieves user and management claims.
/// </summary>
public interface IClaimsService
{
    /// <summary>
    /// Gets claims associated with the user's session.
    /// </summary>
    /// <param name="principal"></param>
    /// <param name="properties"></param>
    /// <returns></returns>
    Task<IEnumerable<ClaimRecord>> GetUserClaimsAsync(ClaimsPrincipal? principal, AuthenticationProperties? properties);
    
    /// <summary>
    /// Gets claims that facilitate session and token management.
    /// </summary>
    /// <param name="pathBase"></param>
    /// <param name="principal"></param>
    /// <param name="properties"></param>
    /// <returns></returns>
    Task<IEnumerable<ClaimRecord>> GetManagementClaimsAsync(PathString pathBase, ClaimsPrincipal? principal, AuthenticationProperties? properties);
}
