namespace Patch.API.Helpers {
    public interface IGameState {
        bool? IsModEnabled(string workingPath);
    }
}