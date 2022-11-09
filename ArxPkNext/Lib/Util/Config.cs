using System;
using System.Reflection;
using System.Configuration;
using System.IO;

namespace Poker.Lib.Util
{
    public sealed class Config
    {
        private static Config _instance;
        private static readonly Object _sync = new Object();
        private readonly Configuration _configManager;
        public readonly string ConfigPath;
        public readonly string Version;
        public readonly string Name;

        private Config()
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            AssemblyName name = executingAssembly.GetName();

            // Retrieve assembly name
            Name = ((AssemblyProductAttribute)Attribute.GetCustomAttribute(executingAssembly, typeof(AssemblyProductAttribute))).Product;

            // Retrieve assembly version
            Version = name.Version.ToString();

            // Retrieve current workdir
            string WorkingPath = Path.GetDirectoryName(executingAssembly.Location);
            ConfigPath = Path.Combine(WorkingPath, name.Name + ".dll.config");

            // Set up a exe configuration map - specify the file name of the DLL's config file
            ExeConfigurationFileMap map = new ExeConfigurationFileMap { ExeConfigFilename = ConfigPath };

            // Now grab the configuration from the ConfigManager
            _configManager = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
        }

        public static Config Instance
        {
            get
            {
                if (_instance == null)
                    lock (_sync)
                        if (_instance == null)
                            _instance = new Config();
                return _instance;
            }
        }

        public string Retrieve(string section, string key)
        {
            // Now grab the <appSettings> section from the opened config
            AppSettingsSection ass = _configManager.GetSection("appSettings") as AppSettingsSection;

            // Build selector as "section.key"
            string selector = string.Format("{0}.{1}", section, key);

            try
            {
                return ass.Settings[selector].Value;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public string LogPath
        {
            get {
                return Instance.Retrieve("log", "path");
            }
        }
    }
}
