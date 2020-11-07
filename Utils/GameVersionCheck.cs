using System;
using System.Reflection;

namespace Utils {
    public static class GameVersionCheck {
        private const uint _minVersion = 188997904U;
        private static readonly Version MinVersion = new Version(1,13,1);

        public static bool IsGameVersionSupported(uint version) {
            return version >= _minVersion;
        }

        public static bool IsGameVersionSupported(Version version) {
            return version >= MinVersion;
        }

        public static Version GetVersion(string versionString) {
            string[] majorVersionElms = versionString.Split('-');
            string[] versionElms = majorVersionElms[0].Split('.');
            int versionA = Convert.ToInt32(versionElms[0]);
            int versionB = Convert.ToInt32(versionElms[1]);
            int versionC = Convert.ToInt32(versionElms[2]);
            
            return new Version(versionA, versionB, versionC);
        }
    }
}