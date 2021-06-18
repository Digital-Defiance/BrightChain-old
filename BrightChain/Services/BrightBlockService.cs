using BrightChain.Exceptions;
using BrightChain.Interfaces;
using BrightChain.Models.Blocks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace BrightChain.Services
{
    public class BrightBlockService : ILogger
    {
        protected ILogger logger;
        protected IConfiguration configuration;

        protected ICacheManager<BlockHash, Block> blockMemoryCache;
        protected ICacheManager<BlockHash, Block> blockDiskCache;

        public BrightBlockService(ILoggerFactory logger, IConfiguration configuration)
        {
            this.logger = logger.CreateLogger(nameof(BrightBlockService));
            if (this.logger is null)
                throw new BrightChainException("CreateLogger failed");
            this.logger.LogInformation(String.Format("<%s>: logging initialized", nameof(BrightBlockService)));
            this.configuration = configuration;

            this.blockMemoryCache = new MemoryCacheManager<BlockHash, Block>(logger: this.logger);
            this.blockDiskCache = new DiskBlockCacheManager(logger: this.logger);
            this.logger.LogInformation(String.Format("<%s>: caches initialized", nameof(BrightBlockService)));
        }

        public IDisposable BeginScope<TState>(TState state) =>
            logger.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel) =>
            logger.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) =>
            logger.Log(logLevel, eventId, state, exception, formatter);

        public IDisposable BeginScope(string messageFormat, params object?[] args)
            => this.logger.BeginScope(messageFormat, args);

        public void LogCritical(EventId eventId, Exception? exception, string? message, params object?[] args) =>
            this.logger.LogCritical(eventId, exception, message, args);
        //
        // Summary:
        //     Formats and writes a critical log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   eventId:
        //     The event id associated with the log.
        //
        //   message:
        //     Format string of the log message in message template format. Example: "User {User}
        //     logged in from {Address}"
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogCritical(EventId eventId, string? message, params object?[] args) =>
            this.logger.LogCritical(eventId, message, args);
        //
        // Summary:
        //     Formats and writes a critical log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   exception:
        //     The exception to log.
        //
        //   message:
        //     Format string of the log message in message template format. Example: "User {User}
        //     logged in from {Address}"
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogCritical(Exception? exception, string? message, params object?[] args) =>
            this.logger.LogCritical(exception, message, args);
        //
        // Summary:
        //     Formats and writes a critical log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   message:
        //     Format string of the log message in message template format. Example: "User {User}
        //     logged in from {Address}"
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogCritical(string? message, params object?[] args) =>
            this.logger.LogCritical(message, args);
        //
        // Summary:
        //     Formats and writes a debug log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   eventId:
        //     The event id associated with the log.
        //
        //   exception:
        //     The exception to log.
        //
        //   message:
        //     Format string of the log message in message template format. Example: "User {User}
        //     logged in from {Address}"
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogDebug(EventId eventId, Exception? exception, string? message, params object?[] args) =>
            this.logger.LogDebug(eventId, exception, message, args);
        //
        // Summary:
        //     Formats and writes a debug log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   eventId:
        //     The event id associated with the log.
        //
        //   message:
        //     Format string of the log message in message template format. Example: "User {User}
        //     logged in from {Address}"
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogDebug(EventId eventId, string? message, params object?[] args) =>
            this.logger.LogDebug(eventId, message, args);
        //
        // Summary:
        //     Formats and writes a debug log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   exception:
        //     The exception to log.
        //
        //   message:
        //     Format string of the log message in message template format. Example: "User {User}
        //     logged in from {Address}"
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogDebug(Exception? exception, string? message, params object?[] args) =>
            this.logger.LogDebug(exception, message, args);
        //
        // Summary:
        //     Formats and writes a debug log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   message:
        //     Format string of the log message in message template format. Example: "User {User}
        //     logged in from {Address}"
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogDebug(string? message, params object?[] args) =>
            this.logger.LogDebug(message, args);
        //
        // Summary:
        //     Formats and writes an error log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   message:
        //     Format string of the log message in message template format. Example: "User {User}
        //     logged in from {Address}"
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogError(string? message, params object?[] args) =>
            this.logger.LogError(message, args);
        //
        // Summary:
        //     Formats and writes an error log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   exception:
        //     The exception to log.
        //
        //   message:
        //     Format string of the log message in message template format. Example: "User {User}
        //     logged in from {Address}"
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogError(Exception? exception, string? message, params object?[] args) =>
            this.logger.LogError(exception, message, args);
        //
        // Summary:
        //     Formats and writes an error log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   eventId:
        //     The event id associated with the log.
        //
        //   message:
        //     Format string of the log message in message template format. Example: "User {User}
        //     logged in from {Address}"
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogError(EventId eventId, string? message, params object?[] args) =>
            this.logger.LogError(eventId, message, args);
        //
        // Summary:
        //     Formats and writes an error log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   eventId:
        //     The event id associated with the log.
        //
        //   exception:
        //     The exception to log.
        //
        //   message:
        //     Format string of the log message in message template format. Example: "User {User}
        //     logged in from {Address}"
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogError(EventId eventId, Exception? exception, string? message, params object?[] args) =>
            this.logger.LogError(eventId, exception, message, args);
        //
        // Summary:
        //     Formats and writes an informational log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   eventId:
        //     The event id associated with the log.
        //
        //   exception:
        //     The exception to log.
        //
        //   message:
        //     Format string of the log message in message template format. Example: "User {User}
        //     logged in from {Address}"
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogInformation(EventId eventId, Exception? exception, string? message, params object?[] args) =>
            this.logger.LogInformation(eventId, exception, message, args);
        //
        // Summary:
        //     Formats and writes an informational log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   eventId:
        //     The event id associated with the log.
        //
        //   message:
        //     Format string of the log message in message template format. Example: "User {User}
        //     logged in from {Address}"
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogInformation(EventId eventId, string? message, params object?[] args) =>
            this.logger.LogInformation(eventId, message, args);
        //
        // Summary:
        //     Formats and writes an informational log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   exception:
        //     The exception to log.
        //
        //   message:
        //     Format string of the log message in message template format. Example: "User {User}
        //     logged in from {Address}"
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogInformation(Exception? exception, string? message, params object?[] args) =>
            this.logger.LogInformation(exception, message, args);
        //
        // Summary:
        //     Formats and writes an informational log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   message:
        //     Format string of the log message in message template format. Example: "User {User}
        //     logged in from {Address}"
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogInformation(string? message, params object?[] args) =>
            this.logger.LogInformation(message, args);
        //
        // Summary:
        //     Formats and writes a trace log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   eventId:
        //     The event id associated with the log.
        //
        //   exception:
        //     The exception to log.
        //
        //   message:
        //     Format string of the log message in message template format. Example: "User {User}
        //     logged in from {Address}"
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogTrace(EventId eventId, Exception? exception, string? message, params object?[] args) =>
            this.logger.LogTrace(eventId, exception, message, args);
        //
        // Summary:
        //     Formats and writes a trace log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   eventId:
        //     The event id associated with the log.
        //
        //   message:
        //     Format string of the log message in message template format. Example: "User {User}
        //     logged in from {Address}"
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogTrace(EventId eventId, string? message, params object?[] args) =>
            this.logger.LogTrace(eventId, message, args);
        //
        // Summary:
        //     Formats and writes a trace log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   exception:
        //     The exception to log.
        //
        //   message:
        //     Format string of the log message in message template format. Example: "User {User}
        //     logged in from {Address}"
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogTrace(Exception? exception, string? message, params object?[] args) =>
            this.logger.LogTrace(exception, message, args);
        //
        // Summary:
        //     Formats and writes a trace log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   message:
        //     Format string of the log message in message template format. Example: "User {User}
        //     logged in from {Address}"
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogTrace(string? message, params object?[] args) =>
            this.logger.LogTrace(message, args);
        //
        // Summary:
        //     Formats and writes a warning log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   eventId:
        //     The event id associated with the log.
        //
        //   exception:
        //     The exception to log.
        //
        //   message:
        //     Format string of the log message in message template format. Example: "User {User}
        //     logged in from {Address}"
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogWarning(EventId eventId, Exception? exception, string? message, params object?[] args) =>
            this.logger.LogWarning(eventId, exception, message, args);
        //
        // Summary:
        //     Formats and writes a warning log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   eventId:
        //     The event id associated with the log.
        //
        //   message:
        //     Format string of the log message in message template format. Example: "User {User}
        //     logged in from {Address}"
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogWarning(EventId eventId, string? message, params object?[] args) =>
            this.logger.LogWarning(eventId, message, args);
        //
        // Summary:
        //     Formats and writes a warning log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   exception:
        //     The exception to log.
        //
        //   message:
        //     Format string of the log message in message template format. Example: "User {User}
        //     logged in from {Address}"
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogWarning(Exception? exception, string? message, params object?[] args) =>
            this.logger.LogWarning(exception, message, args);
        //
        // Summary:
        //     Formats and writes a warning log message.
        //
        // Parameters:
        //   logger:
        //     The Microsoft.Extensions.Logging.ILogger to write to.
        //
        //   message:
        //     Format string of the log message in message template format. Example: "User {User}
        //     logged in from {Address}"
        //
        //   args:
        //     An object array that contains zero or more objects to format.
        public void LogWarning(string? message, params object?[] args) =>
            this.logger.LogWarning(message, args);
    }
}
