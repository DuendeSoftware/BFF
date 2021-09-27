// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;
using Microsoft.Extensions.Logging;

namespace Duende.Bff.Yarp.Logging
{
    internal static class EventIds
    {
        public static readonly EventId AccessTokenMissing = new (4, "AccessTokenMissing");
        public static readonly EventId ProxyError = new (5, "ProxyError");
    }
    
    internal static class Log
    {
        private static readonly Action<ILogger, string, string, Exception> _accessTokenMissing = LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            EventIds.AccessTokenMissing,
            "Access token is missing. token type: '{tokenType}', local path: '{localpath}'.");

        private static readonly Action<ILogger, string, string, Exception> _proxyResponseError = LoggerMessage.Define<string, string>(
            LogLevel.Information,
            EventIds.ProxyError,
            "Proxy response error. local path: '{localPath}', error: '{error}'");
        
        public static void AccessTokenMissing(this ILogger logger, string localPath, TokenType tokenType)
        {
            _accessTokenMissing(logger, tokenType.ToString(), localPath, null);
        }
        
        public static void ProxyResponseError(this ILogger logger, string localPath, string error)
        { 
            _proxyResponseError(logger, localPath, error, null);
        }
    }
}