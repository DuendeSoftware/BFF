using System;
using Duende.Bff;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Builder
{
    internal static class Log
    {
        private static readonly Action<ILogger, string, string, Exception> _accessTokenMissing = LoggerMessage.Define<string, string>(
            LogLevel.Warning,
            EventIds.AccessTokenMissing,
            "Access token is missing. token type: '{tokenType}', local path: '{localpath}'.");

        private static readonly Action<ILogger, string, Exception> _antiforgeryValidationFailed = LoggerMessage.Define<string>(
            LogLevel.Error,
            EventIds.AntiforgeryValidationFailed,
            "Antiforgery validation failed. local path: '{localPath}'");
        
        private static readonly Action<ILogger, string, string, Exception> _proxyResponseError = LoggerMessage.Define<string, string>(
            LogLevel.Error,
            EventIds.ProxyResponseError,
            "Proxy response error. local path: '{localPath}', error: '{error}'");

        public static void AccessTokenMissing(this ILogger logger, string localPath, TokenType tokenType)
        {
            _accessTokenMissing(logger, tokenType.ToString(), localPath, null);
        }

        public static void AntiforgeryValidationFailed(this ILogger logger, string localPath)
        {
            _antiforgeryValidationFailed(logger, localPath, null);
        }
        
        public static void ProxyResponseError(this ILogger logger, string localPath, string error)
        { 
            _proxyResponseError(logger, localPath, error, null);
        }
    }
}