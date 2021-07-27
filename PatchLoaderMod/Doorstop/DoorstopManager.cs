using System;
using System.IO;
using System.Text;
using ColossalFramework.UI;
using PatchLoaderMod.DoorstopUpgrade;
using UnityEngine;
using Utils;
using Logger = Utils.Logger;

namespace PatchLoaderMod.Doorstop {
    public abstract class DoorstopManager {
        protected readonly string _expectedTargetAssemblyPath;
        protected readonly Logger _logger;
        protected string _loaderFileName;
        protected string _configFileName;
        public virtual string LoaderMD5 => "";

        public string TempDirName => "Temp_PatchLoader";
        public virtual bool RequiresRestart { get; protected set; } = false;
        
        public virtual string InstallMessage { get; } = "The game must be closed.\nPlease start game once again to fully initialize mod";
        public virtual string UninstallMessage { get; } = "The game must be restarted in order to restore it's original state.";
        
        public virtual bool CanEnable { get; } = true;
        public virtual bool PlatformSupported { get; } = false;

        protected abstract void InstallLoader();
        protected abstract string BuildConfig();
        protected abstract bool IsLatestLoaderVersion();
        protected abstract ConfigValues InternalLoadConfig(string[] lines);
        
        protected ConfigValues _configValues;

        public IUpgradeManager UpgradeManager { get; protected set; }
        
        public string LoaderFileName {
            get { return _loaderFileName; }
        }

        //Use static factory method 'Create()' for construction.
        protected DoorstopManager(string expectedTargetAssemblyPath, Logger logger) {
            _expectedTargetAssemblyPath = expectedTargetAssemblyPath ?? throw new ArgumentNullException(nameof(expectedTargetAssemblyPath));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public static DoorstopManager Create(string expectedTargetAssemblyPath, Logger logger, ConfigManager<Config> configManager) {
            DoorstopManager manager = null;
            RuntimePlatform platform = Application.platform;
            if (platform == RuntimePlatform.WindowsPlayer || platform == RuntimePlatform.WindowsEditor) {
                manager = new WindowsDoorstopManager(expectedTargetAssemblyPath, logger);
            }

            if (platform == RuntimePlatform.OSXPlayer || platform == RuntimePlatform.OSXEditor) {
                manager = new MacOSDoorstopManager(expectedTargetAssemblyPath, logger);
            }

            if (platform == RuntimePlatform.LinuxPlayer) {
                manager = new LinuxDoorstopManager(expectedTargetAssemblyPath, logger);
            }

            if (manager == null) {
                throw new PlatformNotSupportedException($"Platform {platform} is not supported!");
            }
            manager.UpgradeManager.SetLogger(logger);
            manager.UpgradeManager.SetDoorstopManager(manager);
            manager.UpgradeManager.SetConfigManager(configManager);
            manager.UpgradeManager.UpdateState();

            if (manager.IsInstalled()) {
                manager.LoadConfig();
                manager.RestoreTargetAssemblyPathIfNecessary();
            }

            return manager;
        }

        public bool IsInstalled() {
            return IsLoaderInstalled()
                   && File.Exists(_configFileName);
        }

        public bool IsLoaderInstalled() {
            return File.Exists(LoaderFileName);
        }

        public void Install() {
            if (!File.Exists(LoaderFileName)) {
                InstallLoader();
            }

            if (!File.Exists(_configFileName)) {
                InstallConfigFile();
            }

            RuntimePlatform platform = Application.platform;
            if (File.Exists(_configFileName) && platform != RuntimePlatform.WindowsPlayer && platform != RuntimePlatform.WindowsEditor) {
                if (platform == RuntimePlatform.LinuxPlayer) {
                    (this as LinuxDoorstopManager)?.GrantExecuteAccessForConfig();
                    RequiresRestart = true;
                } else if (platform == RuntimePlatform.OSXPlayer) {
                    (this as MacOSDoorstopManager)?.GrantExecuteAccessForConfig();
                }
            } else {
                RequiresRestart = true;
            }

            _logger.Info("Doorstop installed successfully.");
        }

        public bool IsEnabled() {
            return _configValues.Enabled;
        }

        public void Enable() {
            _configValues.Enabled = true;
            SaveConfig();
            RequiresRestart = true;
        }

        public void Disable() {
            _configValues.Enabled = false;
            SaveConfig();
            RequiresRestart = true;
        }
        
        private void InstallConfigFile() {
            _configValues = new ConfigValues(true, _expectedTargetAssemblyPath);
            SaveConfig();
        }

        public bool CheckLoaderVersionVersion() {
            return IsLatestLoaderVersion();
        }

        private void SaveConfig() {
            _logger.Info($"Saving Loader config, actual loader state: [{_configValues.Enabled}]");
            string content = BuildConfig();
            _logger._Debug($"Saving config: \n{content}");
            try {
                using (FileStream stream = File.Create(_configFileName)) {
                    byte[] byteContent = new UTF8Encoding(true).GetBytes(content);
                    stream.Write(byteContent, 0, byteContent.Length);
                }

                _logger.Info("Loader config saved successfully!");
            } catch (Exception e) {
                _logger.Error("Something went wrong while saving config \n" + e);
            }
        }

        private void LoadConfig() {
            _logger.Info($"Loading Doorstop config");

            try {
                string[] lines = File.ReadAllLines(_configFileName);

                _logger.Info("Doorstop config found! Parsing...");

                _configValues = InternalLoadConfig(lines);
            } catch (FileNotFoundException) {
                _logger.Info("Doorstop config not found!");
                throw;
            } catch (Exception) {
                _logger.Error("Invalid Config!");
                throw;
            }
        }

        private void RestoreTargetAssemblyPathIfNecessary() {
            if (_configValues.TargetAssembly == _expectedTargetAssemblyPath && !_configValues.RequiresReset)
                return;

            _logger.Info("Updating Loader config with new path location. " + $"{(_configValues.RequiresReset? "[Resetting...]" :"")}");

            _configValues.TargetAssembly = _expectedTargetAssemblyPath;
            SaveConfig();

            RequiresRestart = true;
        }

        public void OpenGenericConfirmModal(string message, Action<int> callback) {
            CoroutineHelper.WaitFor(
                () => UIView.library != null,
                success: () => { UIView.library.ShowModal<ConfirmPanel>("ConfirmPanel", (comp, result) => { callback.Invoke(result); }).SetMessage("PatchLoaderMod", message); },
                failure: () => throw new Exception("PatchLoader could not open an important dialog. Something seems to be seriously broken. Please contact the author."),
                stopPollingAfterInSec: 30f
            );
        }
    }
}