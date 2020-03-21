using ICities;
using PatchLoaderMod.Utils;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace PatchLoaderMod {
    public class PatchLoaderMod : LoadingExtensionBase, IUserMod {
        private SettingsUi _settingsUi;
        private LoaderConfig _loaderConfig;

        public string Name => "Patch Loader Mod";
        public string Description => "Automatically loads Patches implementing IPatch API.";
        public bool Enabled => _loaderConfig.Enabled;
        public bool RestartRequired { get; private set; }

        public void OnEnabled() {
            _loaderConfig = LoaderConfig.Create();
            Debug.Log("PatchLoader enabled");
        }

        public void OnDisabled() {
            _loaderConfig = null;
            Debug.Log("PatchLoader disabled");
        }

        public void Enable()
        {
            if (CheckIfInstalled())
            {
                _loaderConfig.Enable();
                RestartRequired = true;
            }
        }

        public void Disable()
        {
            if (CheckIfInstalled())
            {
                _loaderConfig.Disable();
                RestartRequired = true;
            }
        }

        public void ToggleEnabled()
        {
            if (_loaderConfig.Enabled)
            {
                Disable();
            }
            else
            {
                Enable();
            }
        }

        public override void OnCreated(ILoading loading)
        {
            if (LoadingManager.instance.m_loadingComplete)
            {
                //hot-reload
                _settingsUi.InGame();
            }
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            _settingsUi.InGame();
        }

        public override void OnLevelUnloading()
        {
            _settingsUi.InMenu();
        }

        // ReSharper disable once InconsistentNaming
        public void OnSettingsUI(UIHelperBase helper)
        {
            _settingsUi = new SettingsUi(this);
            _settingsUi.CreateUi(helper);
        }

        public void Install()
        {
            _loaderConfig.Enable();

            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            try
            {
                string resourcePath = $"PatchLoaderMod.Resources.winhttp.dll";
                Log._Debug("Resource path: " + resourcePath);
                using (Stream input = executingAssembly.GetManifestResourceStream(resourcePath))
                {
                    try
                    {
                        using (Stream output = File.Create("winhttp.dll"))
                        {
                            FileTools.CopyStream(input, output);
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error(e.Message + "\n" + e.StackTrace);
                        throw e;
                    }
                }

                Log.Info("Loader installed successfully.");
                RestartRequired = true;
            }
            catch (Exception e)
            {
                Log.Error(e.Message + "\n" + e.StackTrace);
            }
        }

        public bool CheckIfInstalled()
        {
            return File.Exists("winhttp.dll");
        }
    }
}