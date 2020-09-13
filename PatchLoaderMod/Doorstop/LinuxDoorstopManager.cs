using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Utils;

namespace PatchLoaderMod.Doorstop {
    public class LinuxDoorstopManager : DoorstopManager {
        public override bool RequiresRestart { get; protected set; } = false;

        public override string InstallMessage { get; } = "The game will be closed.\n\n" +
                                                         "\tIMPORTANT!\n\n" +
                                                         "To make it work, add './Cities_Loader.sh %command%' (without quotes) to the game Steam Set Launch Options\n" +
                                                         "If game won't launch, remove it to run game normally";
        public override string UninstallMessage { get; } = "The game will be closed.\n\n" +
                                                           "\tIMPORTANT!\n\n" +
                                                           "Clear parameters from Steam Set Launch Options if any in order to run game!\n";

        private UnixConfigProperties _configProperties = new UnixConfigProperties(
            "#!/bin/sh\n" +
                    "doorstop_libname=\"doorstop.so\"\n" +
                    "doorstop_dir=$PWD\n"+
                    "export LD_LIBRARY_PATH=${doorstop_dir}:${LD_LIBRARY_PATH};\nexport LD_PRELOAD=$doorstop_libname;",
            "export DOORSTOP_ENABLE",
            "export DOORSTOP_INVOKE_DLL_PATH",
            "./Cities.x64"
        );

        public LinuxDoorstopManager(string expectedTargetAssemblyPath, Logger logger) : base(expectedTargetAssemblyPath, logger) {
            logger.Info("Instantiating LinuxDoorstopManager");
            _loaderFileName = "doorstop.so";
            _configFileName = "Cities_Loader.sh";
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
            string resourcePath = $"PatchLoaderMod.Resources.linux_doorstop.so";
            _logger._Debug("Resource path: " + resourcePath);

            using (Stream input = executingAssembly.GetManifestResourceStream(resourcePath))
            using (Stream output = File.Create("doorstop.so"))
            {
                _logger._Debug("Copying stream.");
                input.CopyStream(output);
            }
        }

        internal void GrantExecuteAccessForConfig() {
            _logger.Info("Trying to grant execute permission to startup script");
            string command = $"chmod +x {_configFileName}";
            Process proc = new Process ();
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
    }
}