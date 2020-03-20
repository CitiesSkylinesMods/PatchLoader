using PatchLoader.Utils;

namespace PatchLoader {

    [ConfigPath("PatchLoader.Config.xml")] // Stored in ...\Steam\steamapps\common\Cities_Skylines
    public class Config {
        public string WorkshopPath { get; set; }

        private static Config _instance;
        public static Config Instance => _instance ?? (_instance = ConfigManager<Config>.Load());
        public void Save() => ConfigManager<Config>.Save();
    }
}