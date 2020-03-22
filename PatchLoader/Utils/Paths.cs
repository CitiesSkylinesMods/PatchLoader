using System;
using System.IO;
using Utils;

namespace PatchLoader.Utils {
    public static class Paths {
        /// <summary>
        /// Path to the assembly that was invoked via Doorstop.
        /// </summary>
        public static string WorkingPath { get; private set; }
        
        /// <summary>
        /// Path to the loader.
        /// </summary>
        public static string LoaderPath { get; private set; }

        /// <summary>
        /// Full path to the game's "Managed" folder that contains all the game's managed assemblies
        /// </summary>
        public static string ManagedFolderPath { get; private set; }

        /// <summary>
        /// Path to game's directory Mods folder.
        /// </summary>
        public static string ModsPath { get; private set; }
        
        /// <summary>
        /// Path to user AppData mods folder.
        /// </summary>
        public static string AppDataModsPath { get; private set; }
        
        internal static void LoadVars() {
            LoaderPath = Path.GetDirectoryName(Environment.GetEnvironmentVariable("DOORSTOP_INVOKE_DLL_PATH"));
            
            WorkingPath = Directory.GetParent(Directory.GetParent(Environment.GetEnvironmentVariable("DOORSTOP_MANAGED_FOLDER_DIR")).FullName).FullName;
            
            ManagedFolderPath = Environment.GetEnvironmentVariable("DOORSTOP_MANAGED_FOLDER_DIR");

            ModsPath = PathExtensions.Combine(WorkingPath, "Files", "Mods");

            AppDataModsPath = PathExtensions.Combine(Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)).FullName, "Colossal Order", "Cities_Skylines", "Addons", "Mods");
        }

        public static void LogPaths() {
            Log._Debug($"\nLoaderPath: {LoaderPath}\nWorkingPath: {WorkingPath}\nManagedFolderPath: {ManagedFolderPath}\nModsPath: {ModsPath}\nAppDataModsPath: {AppDataModsPath}");
        }
    }
}