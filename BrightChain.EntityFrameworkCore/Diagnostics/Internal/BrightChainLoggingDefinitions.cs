using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BrightChain.EntityFrameworkCore.Diagnostics.Internal
{
    public class BrightChainLoggingDefinitions : LoggingDefinitions
    {
        public EventDefinitionBase LogSavedChanges;
        public EventDefinitionBase LogTransactionsNotSupported;
    }
}
