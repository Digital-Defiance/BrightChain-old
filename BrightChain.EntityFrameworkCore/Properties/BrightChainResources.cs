using BrightChain.EntityFrameworkCore.Diagnostics;
using BrightChain.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace BrightChain.EntityFrameworkCore.Properties
{
    public static class BrightChainResources
    {
        /// <summary>
        ///     Saved {count} entities to in-memory store.
        /// </summary>
        public static EventDefinition<int> LogSavedChanges(IDiagnosticsLogger logger)
        {
            var definition = ((Diagnostics.Internal.BrightChainLoggingDefinitions)logger.Definitions).LogSavedChanges;
            if (definition == null)
            {
                definition = NonCapturingLazyInitializer.EnsureInitialized(
                    ref ((Diagnostics.Internal.BrightChainLoggingDefinitions)logger.Definitions).LogSavedChanges,
                    logger,
                static logger => new EventDefinition<int>(
                        logger.Options,
                        BrightChainEventId.ChangesSaved,
                        LogLevel.Information,
                        "BrightChainEventId.ChangesSaved",
                        level => LoggerMessage.Define<int>(
                            level,
                            BrightChainEventId.ChangesSaved,
                            "Saved {count} entities to in-memory store."!)));
            }

            return (EventDefinition<int>)definition;
        }

        /// <summary>
        ///     Transactions are not supported by the in-memory store. See http://go.microsoft.com/fwlink/?LinkId=800142
        /// </summary>
        public static EventDefinition LogTransactionsNotSupported(IDiagnosticsLogger logger)
        {
            var definition = ((Diagnostics.Internal.BrightChainLoggingDefinitions)logger.Definitions).LogTransactionsNotSupported;
            if (definition == null)
            {
                definition = NonCapturingLazyInitializer.EnsureInitialized(
                    ref ((Diagnostics.Internal.BrightChainLoggingDefinitions)logger.Definitions).LogTransactionsNotSupported,
                    logger,
                    static logger => new EventDefinition(
                        logger.Options,
                        BrightChainEventId.TransactionIgnoredWarning,
                        LogLevel.Warning,
                        "InMemoryEventId.TransactionIgnoredWarning",
                        level => LoggerMessage.Define(
                            level,
                            BrightChainEventId.TransactionIgnoredWarning,
                            "Transactions are not supported by the in-memory store. See http://go.microsoft.com/fwlink/?LinkId=800142"!)));
            }

            return (EventDefinition)definition;
        }
    }
}
