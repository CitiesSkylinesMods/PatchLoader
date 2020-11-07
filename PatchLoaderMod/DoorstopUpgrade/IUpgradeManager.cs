using PatchLoaderMod.Doorstop;
using Utils;

namespace PatchLoaderMod.DoorstopUpgrade {
    public interface IUpgradeManager {

        UpgradeState State { get; }

        void SetLogger(Logger logger);
        void SetDoorstopManager(DoorstopManager manager);
        void SetConfigManager(ConfigManager<Config> manager);
        
        void UpdateState();

        bool FollowToPhaseOne();
        bool FollowToPhaseTwo();
        bool FollowToPhaseThree();

        bool HandleError();
    }
}