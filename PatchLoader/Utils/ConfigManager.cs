
using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace PatchLoader.Utils {
    public abstract class ConfigManager<TC> where TC : class, new() {
        private static TC _instance;

        public static TC Load() {
            if (_instance == null) {
                var configPath = GetConfigPath();
                Log.Info($"[ConfigManager.Load] {configPath}");
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(TC));

                try {
                    if (File.Exists(configPath)) {
                        using (StreamReader streamReader = new StreamReader(configPath)) {
                            _instance = xmlSerializer.Deserialize(streamReader) as TC;
                        }
                    } else {
                        Log._Debug($"File {configPath} doesn't exist.");
                    }
                }
                catch (Exception e) {
                    Log.Error("[ConfigManager.Load] Error: " + e.Message);
                }
            }
            return _instance ?? (_instance = new TC());
        }

        public static void Save() {
            if (_instance == null) return;

            string configPath = GetConfigPath();
            Log.Info($"[ConfigManager.Save] {configPath}");

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(TC));
            XmlSerializerNamespaces noNamespaces = new XmlSerializerNamespaces();
            noNamespaces.Add("", "");

            try {
                using (StreamWriter streamWriter = new StreamWriter(configPath)) {
                    xmlSerializer.Serialize(streamWriter, _instance, noNamespaces);
                }
            }
            catch (Exception e) {
                Log.Error("[ConfigManager.Save] Error: " + e.Message);
            }
        }

        private static string GetConfigPath() {
            if (typeof(TC).GetCustomAttributes(typeof(ConfigPathAttribute), true)
                .FirstOrDefault() is ConfigPathAttribute configPathAttribute) {
                return configPathAttribute.Value;
            } else {
                Log.Error("[OptionsManager.GetConfigPath] ConfigurationPath attribute missing in " + typeof(TC).Name);
                return typeof(TC).Name + ".xml";
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigPathAttribute : Attribute {
        public ConfigPathAttribute(string value) {
            Value = value;
        }

        public string Value { get; private set; }
    }
}
