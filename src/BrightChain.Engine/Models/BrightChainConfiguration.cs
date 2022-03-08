using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace BrightChain.Engine.Models;

public class BrightChainConfiguration : ConfigurationSection
{
    public BrightChainConfiguration()
        : base(root: new ConfigurationRoot(providers: new List<IConfigurationProvider>()),
            path: string.Empty)
    {
    }
}
