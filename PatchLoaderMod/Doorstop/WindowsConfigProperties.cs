using System;

namespace PatchLoaderMod.Doorstop {
    internal class WindowsConfigProperties {
        public WindowsConfigProperties(string header, string enabledStateKey, string targetAssemblyKey) {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            EnabledStateKey = enabledStateKey ?? throw new ArgumentNullException(nameof(enabledStateKey));
            TargetAssemblyKey = targetAssemblyKey ?? throw new ArgumentNullException(nameof(targetAssemblyKey));
        }

        public string Header { get; }
        public string EnabledStateKey { get; }
        public string TargetAssemblyKey { get; }
    }
}