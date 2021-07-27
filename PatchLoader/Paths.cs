using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Patch.API;
using Utils;

namespace PatchLoader {
    public class Paths : IPaths {
        //use static factory method 'Create()' instead
        private Paths(
            string workingPath,
            string loaderPath,
            string managedFolderPath,
            string filesModsPath,
            string appDataPath,
            string appDataModsPath) {
            WorkingPath = workingPath ?? throw new ArgumentNullException(nameof(workingPath));
            LoaderPath = loaderPath ?? throw new ArgumentNullException(nameof(loaderPath));
            ManagedFolderPath = managedFolderPath ?? throw new ArgumentNullException(nameof(managedFolderPath));
            FilesModsPath = filesModsPath ?? throw new ArgumentNullException(nameof(filesModsPath));
            AppDataPath = appDataPath ?? throw new ArgumentNullException(nameof(appDataPath));
            AppDataModsPath = appDataModsPath ?? throw new ArgumentNullException(nameof(appDataModsPath));
            SkipWorkshop = Environment.GetCommandLineArgs().Any(command => command.Equals("--noWorkshop"));
            DisableMods = Environment.GetCommandLineArgs().Any(command => command.Equals("--disableMods"));
            SetupLogsDirectoryPath();
        }

        public static Paths Create() {
            var workingPath = "";
            
            bool isMac = Environment.GetEnvironmentVariable("DOORSTOP_MANAGED_FOLDER_DIR").Contains("Cities.app");
            if (isMac) {
                workingPath = new DirectoryInfo(Environment.GetEnvironmentVariable("DOORSTOP_MANAGED_FOLDER_DIR")).Parent?.Parent?.Parent?.FullName;
            } else {
                workingPath = new DirectoryInfo(Environment.GetEnvironmentVariable("DOORSTOP_MANAGED_FOLDER_DIR")).Parent?.Parent?.FullName;
            }

            var loaderPath = Path.GetDirectoryName(Environment.GetEnvironmentVariable("DOORSTOP_INVOKE_DLL_PATH"));
            var managedFolderPath = Environment.GetEnvironmentVariable("DOORSTOP_MANAGED_FOLDER_DIR");
            var filesModsPath = "";
            if (isMac) {
                filesModsPath = PathExtensions.Combine(workingPath, "Resources", "Files", "Mods");
            } else {
                filesModsPath = PathExtensions.Combine(workingPath, "Files", "Mods");
            }
            
            var appDataPath = "";
            if (isMac) {
                appDataPath = PathExtensions.Combine(new DirectoryInfo(workingPath).Parent.Parent.Parent.Parent.Parent.Parent.FullName, "Colossal Order", "Cities_Skylines");
            } else {
                appDataPath = PathExtensions.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Colossal Order", "Cities_Skylines");
            }

            var appDataModsPath = PathExtensions.Combine(appDataPath, "Addons", "Mods");

            return new Paths(
                workingPath,
                loaderPath,
                managedFolderPath,
                filesModsPath,
                appDataPath,
                appDataModsPath);
        }

        /// <summary>
        /// Path to the assembly that was invoked via Doorstop.
        /// </summary>
        public string WorkingPath { get; }

        /// <summary>
        /// Path to the loader.
        /// </summary>
        public string LoaderPath { get; }

        /// <summary>
        /// Full path to the game's "Managed" folder that contains all the game's managed assemblies
        /// </summary>
        public string ManagedFolderPath { get; }

        /// <summary>
        /// Path to game's directory Mods folder.
        /// </summary>
        public string FilesModsPath { get; }

        /// <summary>
        /// Path to user AppData mods folder.
        /// </summary>
        public string AppDataModsPath { get; }

        /// <summary>
        /// Path to user AppData folder.
        /// </summary>
        public string AppDataPath { get; }

        /// <summary>
        /// Path to Workshop mods folder.
        /// </summary>
        public string WorkshopModsPath { get; set; } //can be null

        /// <summary>
        /// Path to Logs folder
        /// </summary>
        public string LogsPath { get; set; }

        /// <summary>
        /// Scan workshop directory mods
        /// </summary>
        public bool SkipWorkshop { get; }

        /// <summary>
        /// Scan mods
        /// </summary>
        public bool DisableMods { get; }

        public IEnumerable<string> AllModsFolders() {
            yield return AppDataModsPath;
            yield return FilesModsPath;
            if (!string.IsNullOrEmpty(WorkshopModsPath) && !SkipWorkshop) {
                yield return WorkshopModsPath;
            }
        }

        private void SetupLogsDirectoryPath() {
            string managedParent = "";
            if (ManagedFolderPath.Contains("Cities.app")) {
                //MacOS
                managedParent = new DirectoryInfo(ManagedFolderPath).Parent.Parent.Parent.FullName;
            } else {
                managedParent = new DirectoryInfo(ManagedFolderPath).Parent.FullName;
            }

            if (!Directory.Exists(Path.Combine(managedParent, "Logs"))) {
                Directory.CreateDirectory(Path.Combine(managedParent, "Logs"));
            }

            LogsPath = Path.Combine(managedParent, "Logs");
        }

        public override string ToString() {
            return new StringBuilder()
                .Append("WorkingPath: ").AppendLine(WorkingPath)
                .Append("LoaderPath: ").AppendLine(LoaderPath)
                .Append("ManagedFolderPath: ").AppendLine(ManagedFolderPath)
                .Append("LocalApplicationData: ").AppendLine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))
                .Append("FilesModsPath: ").AppendLine(FilesModsPath)
                .Append("AppDataModsPath: ").AppendLine(AppDataModsPath)
                .Append("WorkshopModsPath: ").AppendLine(WorkshopModsPath)
                .Append("Logs folder path: ").AppendLine(LogsPath)
                .Append("--noWorkshop flag set: ").AppendLine(SkipWorkshop.ToString())
                .Append("--disableMods flag set: ").AppendLine(DisableMods.ToString())
                .ToString();
        }
    }
}