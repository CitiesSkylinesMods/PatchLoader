using System;

namespace PatchLoaderMod.Doorstop {
    public class UnixConfigProperties {
        
        public UnixConfigProperties(string header, string preloadKey, string enabledStateKey, string targetAssemblyKey, string gameExePath)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            PreloadKey = preloadKey ?? throw new ArgumentNullException(nameof(preloadKey));
            EnabledStateKey = enabledStateKey ?? throw new ArgumentNullException(nameof(enabledStateKey));
            TargetAssemblyKey = targetAssemblyKey ?? throw new ArgumentNullException(nameof(targetAssemblyKey));
            GameExePath = gameExePath ?? throw new ArgumentNullException(nameof(gameExePath));
        }

        public string Header { get; }
        public string PreloadKey { get; }
        public string EnabledStateKey { get; }
        public string TargetAssemblyKey { get; }
        public string GameExePath { get; }
    }
}