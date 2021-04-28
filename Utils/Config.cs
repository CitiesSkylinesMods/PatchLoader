namespace Utils
{
    public class Config
    {
        public int ConfigVersion { get; set; }
        public string WorkshopPath { get; set; }
        
        public bool UpgradeInProgress { get; set; }


        public static Config InitialValues() {
            return new Config(){ConfigVersion = 1, UpgradeInProgress = false, WorkshopPath = ""};
        }
    }
}