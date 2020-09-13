using System;

namespace PatchLoaderMod.Doorstop {
    public class UnixConfigProperties {
        
        public UnixConfigProperties(string header, string enabledStateKey, string targetAssemblyKey, string gameExePath)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            EnabledStateKey = enabledStateKey ?? throw new ArgumentNullException(nameof(enabledStateKey));
            TargetAssemblyKey = targetAssemblyKey ?? throw new ArgumentNullException(nameof(targetAssemblyKey));
            GameExePath = gameExePath ?? throw new ArgumentNullException(nameof(gameExePath));
        }

        public string Header { get; }
        public string EnabledStateKey { get; }
        public string TargetAssemblyKey { get; }
        public string GameExePath { get; }
    }
}