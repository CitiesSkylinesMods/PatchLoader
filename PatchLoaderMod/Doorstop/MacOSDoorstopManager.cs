using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using PatchLoaderMod.DoorstopUpgrade;
using Utils;

namespace PatchLoaderMod.Doorstop {
    public class MacOSDoorstopManager: DoorstopManager {
        public override string LoaderMD5 => "806bb7ee34f2506de077d6a6b18fec44";
        public override bool RequiresRestart { get; protected set; } = false;
        public override bool PlatformSupported => true;

        public override string InstallMessage { get; } = "The game will be closed.\n\n" +
                                                         "\tIMPORTANT!\n\n" +
                                                         "If you use Paradox game launcher:\n" +
                                                         "  1. Open main game directory (Cities.app) navigate to " +
                                                         "     /Contents/Launcher directory and search for launcher-settings.json\n" +
                                                         "  2. Make backup of that file (e.g. create copy with different name)\n" +
                                                         "  3. Open launcher-settings.json with any text editor and change" +
                                                         " 'exePath' value to '../../../Cities_Loader.sh' instead of" +
                                                         " original '../MacOS/Cities' instead of original '../MacOS/Cities'\n" +
                                                         "  4. Save file and run game normally\n\n" +
                                                         "---------------------------------------------------------------------\n" +
                                                         "If don't use Paradox game launcher:\n" +
                                                         "  1. Add './Cities_Loader.sh %command%' (without quotes) to the game Steam Set Launch Options\n" +
                                                         "    in the Steam Client\n" +
                                                         "  2. Run game normally\n" +
                                                         "---------------------------------------------------------------------\n" +
                                                         "If game won't launch remove commandline parameter or restore backup launcher-settings.json\n" +
                                                         "and contact the mod author for more solutions";
        public override string UninstallMessage { get; } = "The game will be closed.\n\n";

        public override bool CanEnable { get; } = true;

        private UnixConfigProperties _configProperties = new UnixConfigProperties(
            "#!/bin/sh\n" +
                    "doorstop_libname=\"doorstop.dylib\"\n" +
                    "doorstop_dir=$PWD\n" +
                    "export DYLD_LIBRARY_PATH=${doorstop_dir}:${DYLD_LIBRARY_PATH};",
            "export DYLD_INSERT_LIBRARIES",
            "export DOORSTOP_ENABLE",
            "export DOORSTOP_INVOKE_DLL_PATH",
            "./Cities.app/Contents/MacOS/Cities $@"
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
                .Append(_configProperties.PreloadKey).Append("=").Append(ExtractInsertLibEnvVariable()).Append(":").AppendLine("$doorstop_libname;")
                .Append(_configProperties.EnabledStateKey).Append("=").Append(_configValues.Enabled.ToString().ToUpper()).AppendLine(";")
                .Append(_configProperties.TargetAssemblyKey).Append("=\"").Append(_configValues.TargetAssembly).AppendLine("\";")
                .Append(_configProperties.GameExePath)
                .ToString();
        }

        private string ExtractInsertLibEnvVariable() {
            string env = Environment.GetEnvironmentVariable("DYLD_INSERT_LIBRARIES") ?? "";
            if (env.Contains("$doorstop_libname")) {
                string[] values = env.Split(':').Where(v => !v.StartsWith("$doorstop_libname")).ToArray();
                env = string.Join(":", values);
            }

            return env;
        }

        protected override ConfigValues InternalLoadConfig(string[] lines) {
            string[] preloadValues = lines[4].Split('=');
            string preloadValue = preloadValues[1];
            
            string[] stateKeyValue = lines[5].Split('=');
            var enabled = bool.Parse(stateKeyValue[1].ToLower().Trim(';'));

            string[] targetPathKeyValue = lines[6].Split('=');
            var targetAssembly = targetPathKeyValue[1].Trim('"', ';');

            _logger.Info($"Loader config parsing complete. Status: [{(enabled ? "enabled" : "disabled")}] Loader assembly path [{targetAssembly}] PreloadValue [{preloadValue}]");

            return new ConfigValues(enabled, targetAssembly, !preloadValue.Contains("Application Support"));
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