namespace Patch.API {
    public interface IPaths {
        /// <summary>
        /// Path of internal loader executable, main game directory
        /// </summary>
        string WorkingPath { get; }

        /// <summary>
        /// Path to the PatchLoader assembly.
        /// </summary>
        string LoaderPath { get; }

        /// <summary>
        /// Full path to the game's "Managed" folder that contains all the game's managed assemblies
        /// </summary>
        string ManagedFolderPath { get; }

        /// <summary>
        /// Path to game's directory Mods folder
        /// </summary>
        string FilesModsPath { get; }
        
        /// <summary>
        /// Path to user AppData folder.
        /// </summary>
        string AppDataPath { get; }

        /// <summary>
        /// Path to user AppData mods folder.
        /// </summary>
        string AppDataModsPath { get; }

        /// <summary>
        /// Path to Workshop mods folder, only if PatchLoader was running from workshop directory
        /// or helper Workshop path was set during mod installation
        /// </summary>
        string WorkshopModsPath { get; } //can be null

        /// <summary>
        /// Path to the Logs folder
        /// </summary>
        string LogsPath { get; }
    }
}