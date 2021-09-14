namespace BrightChain.Engine.Helpers
{
    using System;
    using System.IO;
    using Microsoft.Extensions.Configuration;

    public static class ConfigurationHelper
    {
        /// <summary>
        /// Gets a string containing the directory to look for configuration files in.
        /// </summary>
        public static string ConfigurationBaseDirectory
            => AppDomain.CurrentDomain.BaseDirectory;

        /// <summary>
        /// Gets a string containing the non-pathed configuration file name to look for within the base path.
        /// </summary>
        public static string ConfigurationFileName
            => "brightChainSettings.json";

        public static string FullyQualifiedConfigurationFileName
            => Path.Combine(
                path1: ConfigurationHelper.ConfigurationBaseDirectory,
                path2: ConfigurationHelper.ConfigurationFileName);

        public static IConfiguration LoadConfiguration()
        {
            return new ConfigurationBuilder()
                           .SetBasePath(ConfigurationHelper.ConfigurationBaseDirectory)
                           .AddJsonFile(
                               path: ConfigurationHelper.ConfigurationFileName,
                               optional: false,
                               reloadOnChange: true)
                           .AddEnvironmentVariables()
                       .Build();
        }
    }
}
