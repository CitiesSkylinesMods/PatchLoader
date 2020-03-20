using ICities;

namespace PatchLoaderMod {
    public class LoadingExtension : ILoadingExtension {
        public void OnCreated(ILoading loading) {
            if (LoadingManager.instance.m_loadingComplete) {
                //hot-reload
                PatchLoaderMod.SettingsUi.InGame();
            }
        }

        public void OnReleased() {
        }

        public void OnLevelLoaded(LoadMode mode) {
            PatchLoaderMod.SettingsUi.InGame();
        }

        public void OnLevelUnloading() {
            PatchLoaderMod.SettingsUi.InMenu();
        }
    }
}