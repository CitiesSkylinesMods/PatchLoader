using System.Collections.Generic;

namespace PatchLoader {
    public static class PatchLoaderStatusInfo {

        internal static Dictionary<string, PatchStatus> Statuses { get; } = new Dictionary<string, PatchStatus>();

        public static PatchStatus GetPatchStatus(string assemblyName) {
            return Statuses.TryGetValue(assemblyName, out PatchStatus status) ? status : null;
        }
    }
}