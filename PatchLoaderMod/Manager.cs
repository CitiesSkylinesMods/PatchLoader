using System;
using System.IO;
using System.Reflection;
using PatchLoaderMod.Utils;

namespace PatchLoaderMod {
    public class Manager {
        public static Manager _instance;

        public static Manager Instance => _instance ?? (_instance = new Manager());
        
        public bool Installed { get; private set; }
        public bool Enabled { get; private set; }

        private Manager() {
            Init();
        }

        private void Init() {
            if (CheckIfInstalled()) {
                Installed = true;
                Enabled = CheckIfEnabled();
            }

        }

        public void UpdateStatus() {
            Installed = CheckIfInstalled();
            Enabled = CheckIfEnabled();
        }

        private bool CheckIfInstalled() {
            return LoaderConfig.Instance.Exists && File.Exists("winhttp.dll");
        }

        private bool CheckIfEnabled() {
            return LoaderConfig.Instance.Exists && LoaderConfig.Instance.Enabled;
        }

        public bool Install() {
            if (!Installed) {
                LoaderConfig.Instance.Enabled = true;
                LoaderConfig.Instance.SaveConfig();
                
                Assembly executingAssembly = Assembly.GetExecutingAssembly();
                try {
                    string resourcePath = $"PatchLoaderMod.Resources.winhttp.dll";
                    Log._Debug("Resource path: " + resourcePath);
                    using (Stream input = executingAssembly.GetManifestResourceStream(resourcePath)) {
                        try {
                            using (Stream output = File.Create("winhttp.dll")) {
                                FileTools.CopyStream(input, output);
                            }
                        } catch (Exception e) {
                            Log.Error(e.Message + "\n" + e.StackTrace);
                            throw e;
                        }
                    }

                    Log.Info("Loader installed successfully.");
                    PatchLoaderMod.RequireRestart();
                    return true;
                } catch (Exception e) {
                    Log.Error(e.Message + "\n" + e.StackTrace);
                }

                UpdateStatus();
                
            }

            return false;
        }
        public void Enable() {
            if (Installed) {
                LoaderConfig.Instance.Enabled = true;
                LoaderConfig.Instance.SaveConfig();
                PatchLoaderMod.RequireRestart();
                UpdateStatus();
            }
        }
        public void Disable() {
            if (Installed) {
                LoaderConfig.Instance.Enabled = false;
                LoaderConfig.Instance.SaveConfig();
                PatchLoaderMod.RequireRestart();
                UpdateStatus();
            }
        }
    }
}