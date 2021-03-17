using System.IO;

namespace Patch.API.Helpers {
    public static class GameStateUtil {
        internal static IGameState Util { get; set; }
        
        public static bool? IsModEnabled(string workingPath) {
            return Util.IsModEnabled(workingPath);
        }
    }
}