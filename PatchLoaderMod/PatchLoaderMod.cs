using ColossalFramework.IO;
using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using System;
using System.IO;
using System.Reflection;
using PatchLoaderMod.Doorstop;
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

            if (Application.platform == RuntimePlatform.OSXPlayer) {
                ShowExceptionModal($"The '{Name}'\nMacOS platform is not supported yet.\n\n" +
                                   "Mod will disable itself.\n" +
                                   "Follow FPS Booster and PatchLoader mod development to stay informed about changes.\n" +
                                   "MacOS support will be added in one of the next major updates for PatchLoader mod.",
                    () => {
                        _pluginInfo.isEnabled = false;
                    });
                return;
            }

            if (!_doorstopManager.IsInstalled())
            {
                _doorstopManager.Install();
                SaveOrUpdateWorkshopPath();
            }

            if (_doorstopManager.CanEnable && !_doorstopManager.IsEnabled())
            {
                _doorstopManager.Enable();
            }

            if (_doorstopManager.RequiresRestart)
            {
                ShowRestartGameModal($"The '{Name}' was installed.\n{_doorstopManager.InstallMessage}");
            }

            Debug.Log("PatchLoader enabled");
        }

        //TODO: maybe move into DoorstopManager, but cleanly
        private void SaveOrUpdateWorkshopPath()
        {
            if (_pluginInfo.publishedFileID == PublishedFileId.invalid)
            {
                _logger.Info("Mod not in workshop folder. Cannot detect workshop directory path. Config not saved.");
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
            if (Application.platform == RuntimePlatform.OSXPlayer) {
                Debug.Log("PatchLoader disabled");
                return;
            }

            if (_doorstopManager.IsInstalled() && !_pluginInfo.isEnabled)
            {
                _doorstopManager.Disable();
            }

            if (_doorstopManager.RequiresRestart)
            {
                ShowRestartGameModal($"The '{Name}' was uninstalled.\n{_doorstopManager.UninstallMessage}");
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
        private void ShowExceptionModal(string message, Action callback)
        {
            CoroutineHelper.WaitFor(
                () => UIView.library != null,
                success: () =>
                {
                    UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel", (comp, result) =>
                    {
                        callback.Invoke();
                    }).SetMessage("PatchLoaderMod", message, false);
                },
                failure: () =>
                {
                    throw new Exception("PatchLoader could not open an important dialog. Something seems to be seriously broken. Please contact the author.");
                },
                stopPollingAfterInSec: 30f
            );
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