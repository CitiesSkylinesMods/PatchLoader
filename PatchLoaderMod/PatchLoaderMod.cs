using ColossalFramework.IO;
using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using Utils;

namespace PatchLoaderMod
{
    public class PatchLoaderMod : LoadingExtensionBase, IUserMod
    {
        private DoorstopManager _doorstopManager;
        private PluginManager.PluginInfo _pluginInfo;
        private Utils.Logger _logger;
        private string _patchLoaderConfigFilePath;
        private ConfigManager<Config> _configManager;

        public string Name => "Patch Loader Mod";
        public string Description => "Automatically loads Patches implementing IPatch API.";

        public void OnEnabled()
        {
            _pluginInfo = PluginManager.instance.FindPluginInfo(Assembly.GetExecutingAssembly());
            EnsureLogsDirectoryCreated();
            _logger = new Utils.Logger(Path.Combine(Path.Combine(Application.dataPath, "Logs"), "PatchLoaderMod.log"));
            _patchLoaderConfigFilePath = Path.Combine(DataLocation.applicationBase, "PatchLoader.Config.xml");
            _configManager = new ConfigManager<Config>(_patchLoaderConfigFilePath, _logger);

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
                ShowRestartGameModal($"The '{Name}' was installed and the game must be restarted in order to initialize new patches.");
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

            var workshopPath = Directory.GetParent(PlatformService.workshop.GetSubscribedItemPath(_pluginInfo.publishedFileID)).FullName;
            if (File.Exists(_patchLoaderConfigFilePath))
            {
                var config = _configManager.Load();
                if (config.WorkshopPath != workshopPath)
                {
                    config.WorkshopPath = workshopPath;
                    _configManager.Save(config);
                }
            }
            else
            {
                var config = new Config() { WorkshopPath = workshopPath };
                _configManager.Save(config);
            }
        }

        public void OnDisabled()
        {
            if (_doorstopManager.IsInstalled() && !_pluginInfo.isEnabled)
            {
                _doorstopManager.Disable();
            }

            if (_doorstopManager.RequiresRestart)
            {
                ShowRestartGameModal($"The '{Name}' was uninstalled and the game must be restarted in order to restore it's original state.");
            }

            _doorstopManager = null;
            _pluginInfo = null;
            _logger = null;
            _patchLoaderConfigFilePath = null;
            _configManager = null;

            Debug.Log("PatchLoader disabled");
        }

        // ReSharper disable once InconsistentNaming
        public void OnSettingsUI(UIHelperBase helper)
        {
            new SettingsUi()
                .CreateUi(helper);
        }

        private void ShowRestartGameModal(string message)
        {
            CoroutineHelper.WaitFor(
                () => UIView.library != null,
                success: () =>
                {
                    UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel", (comp, result) =>
                    {
                        LoadingManager.instance.QuitApplication();
                    }).SetMessage("PatchLoaderMod", message, false);
                },
                failure: () =>
                {
                    throw new Exception("PatchLoader could not open an important dialog. Something seems to be seriously broken. Please contact the author.");
                },
                stopPollingAfterInSec: 30f
            );
        }

        private void EnsureLogsDirectoryCreated() {
            if (!Directory.Exists(Path.Combine(Application.dataPath, "Logs"))) {
                Directory.CreateDirectory(Path.Combine(Application.dataPath, "Logs"));
            }
        }
    }
}