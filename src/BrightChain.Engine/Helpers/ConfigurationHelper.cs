using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace BrightChain.Engine.Helpers
{
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
            => "brightChainSettings.yaml";

        public static string FullyQualifiedConfigurationFileName
            => Path.Combine(
                path1: ConfigurationHelper.ConfigurationBaseDirectory,
                path2: ConfigurationHelper.ConfigurationFileName);

        public static IConfiguration LoadConfiguration()
        {
            return new ConfigurationBuilder()
                           .SetBasePath(ConfigurationHelper.ConfigurationBaseDirectory)
                           .AddYamlFile(
                               path: ConfigurationHelper.ConfigurationFileName,
                               optional: false,
                               reloadOnChange: true)
                           .AddEnvironmentVariables()
                       .Build();
        }

        public static T LoadConfigurationAs<T>()
        {
            var deserializer = new YamlDotNet.Serialization.Deserializer();
            dynamic sourceDocument = deserializer.Deserialize(
                input: File.ReadAllText(ConfigurationHelper.FullyQualifiedConfigurationFileName),
                type: typeof(T));
            return sourceDocument;
        }

        public static void SaveConfigurationFrom<T>(T configuration)
        {
            File.WriteAllText(ConfigurationHelper.FullyQualifiedConfigurationFileName, new YamlDotNet.Serialization.Serializer().Serialize(configuration));
        }

        public static void AddOrUpdateAppSetting<T>(string sectionPathKey, T value)
        {
            try
            {
                dynamic configData = LoadConfigurationAs<dynamic>();
                SetValueRecursively(sectionPathKey, configData, value);
                SaveConfigurationFrom<dynamic>(configData);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error writing app settings | {ex.Message}", ex);
            }
        }

        private static void SetValueRecursively<T>(string sectionPathKey, dynamic jsonObj, T value)
        {
            // split the string at the first ':' character
            var remainingSections = sectionPathKey.Split(":", 2);

            var currentSection = remainingSections[0];
            if (remainingSections.Length > 1)
            {
                // continue with the procress, moving down the tree
                var nextSection = remainingSections[1];
                SetValueRecursively(nextSection, jsonObj[currentSection], value);
            }
            else
            {
                // we've got to the end of the tree, set the value
                jsonObj[currentSection] = value;
            }
        }
    }
}
