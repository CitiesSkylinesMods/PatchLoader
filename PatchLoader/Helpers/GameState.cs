using System.IO;
using Patch.API.Helpers;

namespace PatchLoader.Helpers {
    internal class GameState: IGameState {
        public bool? IsModEnabled(string workingPath) {
            string name = Path.GetFileNameWithoutExtension(workingPath);
            string key = name + workingPath.GetHashCode().ToString() + ".enabled";
            SavedBool save = new SavedBool(key, "userGameState");
            return save.m_Exists ? save.value : (bool?) null;
        }
    }
}