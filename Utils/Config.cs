namespace Utils
{
    public class Config
    {
        public string WorkshopPath { get; set; }
        
        public bool UpgradeInProgress { get; set; }


        public static Config InitialValues() {
            return new Config(){UpgradeInProgress = false, WorkshopPath = ""};
        }
    }
}