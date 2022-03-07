namespace BrightChain.Engine.Models
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;

    public class BrightChainConfiguration : ConfigurationSection
    {
        public BrightChainConfiguration()
            : base(root: new ConfigurationRoot(new List<IConfigurationProvider>() { }), path: string.Empty)
        {
        }
    }
}
