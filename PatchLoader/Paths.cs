using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Utils;

namespace PatchLoader
{
    public class Paths
    {
        //use static factory method 'Create()' instead
        private Paths(
            string workingPath,
            string loaderPath,
            string managedFolderPath,
            string filesModsPath,
            string appDataModsPath)
        {
            WorkingPath = workingPath ?? throw new ArgumentNullException(nameof(workingPath));
            LoaderPath = loaderPath ?? throw new ArgumentNullException(nameof(loaderPath));
            ManagedFolderPath = managedFolderPath ?? throw new ArgumentNullException(nameof(managedFolderPath));
            FilesModsPath = filesModsPath ?? throw new ArgumentNullException(nameof(filesModsPath));
            AppDataModsPath = appDataModsPath ?? throw new ArgumentNullException(nameof(appDataModsPath));
            SkipWorkshop = Environment.GetCommandLineArgs().Any(command => command.Equals("--noWorkshop"));
            DisableMods = Environment.GetCommandLineArgs().Any(command => command.Equals("--disableMods"));
            SetupLogsDirectoryPath();
        }

        public static Paths Create()
        {
            var workingPath = new DirectoryInfo(Environment.GetEnvironmentVariable("DOORSTOP_MANAGED_FOLDER_DIR")).Parent.Parent.FullName;
            var loaderPath = Path.GetDirectoryName(Environment.GetEnvironmentVariable("DOORSTOP_INVOKE_DLL_PATH"));
            var managedFolderPath = Environment.GetEnvironmentVariable("DOORSTOP_MANAGED_FOLDER_DIR");
            var filesModsPath = PathExtensions.Combine(workingPath, "Files", "Mods");
            var appDataModsPath = PathExtensions.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Colossal Order", "Cities_Skylines", "Addons", "Mods");

            return new Paths(
                workingPath,
                loaderPath,
                managedFolderPath,
                filesModsPath,
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

        public IEnumerable<string> AllModsFolders()
        {
            yield return AppDataModsPath;
            yield return FilesModsPath;
            if (!string.IsNullOrEmpty(WorkshopModsPath) && !SkipWorkshop)
            {
                yield return WorkshopModsPath;
            }
        }

        private void SetupLogsDirectoryPath() {
            string managedParent = new DirectoryInfo(ManagedFolderPath).Parent.FullName;
            if (!Directory.Exists(Path.Combine(managedParent, "Logs"))) {
                Directory.CreateDirectory(Path.Combine(managedParent, "Logs"));
            }

            LogsPath = Path.Combine(managedParent, "Logs");
        }

        public override string ToString()
        {
            return new StringBuilder()
                .Append("WorkingPath: ").AppendLine(WorkingPath)
                .Append("LoaderPath: ").AppendLine(LoaderPath)
                .Append("ManagedFolderPath: ").AppendLine(ManagedFolderPath)
                .Append("FilesModsPath: ").AppendLine(FilesModsPath)
                .Append("AppDataModsPath: ").AppendLine(AppDataModsPath)
                .Append("WorkshopModsPath: ").AppendLine(WorkshopModsPath)
                .Append("--noWorkshop flag set: ").AppendLine(SkipWorkshop.ToString())
                .Append("--disableMods flag set: ").AppendLine(DisableMods.ToString())
                .ToString();
        }
    }
}