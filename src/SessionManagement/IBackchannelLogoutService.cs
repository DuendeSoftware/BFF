// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Duende.Bff
{
    /// <summary>
    /// Service for handling back-channel logout notifications
    /// </summary>
    public interface IBackchannelLogoutService
    {
        /// <summary>
        /// Process the back-channel logout notification
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task ProcessRequequestAsync(HttpContext context);
    }
}