using System.IO;
using System.Reflection;
using System.Text;
using Utils;

namespace PatchLoaderMod.Doorstop {
    public partial class WindowsDoorstopManager : DoorstopManager {
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