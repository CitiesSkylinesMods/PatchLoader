using System.IO;
using PatchLoaderMod.Doorstop;
using Utils;

namespace PatchLoaderMod.DoorstopUpgrade {
    public class WindowsUpgrade : IUpgradeManager {
        private Logger _logger;
        private DoorstopManager _doorstopManager;
        private ConfigManager<Config> _configManager;
        public UpgradeState State { get; private set; }

        public void SetLogger(Logger logger) {
            _logger = logger;
        }

        public void SetDoorstopManager(DoorstopManager manager) {
            _doorstopManager = manager;
        }

        public void SetConfigManager(ConfigManager<Config> manager) {
            _configManager = manager;
        }
        public void UpdateState() {
            _logger.Info("Updating Upgrade state");
            var config = _configManager.Load();
            if (_doorstopManager.IsLoaderInstalled() && !config.UpgradeInProgress) {
                State = _doorstopManager.CheckLoaderVersionVersion() ? UpgradeState.Latest : UpgradeState.Outdated;
            } else if (Directory.Exists(_doorstopManager.TempDirName)) {
                State = UpgradeState.Phase1;
            } else if (_configManager.Load().UpgradeInProgress) {
                State = UpgradeState.Phase2;
            } else {
                State = UpgradeState.Error;
            }
            
            _logger.Info($"Updated state to [{State}]");
        }

        public bool FollowToPhaseOne() {
            if (Directory.CreateDirectory(_doorstopManager.TempDirName).Exists) {
                if (File.Exists(_doorstopManager.LoaderFileName)) {
                    File.Move(_doorstopManager.LoaderFileName, Path.Combine(_doorstopManager.TempDirName, _doorstopManager.LoaderFileName));
                    var config = _configManager.Load();
                    config.UpgradeInProgress = true;
                    _configManager.Save(config);
                    return true;
                }
            }
            return false;
        }

        public bool FollowToPhaseTwo() {
            if (Directory.Exists(_doorstopManager.TempDirName)) {
                Directory.Delete(_doorstopManager.TempDirName, true);
                if (Directory.Exists(_doorstopManager.TempDirName)) {
                    return false;
                }
                _doorstopManager.Install();
                return true;
            }
            return false;
        }

        public bool FollowToPhaseThree() {
            var config = _configManager.Load();
            if (config.UpgradeInProgress) {
                config.UpgradeInProgress = false;
                _configManager.Save(config);
                return true;
            }
            return false;
        }

        public bool HandleError() {
            return false;
        }
    }
}