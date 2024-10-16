// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;
using Microsoft.Extensions.Logging;

namespace Duende.Bff.Logging;

internal static class LogCategories
{
    public const string ManagementEndpoints = "Duende.Bff.ManagementEndpoints";
    public const string RemoteApiEndpoints = "Duende.Bff.RemoteApiEndpoints";
}
    
internal static class EventIds
{
    public static readonly EventId AntiForgeryValidationFailed = new (1, "AntiForgeryValidationFailed");
    public static readonly EventId BackChannelLogout = new (2, "BackChannelLogout");
    public static readonly EventId BackChannelLogoutError = new (3, "BackChannelLogoutError");
    public static readonly EventId AccessTokenMissing = new (4, "AccessTokenMissing");
    public static readonly EventId InvalidRouteConfiguration = new (5, "InvalidRouteConfiguration");
}
    
internal static class Log
{
    private static readonly Action<ILogger, string, Exception?> _antiForgeryValidationFailed = LoggerMessage.Define<string>(
        LogLevel.Error,
        EventIds.AntiForgeryValidationFailed,
        "Anti-forgery validation failed. local path: '{localPath}'");
        
    private static readonly Action<ILogger, string, string, Exception?> _backChannelLogout = LoggerMessage.Define<string, string>(
        LogLevel.Information,
        EventIds.BackChannelLogout,
        "Back-channel logout. sub: '{sub}', sid: '{sid}'");
        
    private static readonly Action<ILogger, string, Exception?> _backChannelLogoutError = LoggerMessage.Define<string>(
        LogLevel.Information,
        EventIds.BackChannelLogoutError,
        "Back-channel logout error. error: '{error}'");

    private static readonly Action<ILogger, string, string, string, Exception?> _accessTokenMissing = LoggerMessage.Define<string, string, string>(
        LogLevel.Warning,
        EventIds.AccessTokenMissing,
        "Access token is missing. token type: '{tokenType}', local path: '{localpath}', detail: '{detail}'");

    private static readonly Action<ILogger, string, string, Exception?> _invalidRouteConfiguration = LoggerMessage.Define<string, string>(
        LogLevel.Warning,
        EventIds.InvalidRouteConfiguration,
        "Invalid route configuration. Cannot combine a required access token (a call to WithAccessToken) and an optional access token (a call to WithOptionalUserAccessToken). clusterId: '{clusterId}', routeId: '{routeId}'");

    public static void AntiForgeryValidationFailed(this ILogger logger, string localPath)
    {
        _antiForgeryValidationFailed(logger, localPath, null);
    }
        
    public static void BackChannelLogout(this ILogger logger, string sub, string sid)
    { 
        _backChannelLogout(logger, sub, sid, null);
    }
        
    public static void BackChannelLogoutError(this ILogger logger, string error)
    { 
        _backChannelLogoutError(logger, error, null);
    }

    public static void AccessTokenMissing(this ILogger logger, string tokenType, string localPath, string detail)
    {
        _accessTokenMissing(logger, tokenType, localPath, detail, null);
    }

    public static void InvalidRouteConfiguration(this ILogger logger, string? clusterId, string routeId)
    {
        _invalidRouteConfiguration(logger, clusterId ?? "no cluster id", routeId, null);
    }
}