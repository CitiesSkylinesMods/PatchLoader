using System.IO;
using System.Reflection;
using System.Text;
using PatchLoaderMod.DoorstopUpgrade;
using Utils;

namespace PatchLoaderMod.Doorstop {
    public class WindowsDoorstopManager : DoorstopManager {
        public override string LoaderMD5 => "88fcbe634fe023a020fe533814da7840"; //3.0.2.2
        public override bool PlatformSupported => true;
        
        private WindowsConfigProperties _configProperties = new WindowsConfigProperties(
            "[UnityDoorstop]",
            "enabled",
            "targetAssembly"
        );

        public override bool RequiresRestart { get; protected set; } = false;

        public WindowsDoorstopManager(string expectedTargetAssemblyPath, Logger logger) : base(expectedTargetAssemblyPath, logger) {
            logger.Info("Instantiating WindowsDoorstopManager");
            _loaderFileName = "winhttp.dll";
            _configFileName = "doorstop_config.ini";
            UpgradeManager = new WindowsUpgrade();
        }

        protected override void InstallLoader() {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string resourcePath = $"PatchLoaderMod.Resources.winhttp.dll";
            _logger._Debug("Resource path: " + resourcePath);

            using (Stream input = executingAssembly.GetManifestResourceStream(resourcePath))
            using (Stream output = File.Create("winhttp.dll")) {
                _logger._Debug("Copying stream.");
                input.CopyStream(output);
            }
        }

        protected override string BuildConfig() {
            return new StringBuilder()
                .AppendLine(_configProperties.Header)
                .Append(_configProperties.EnabledStateKey).Append("=").AppendLine(_configValues.Enabled.ToString().ToLower())
                .Append(_configProperties.TargetAssemblyKey).Append("=").Append(_configValues.TargetAssembly)
                .ToString();
        }

        protected override bool IsLatestLoaderVersion() {
            string loaderHash = FileExtensions.CalculateFileMd5Hash(LoaderFileName);
            _logger.Info($"Calculated Hash {loaderHash} expected {LoaderMD5}");
            return LoaderMD5.Equals(loaderHash);
        }

        protected override ConfigValues InternalLoadConfig(string[] lines) {
            string[] stateKeyValue = lines[1].Split('=');
            var enabled = bool.Parse(stateKeyValue[1]);

            string[] targetPathKeyValue = lines[2].Split('=');
            var targetAssembly = targetPathKeyValue[1];

            _logger.Info($"Loader config parsing complete. Status: [{(enabled ? "enabled" : "disabled")}] Loader assembly path [{targetAssembly}])");

            return new ConfigValues(enabled, targetAssembly);
        }
    }
}