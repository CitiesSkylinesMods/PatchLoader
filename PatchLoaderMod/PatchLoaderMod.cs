using ColossalFramework.IO;
using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;
using ICities;
using System.IO;
using System.Reflection;
using UnityEngine;
using Utils;

namespace PatchLoaderMod
{
    public class PatchLoaderMod : LoadingExtensionBase, IUserMod {
        private DoorstopManager _doorstopManager;
        private PluginManager.PluginInfo _pluginInfo;
        private Utils.Logger _logger;
        private string _patchLoderConfigFilePath;
        private ConfigManager<Config> _configManager;

        public string Name => "Patch Loader Mod";
        public string Description => "Automatically loads Patches implementing IPatch API.";

        public void OnEnabled() {
            _pluginInfo = PluginManager.instance.FindPluginInfo(Assembly.GetExecutingAssembly());
            _logger = new Utils.Logger(Path.Combine(Application.dataPath, "PatchLoaderMod.log"));
            _patchLoderConfigFilePath = Path.Combine(DataLocation.applicationBase, "PatchLoader.Config.xml");
            _configManager = new ConfigManager<Config>(_patchLoderConfigFilePath, _logger);

            var expectedTargetAssemblyPath = PathExtensions.Combine(
                _pluginInfo.modPath,
                "PatchLoader",
                "PatchLoader.dll"
            );
            _doorstopManager = DoorstopManager.Create(expectedTargetAssemblyPath, _logger);
            
            if (!_doorstopManager.IsInstalled())
            {
                _doorstopManager.Install();
                SaveOrUpdateWorkshopPath();

            }

            if (!_doorstopManager.IsEnabled())
            {
                _doorstopManager.Enable();
            }

            if (_doorstopManager.RequiresRestart)
            {
                ShowRestartGameModal();
            }

            Debug.Log("PatchLoader enabled");
        }

        //TODO: maybe move into DoorstopManager, but cleanly
        private void SaveOrUpdateWorkshopPath()
        {
            if (_pluginInfo.publishedFileID == PublishedFileId.invalid)
            {
                return; //not a mod from the workshop folder, so we can give up here.
            }

            if (File.Exists(_patchLoderConfigFilePath))
            {
                var config = _configManager.Load();

                if (config.WorkshopPath != PlatformService.workshop.GetSubscribedItemPath(_pluginInfo.publishedFileID))
                {
                    config.WorkshopPath = DataLocation.addonsPath;
                    _configManager.Save(config);
                }
            }
            else
            {
                var config = new Config() { WorkshopPath = DataLocation.addonsPath };
                _configManager.Save(config);
            }
        }

        public void OnDisabled() {
            if (_doorstopManager.IsInstalled() && !_pluginInfo.isEnabled)
            {
                _doorstopManager.Disable();
            }

            if (_doorstopManager.RequiresRestart)
            {
                ShowRestartGameModal();
            }

            _doorstopManager = null;
            _pluginInfo = null;
            _logger = null;
            _patchLoderConfigFilePath = null;
            _configManager = null;

            Debug.Log("PatchLoader disabled");
        }

        // ReSharper disable once InconsistentNaming
        public void OnSettingsUI(UIHelperBase helper)
        {
            new SettingsUi()
                .CreateUi(helper);
        }

        //TODO: change to nice dialog
        private void ShowRestartGameModal()
        {
            Application.Quit(); 
        }
    }
}