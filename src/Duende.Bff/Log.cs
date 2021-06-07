// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;
using Microsoft.Extensions.Logging;

namespace Duende.Bff
{
    internal static class LogCategories
    {
        public const string ManagementEndpoints = "Duende.Bff.ManagementEndpoints";
        public const string RemoteApiEndpoints = "Duende.Bff.RemoteApiEndpoints";
    }
    
    internal static class EventIds
    {
        public static readonly EventId AccessTokenMissing = new (1, "AccessTokenMissing");
        public static readonly EventId AntiForgeryValidationFailed = new (2, "AntiForgeryValidationFailed");
        public static readonly EventId ProxyError = new (3, "ProxyError");
        public static readonly EventId BackChannelLogout = new (4, "BackChannelLogout");
        public static readonly EventId BackChannelLogoutError = new (5, "BackChannelLogoutError");
    }
    
    internal static class Log
    {
        private static readonly Action<ILogger, string, string, Exception> _accessTokenMissing = LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            EventIds.AccessTokenMissing,
            "Access token is missing. token type: '{tokenType}', local path: '{localpath}'.");

        private static readonly Action<ILogger, string, Exception> _antiForgeryValidationFailed = LoggerMessage.Define<string>(
            LogLevel.Error,
            EventIds.AntiForgeryValidationFailed,
            "Anti-forgery validation failed. local path: '{localPath}'");
        
        private static readonly Action<ILogger, string, string, Exception> _proxyResponseError = LoggerMessage.Define<string, string>(
            LogLevel.Information,
            EventIds.ProxyError,
            "Proxy response error. local path: '{localPath}', error: '{error}'");
        
        private static readonly Action<ILogger, string, string, Exception> _backChannelLogout = LoggerMessage.Define<string, string>(
            LogLevel.Information,
            EventIds.BackChannelLogout,
            "Back-channel logout. sub: '{sub}', sid: '{sid}'");
        
        private static readonly Action<ILogger, string, Exception> _backChannelLogoutError = LoggerMessage.Define<string>(
            LogLevel.Information,
            EventIds.BackChannelLogoutError,
            "Back-channel logout error. error: '{error}'");

        public static void AccessTokenMissing(this ILogger logger, string localPath, TokenType tokenType)
        {
            _accessTokenMissing(logger, tokenType.ToString(), localPath, null);
        }

        public static void AntiForgeryValidationFailed(this ILogger logger, string localPath)
        {
            _antiForgeryValidationFailed(logger, localPath, null);
        }
        
        public static void ProxyResponseError(this ILogger logger, string localPath, string error)
        { 
            _proxyResponseError(logger, localPath, error, null);
        }
        
        public static void BackChannelLogout(this ILogger logger, string sub, string sid)
        { 
            _backChannelLogout(logger, sub, sid, null);
        }
        
        public static void BackChannelLogoutError(this ILogger logger, string error)
        { 
            _backChannelLogoutError(logger, error, null);
        }
    }
}