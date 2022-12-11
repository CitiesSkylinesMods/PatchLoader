using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using PatchLoaderMod.DoorstopUpgrade;
using Utils;

namespace PatchLoaderMod.Doorstop {
    public class LinuxDoorstopManager : DoorstopManager {
        public override string LoaderMD5 => "bc1a46c8a1da53e03a309b74442cca48";
        public override bool RequiresRestart { get; protected set; } = false;
        public override bool PlatformSupported => true;

        public override string InstallMessage { get; } = "The game will be closed.\n\n" +
                                                         "\tIMPORTANT!\n\n" +
                                                         "If you use Paradox game launcher:\n" +
                                                         "  1. Open main game directory and search for launcher-settings.json\n" +
                                                         "  2. Make backup of that file (e.g. create copy with different name)\n" +
                                                         "  3. Open launcher-settings.json with any text editor and change\n" +
                                                         "  'exePath' value to 'Cities_Loader.sh' ('Cities.x64' is the default)\n" +
                                                         "  4. Save file and run game normally\n\n" +
                                                         "---------------------------------------------------------------------\n" +
                                                         "If you don't use Paradox game launcher:\n" +
                                                         "  1. Add './Cities_Loader.sh %command%' (without quotes) to the game Steam Set Launch Options\n" +
                                                         "  2. Run game normally\n" +
                                                         "---------------------------------------------------------------------\n" +
                                                         "If game won't launch, remove parameter or restore backup launcher-settings.json to run game normally\n" +
                                                         "and contact the mod author for more solutions";

        public override string UninstallMessage { get; } = "The game will be closed.\n\n" +
                                                           "\tIMPORTANT!\n\n" +
                                                           "Clear parameters from Steam Set Launch Options if any or restore backup launcher-settings.json\n in order to run game!\n";

        private UnixConfigProperties _configProperties = new UnixConfigProperties(
            "#!/bin/sh\n" +
            "doorstop_libname=\"doorstop.so\"\n" +
            "doorstop_dir=$PWD\n" +
            "export LD_LIBRARY_PATH=${doorstop_dir}:${LD_LIBRARY_PATH};",
            "export LD_PRELOAD",
            "export DOORSTOP_ENABLE",
            "export DOORSTOP_INVOKE_DLL_PATH",
            "exec ./Cities.x64 $@"
        );

        public LinuxDoorstopManager(string expectedTargetAssemblyPath, Logger logger) : base(expectedTargetAssemblyPath, logger) {
            logger.Info("Instantiating LinuxDoorstopManager");
            _loaderFileName = "doorstop.so";
            _configFileName = "Cities_Loader.sh";
            UpgradeManager = new LinuxUpgrade();
        }

        protected override string BuildConfig() {
            return new StringBuilder()
                .AppendLine(_configProperties.Header)
                .Append(_configProperties.PreloadKey).Append("=").AppendLine("$LD_PRELOAD:$doorstop_libname;")
                .Append(_configProperties.EnabledStateKey).Append("=").Append(_configValues.Enabled.ToString().ToUpper()).AppendLine(";")
                .Append(_configProperties.TargetAssemblyKey).Append("=\"").Append(_configValues.TargetAssembly).AppendLine("\";")
                .Append(_configProperties.GameExePath)
                .ToString();
        }

        protected override ConfigValues InternalLoadConfig(string[] lines) {
            string[] preloadValues = lines[4].Split('=');
            string preloadValue = preloadValues[1];
            
            string[] stateKeyValue = lines[5].Split('=');
            var enabled = bool.Parse(stateKeyValue[1].ToLower().Trim(';'));

            string[] targetPathKeyValue = lines[6].Split('=');
            var targetAssembly = targetPathKeyValue[1].Trim('"', ';');

            _logger.Info($"Loader config parsing complete. Status: [{(enabled ? "enabled" : "disabled")}] Loader assembly path [{targetAssembly}] Preload [{preloadValue}]");

            return new ConfigValues(enabled, targetAssembly, !"$LD_PRELOAD:$doorstop_libname;".Equals(preloadValue));
        }

        protected override void InstallLoader() {
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            string resourcePath = $"PatchLoaderMod.Resources.linux_doorstop.so";
            _logger._Debug("Resource path: " + resourcePath);

            using (Stream input = executingAssembly.GetManifestResourceStream(resourcePath))
            using (Stream output = File.Create("doorstop.so")) {
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
                builder.AppendLine(proc.StandardOutput.ReadLine());
            }

            _logger.Info("Result:");
            _logger.Info(builder.ToString());
        }
        
        protected override bool IsLatestLoaderVersion() {
            string loaderHash = FileExtensions.CalculateFileMd5Hash(LoaderFileName);
            _logger.Info($"Calculated Hash {loaderHash} expected {LoaderMD5}");
            return LoaderMD5.Equals(loaderHash);
        }
    }
}
