using PatchLoaderMod.Doorstop;
using Utils;

namespace PatchLoaderMod.DoorstopUpgrade {
    class MacOSUpgrade : IUpgradeManager {
        private Logger _logger;
        private DoorstopManager _doorstopManager;
        private ConfigManager<Config> _configManager;
        public UpgradeState State { get; private set; }

        public void UpdateState() {
            State = UpgradeState.Latest;
        }

        public void SetDoorstopManager(DoorstopManager manager) {
            _doorstopManager = manager;
        }

        public void SetLogger(Logger logger) {
            _logger = logger;
        }

        public void SetConfigManager(ConfigManager<Config> manager) {
            _configManager = manager;
        }
        public bool FollowToPhaseOne() {
            return false;
        }

        public bool FollowToPhaseTwo() {
            return false;
        }

        public bool FollowToPhaseThree() {
            return false;
        }

        public bool HandleError() {
            return false;
        }
    }
}