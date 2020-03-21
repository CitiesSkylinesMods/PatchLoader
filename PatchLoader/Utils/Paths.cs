using System;
using System.IO;
using System.Linq;

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

            ModsPath = PathCombine(WorkingPath, "Files", "Mods");

            AppDataModsPath = PathCombine(Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)).FullName, "Colossal Order", "Cities_Skylines", "Addons", "Mods");
        }

        public static void LogPaths() {
            Log._Debug($"\nLoaderPath: {LoaderPath}\nWorkingPath: {WorkingPath}\nManagedFolderPath: {ManagedFolderPath}\nModsPath: {ModsPath}\nAppDataModsPath: {AppDataModsPath}");
        }

        /// <summary>
        /// Non performant Path.Combine(params string[] paths) implementation for .NET 3.5 and lower, to be able to handle multiple values.
        /// Do not use it in the gameloop, or see the performance crumble, and the few fps you had left burn.
        /// </summary>
        /// <param name="paths">Paths to be combined.</param>
        /// <returns>Returns the full path.</returns>
        public static string PathCombine(params string[] paths)
        {
            return paths
                .Where(p => p != null)
                .Aggregate((a, b) => Path.Combine(a, b));
        }
    }
}