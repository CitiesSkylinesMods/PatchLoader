using System;

namespace PatchLoaderMod.Doorstop {
        public class ConfigValues
        {
            public ConfigValues(bool enabled, string targetAssembly, bool requiresReset = false)
            {
                Enabled = enabled;
                TargetAssembly = targetAssembly ?? throw new ArgumentNullException(nameof(targetAssembly));
                RequiresReset = requiresReset;
            }

            public bool Enabled { get; set; }
            public string TargetAssembly { get; set; }
            
            public bool RequiresReset { get; }
        }
}