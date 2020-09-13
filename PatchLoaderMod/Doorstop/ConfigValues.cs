using System;

namespace PatchLoaderMod.Doorstop {
        public class ConfigValues
        {
            public ConfigValues(bool enabled, string targetAssembly)
            {
                Enabled = enabled;
                TargetAssembly = targetAssembly ?? throw new ArgumentNullException(nameof(targetAssembly));
            }

            public bool Enabled { get; set; }
            public string TargetAssembly { get; set; }
        }
}