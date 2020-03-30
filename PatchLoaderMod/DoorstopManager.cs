using System;
using System.IO;
using System.Reflection;
using System.Text;
using Utils;

namespace PatchLoaderMod
{
    public class DoorstopManager
    {
        private readonly string _winHttpFileName = "winhttp.dll";
        private readonly string _configFileName = "doorstop_config.ini";

        private readonly string _expectedTargetAssemblyPath;
        private ConfigProperties _configProperties = new ConfigProperties(
            "[UnityDoorstop]",
            "enabled",
            "targetAssembly"
        );
        private ConfigValues _configValues;
        private Logger _logger;

        public bool RequiresRestart { get; private set; } = false;

        //Use static factory method 'Create()' for construction.
        private DoorstopManager(string expectedTargetAssemblyPath, Logger logger)
        {
            _expectedTargetAssemblyPath = expectedTargetAssemblyPath ?? throw new ArgumentNullException(nameof(expectedTargetAssemblyPath));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public static DoorstopManager Create(string expectedTargetAssemblyPath, Logger logger)
        {
            var doorstopManager = new DoorstopManager(expectedTargetAssemblyPath, logger);
            if (doorstopManager.IsInstalled())
            {
                doorstopManager.LoadConfig();
                doorstopManager.RestoreTargetAssemblyPathIfNecessary();
            }
            return doorstopManager;
        }

        public bool IsInstalled()
        {
            return File.Exists(_winHttpFileName)
                && File.Exists(_configFileName);
        }

        public void Install()
        {
            if (!File.Exists(_winHttpFileName))
            {
                InstallWinHttpFile();
            }

            if (!File.Exists(_configFileName))
            {
                InstallConfigFile();
            }

            RequiresRestart = true;

            _logger.Info("Doorstop installed successfully.");
        }

        private void InstallWinHttpFile()
        {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string resourcePath = $"PatchLoaderMod.Resources.winhttp.dll";
            _logger._Debug("Resource path: " + resourcePath);

            using (Stream input = executingAssembly.GetManifestResourceStream(resourcePath))
            using (Stream output = File.Create("winhttp.dll"))
            {
                _logger._Debug("Copying stream.");
                input.CopyStream(output);
            }
        }

        private void InstallConfigFile()
        {
            _configValues = new ConfigValues(true, _expectedTargetAssemblyPath);
            SaveConfig();
        }

        public bool IsEnabled()
        {
            return _configValues.Enabled;
        }

        public void Enable()
        {
            _configValues.Enabled = true;
            SaveConfig();
            RequiresRestart = true;
        }

        public void Disable()
        {
            _configValues.Enabled = false;
            SaveConfig();
            RequiresRestart = true;
        }

        private void SaveConfig()
        {
            _logger.Info($"Saving Loader config, actual loader state: [{_configValues.Enabled}]");
            string content = BuildConfig(_configProperties, _configValues);
            _logger._Debug($"Saving config: \n{content}");
            try
            {
                using (FileStream stream = File.Create(_configFileName))
                {
                    byte[] byteContent = new UTF8Encoding(true).GetBytes(content);
                    stream.Write(byteContent, 0, byteContent.Length);
                }

                _logger.Info("Loader config saved successfully!");
            }
            catch (Exception e)
            {
                _logger.Error("Something went wrong while saving config \n" + e);
            }
        }

        private string BuildConfig(ConfigProperties configProperties, ConfigValues configValues)
        {
            return new StringBuilder()
                .AppendLine(configProperties.Header)
                .Append(configProperties.EnabledStateKey).Append("=").AppendLine(configValues.Enabled.ToString().ToLower())
                .Append(configProperties.TargetAssemblyKey).Append("=").Append(configValues.TargetAssembly)
                .ToString();
        }

        private void LoadConfig()
        {
            _logger.Info($"Loading Doorstop config");

            try
            {
                string[] lines = File.ReadAllLines(_configFileName);

                _logger.Info("Doorstop config found! Parsing...");

                string[] stateKeyValue = lines[1].Split('=');
                var enabled = bool.Parse(stateKeyValue[1]);

                string[] targetPathKeyValue = lines[2].Split('=');
                var targetAssembly = targetPathKeyValue[1];

                _logger.Info($"Loader config parsing complete. Status: [{(enabled ? "enabled" : "disabled")}] Loader assembly path [{targetAssembly}])");

                _configValues = new ConfigValues(enabled, targetAssembly);
            }
            catch (FileNotFoundException)
            {
                _logger.Info("Doorstop config not found!");
                throw;
            }
            catch (Exception)
            {
                _logger.Error("Invalid Config!");
                throw;
            }
        }

        private void RestoreTargetAssemblyPathIfNecessary()
        {
            if (_configValues.TargetAssembly == _expectedTargetAssemblyPath)
                return;

            _logger.Info("Updating Loader config with new path location");

            _configValues.TargetAssembly = _expectedTargetAssemblyPath;
            SaveConfig();

            RequiresRestart = true;
        }

        private class ConfigProperties
        {
            public ConfigProperties(string header, string enabledStateKey, string targetAssemblyKey)
            {
                Header = header ?? throw new ArgumentNullException(nameof(header));
                EnabledStateKey = enabledStateKey ?? throw new ArgumentNullException(nameof(enabledStateKey));
                TargetAssemblyKey = targetAssemblyKey ?? throw new ArgumentNullException(nameof(targetAssemblyKey));
            }

            public string Header { get; }
            public string EnabledStateKey { get; }
            public string TargetAssemblyKey { get; }
        }

        private class ConfigValues
        {
            public ConfigValues(bool enabled, string targetAssembly)
            {
                Enabled = enabled;
                TargetAssembly = targetAssembly ?? throw new ArgumentNullException(nameof(targetAssembly));
            }

            public bool Enabled { get; set; }
            public string TargetAssembly { get; set; }
        }
    }
}