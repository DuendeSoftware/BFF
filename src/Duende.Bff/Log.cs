// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;
using Microsoft.Extensions.Logging;

namespace Duende.Bff.Logging
{
    public static class LogCategories
    {
        public const string ManagementEndpoints = "Duende.Bff.ManagementEndpoints";
        public const string RemoteApiEndpoints = "Duende.Bff.RemoteApiEndpoints";
    }
    
    internal static class EventIds
    {
        public static readonly EventId AntiForgeryValidationFailed = new (1, "AntiForgeryValidationFailed");
        public static readonly EventId BackChannelLogout = new (2, "BackChannelLogout");
        public static readonly EventId BackChannelLogoutError = new (3, "BackChannelLogoutError");
    }
    
    internal static class Log
    {
        private static readonly Action<ILogger, string, Exception> _antiForgeryValidationFailed = LoggerMessage.Define<string>(
            LogLevel.Error,
            EventIds.AntiForgeryValidationFailed,
            "Anti-forgery validation failed. local path: '{localPath}'");
        
        private static readonly Action<ILogger, string, string, Exception> _backChannelLogout = LoggerMessage.Define<string, string>(
            LogLevel.Information,
            EventIds.BackChannelLogout,
            "Back-channel logout. sub: '{sub}', sid: '{sid}'");
        
        private static readonly Action<ILogger, string, Exception> _backChannelLogoutError = LoggerMessage.Define<string>(
            LogLevel.Information,
            EventIds.BackChannelLogoutError,
            "Back-channel logout error. error: '{error}'");

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
    }
}