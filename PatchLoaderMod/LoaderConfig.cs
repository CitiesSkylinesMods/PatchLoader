using System;
using System.IO;
using System.Reflection;
using System.Text;
using ColossalFramework;
using ColossalFramework.Plugins;
using PatchLoaderMod.Utils;

namespace PatchLoaderMod {
    public class LoaderConfig {
        private const string FileName = "doorstop_config.ini";
        private const string Header = "[UnityDoorstop]";
        private const string StateKey = "enabled";
        private const string TargetAssemblyKey = "targetAssembly";
        private readonly string _defaultTargetAssemblyPath;

        public bool Enabled { get; private set; }
        public string LoaderPath { get; private set; }

        //Use static factory method 'Create()' for construction.
        private LoaderConfig(string defaultTargetAssemblyPath)
        {
            _defaultTargetAssemblyPath = defaultTargetAssemblyPath;
        }

        public static LoaderConfig Create()
        {
            var targetAssemblyPath = FileTools.PathCombine(
                PluginManager.instance.FindPluginInfo(Assembly.GetExecutingAssembly()).modPath,
                "PatchLoader",
                "PatchLoader.dll"
            );

            var loaderConfig = new LoaderConfig(targetAssemblyPath);
            loaderConfig.ReadConfig();
            return loaderConfig;
        }

        public void Enable()
        {
            Enabled = true;
            SaveConfig();
        }

        public void Disable()
        {
            Enabled = false;
            SaveConfig();
        }

        public void SaveConfig() {
            Log.Info($"Saving Loader config, actual loader state: [{Enabled}]");
            string content = BuildConfig();
            Log._Debug($"Saving config: \n{content}");
            try {
                using (FileStream stream = File.Create(FileName)) {
                    byte[] byteContent = new UTF8Encoding(true).GetBytes(content);
                    stream.Write(byteContent, 0, byteContent.Length);
                }

                Log.Info("Loader config saved successfully!");
            } catch (Exception e) {
                Log.Error("Something went wrong while saving config \n" + e);
            }
        }

        private string BuildConfig()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder
                .AppendLine(Header)
                .Append(StateKey).Append("=").AppendLine(Enabled.ToString().ToLower())
                .Append(TargetAssemblyKey).Append("=").Append(LoaderPath.IsNullOrWhiteSpace() ? _defaultTargetAssemblyPath : LoaderPath);

            return stringBuilder.ToString();
        }

        private void ReadConfig() {
            Log.Info($"Reading Loader config");
            if (File.Exists(FileName)) {
                Log.Info("Loader config found! Parsing...");
                string[] lines = File.ReadAllLines( FileName);
                ParseConfig(lines);
                UpdateLoaderPath();
            } else {
                Log.Info("Loader config not found!");
            }
        }

        private void UpdateLoaderPath() {
            if (!_defaultTargetAssemblyPath.Equals(LoaderPath)) {
                LoaderPath = _defaultTargetAssemblyPath;
                Log.Info("Updating Loader config with new path location");
                SaveConfig();
            }
        }

        private void ParseConfig(string[] lines) {
            if (lines.Length == 3) {
                string[] stateKeyValue = lines[1].Split('=');
                if (StateKey.Equals(stateKeyValue[0]) && bool.TryParse(
                        stateKeyValue[1],
                        out bool isEnabled)) {
                    Enabled = isEnabled;
                }

                string[] targetPathKeyValue = lines[2].Split('=');
                if (TargetAssemblyKey.Equals(targetPathKeyValue[0])) {
                    LoaderPath = targetPathKeyValue[1];
                }

                Log.Info($"Loader config parsing complete. Status: [{(Enabled ? "enabled" : "disabled")}] Loader assembly path [{LoaderPath}])");
            } else {
                Log.Error("Invalid Config!");
            }
        }
    }
}