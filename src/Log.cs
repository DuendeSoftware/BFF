using System;
using Microsoft.Extensions.Logging;

namespace Duende.Bff
{
    internal static class EventIds
    {
        public static readonly EventId AccessTokenMissing = new EventId(1, "AccessTokenMissing");
        public static readonly EventId AntiforgeryValidationFailed = new EventId(2, "AntiforgeryValidationFailed");
        public static readonly EventId ProxyError = new EventId(3, "ProxyError");
    }
    
    internal static class Log
    {
        private static readonly Action<ILogger, string, string, Exception> _accessTokenMissing = LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            EventIds.AccessTokenMissing,
            "Access token is missing. token type: '{tokenType}', local path: '{localpath}'.");

        private static readonly Action<ILogger, string, Exception> _antiForgeryValidationFailed = LoggerMessage.Define<string>(
            LogLevel.Error,
            EventIds.AntiforgeryValidationFailed,
            "Anti-forgery validation failed. local path: '{localPath}'");
        
        private static readonly Action<ILogger, string, string, Exception> _proxyResponseError = LoggerMessage.Define<string, string>(
            LogLevel.Information,
            EventIds.ProxyError,
            "Proxy response error. local path: '{localPath}', error: '{error}'");

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
    }
}