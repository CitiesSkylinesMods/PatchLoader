using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using PatchLoaderMod.DoorstopUpgrade;
using Utils;

namespace PatchLoaderMod.Doorstop {
    public class MacOSDoorstopManager: DoorstopManager {
        public override string LoaderMD5 => "806bb7ee34f2506de077d6a6b18fec44";
        public override bool RequiresRestart { get; protected set; } = false;
        public override bool PlatformSupported => false;
        //todo uncomment when fixed
        // public override string InstallMessage { get; } = "The game will be closed.\n\nIMPORTANT!\n\nAdd './Cities_Loader.sh %command%' (without quotes) to the game Steam Set Launch Options";
        public override string InstallMessage { get; } = "\tMacOS IS NOT SUPPORTED YET!\n\n";
        public override string UninstallMessage { get; } = "The game will be closed.\n\n";

        public override bool CanEnable { get; } = false;

        private UnixConfigProperties _configProperties = new UnixConfigProperties(
            "#!/bin/sh\n" +
                    "doorstop_libname=\"doorstop.dylib\"\n" +
                    "doorstop_dir=$PWD\n" +
                    "export DYLD_LIBRARY_PATH=${doorstop_dir}:${DYLD_LIBRARY_PATH};\n" +
                    "export DYLD_INSERT_LIBRARIES=$doorstop_libname;",
            "export DOORSTOP_ENABLE",
            "export DOORSTOP_INVOKE_DLL_PATH",
            "./Cities.app/Contents/MacOS/Cities"
        );

        public MacOSDoorstopManager(string expectedTargetAssemblyPath, Logger logger) : base(expectedTargetAssemblyPath, logger) {
            logger.Info("Instantiating MacOSDoorstopManager");
            _loaderFileName = "doorstop.dylib";
            _configFileName = "Cities_Loader.sh";
            UpgradeManager = new MacOSUpgrade();
        }

        protected override string BuildConfig() {
            return new StringBuilder()
                .AppendLine(_configProperties.Header)
                .Append(_configProperties.EnabledStateKey).Append("=").Append(_configValues.Enabled.ToString().ToUpper()).AppendLine(";")
                .Append(_configProperties.TargetAssemblyKey).Append("=\"").Append(_configValues.TargetAssembly).AppendLine("\";")
                .Append(_configProperties.GameExePath)
                .ToString();
        }

        protected override ConfigValues InternalLoadConfig(string[] lines) {
            string[] stateKeyValue = lines[5].Split('=');
            var enabled = bool.Parse(stateKeyValue[1].ToLower().Trim(';'));

            string[] targetPathKeyValue = lines[6].Split('=');
            var targetAssembly = targetPathKeyValue[1].Trim('"', ';');

            _logger.Info($"Loader config parsing complete. Status: [{(enabled ? "enabled" : "disabled")}] Loader assembly path [{targetAssembly}])");

            return new ConfigValues(enabled, targetAssembly);
        }

        protected override void InstallLoader() {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string resourcePath = $"PatchLoaderMod.Resources.macos_doorstop.dylib";
            _logger._Debug("Resource path: " + resourcePath);

            using (Stream input = executingAssembly.GetManifestResourceStream(resourcePath))
            using (Stream output = File.Create("doorstop.dylib"))
            {
                _logger._Debug("Copying stream.");
                input.CopyStream(output);
            }
        }
        
        internal void GrantExecuteAccessForConfig() {
            _logger.Info("Trying to grant execute permission to startup script");
            string command = $"chmod +x {_configFileName}";
            Process proc = new Process();
            proc.StartInfo.FileName = "/bin/bash";
            proc.StartInfo.Arguments = "-c \" " + command + " \"";
            proc.StartInfo.UseShellExecute = false; 
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();
            StringBuilder builder = new StringBuilder();
            while (!proc.StandardOutput.EndOfStream) {
                builder.AppendLine(proc.StandardOutput.ReadLine ());
            }
            _logger.Info("Result:");
            _logger.Info(builder.ToString());
        }
        
        protected override bool IsLatestLoaderVersion() {
            return true;
        }
    }
}