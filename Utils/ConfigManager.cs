using System;
using System.IO;
using System.Xml.Serialization;

namespace Utils
{
    public class ConfigManager<TC> where TC : class, new()
    {
        private readonly string _filePath;
        private readonly Logger _logger;
        
        private readonly XmlSerializer _xmlSerializer = new XmlSerializer(typeof(TC));
        private readonly XmlSerializerNamespaces _xmlSerializerNamespaces = new XmlSerializerNamespaces();

        public ConfigManager(string filePath, Logger logger)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _xmlSerializerNamespaces.Add(string.Empty, string.Empty);
        }

        public TC Load()
        {
            using (var sr = new StreamReader(_filePath))
            {
                return _xmlSerializer.Deserialize(sr) as TC;
            }
        }

        public void Save(TC data)
        {
            using (var sw = new StreamWriter(_filePath))
            {
                _xmlSerializer.Serialize(sw, data, _xmlSerializerNamespaces);
            }
        }
    }
}
