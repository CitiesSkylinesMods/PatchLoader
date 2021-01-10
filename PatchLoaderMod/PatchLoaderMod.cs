using ColossalFramework.IO;
using ColossalFramework.PlatformServices;
using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using System;
using System.IO;
using System.Reflection;
using PatchLoaderMod.Doorstop;
using PatchLoaderMod.DoorstopUpgrade;
using UnityEngine;
using Utils;

namespace PatchLoaderMod
{
    public class PatchLoaderMod : IUserMod
    {
        private DoorstopManager _doorstopManager;
        private PluginManager.PluginInfo _pluginInfo;
        private Utils.Logger _logger;
        private string _patchLoaderConfigFilePath;
        private ConfigManager<Config> _configManager;

        public string Name => "Patch Loader Mod";
        public string Description => "Automatically loads Patches implementing IPatch API.";
        private SettingsUi _settingsUi;
        public void OnEnabled()
        {
            _pluginInfo = PluginManager.instance.FindPluginInfo(Assembly.GetExecutingAssembly());
            EnsureLogsDirectoryCreated();
            _logger = new Utils.Logger(Path.Combine(Path.Combine(Application.dataPath, "Logs"), "PatchLoaderMod.log"));
            Version gameVersion = GameVersionCheck.GetVersion(BuildConfig.applicationVersion);
            _logger.Info($"Detected game version {gameVersion} from string [{BuildConfig.applicationVersion}]");
            if (!GameVersionCheck.IsGameVersionSupported(gameVersion)) {
                _logger.Error($"Game version is not supported! ({BuildConfig.applicationVersion})");
                ShowExceptionModal($"The '{Name}'\nGame version is not supported!\n\n" +
                                   "Mod will disable itself.\n" +
                                   "Update game to the latest version and try again or remove/unsubscribe mod.\n",
                                   () => {
                                       _pluginInfo.isEnabled = false;
                                   });
                return;
            }
            
            _patchLoaderConfigFilePath = Path.Combine(DataLocation.applicationBase, "PatchLoader.Config.xml");
            _configManager = new ConfigManager<Config>(_patchLoaderConfigFilePath, _logger);
            _configManager.EnsureCreated(Config.InitialValues());
            
            var expectedTargetAssemblyPath = PathExtensions.Combine(
                _pluginInfo.modPath,
                "PatchLoader",
                "PatchLoader.dll"
            );
            _doorstopManager = DoorstopManager.Create(expectedTargetAssemblyPath, _logger, _configManager);

            if (!_doorstopManager.PlatformSupported) {
                ShowExceptionModal($"The '{Name}'\nPlatform(MacOS) is not supported yet.\n\n" +
                                   "Mod will disable itself.\n" +
                                   "Follow FPS Booster and PatchLoader mod development to stay informed about changes.\n" +
                                   "Platform(MacOS) support will be added in one of the next major updates for PatchLoader mod.",
                    () => {
                        _pluginInfo.isEnabled = false;
                    });
                return;
            }
            SaveOrUpdateWorkshopPath();

            var config = _configManager.Load();
            if (config.UpgradeInProgress) {
                Debug.Log("PatchLoader enabled. Upgrade in progress");
                UpdateUpgradeStage();
                return;
            }

            if (!_doorstopManager.IsInstalled()) {
                _doorstopManager.Install();
                UpdateUpgradeStage();
            } else if (_doorstopManager.IsInstalled()
                       && _doorstopManager.UpgradeManager.State == UpgradeState.Latest
                       && _doorstopManager.CanEnable
                       && !_doorstopManager.IsEnabled()
            ) {
                _doorstopManager.Enable();
            } else {
                UpdateUpgradeStage();
            }

            if (_doorstopManager.RequiresRestart) {
                ShowRestartGameModal($"The '{Name}' was installed.\n{_doorstopManager.InstallMessage}");
            }
            Debug.Log("PatchLoader enabled");
        }

        private void UpdateUpgradeStage() {
            switch (_doorstopManager.UpgradeManager.State) {
                case UpgradeState.Latest:
                    _logger.Info($"PatchLoader is in the latest version, {(!_doorstopManager.RequiresRestart? "no action required": "restart required")}");
                    break;
                case UpgradeState.Outdated:
                    _logger.Info("PatchLoader is outdated. Leave decision to user.");
                    _doorstopManager.OpenGenericConfirmModal("Detected older version of PatchLoader\n" +
                                                             "Upgrade is necessary, involves multiple game restarts\n" +
                                                             "Do you want to upgrade?\n" +
                                                             "Cancel to skip upgrade(run manually from mod options)\n",
                                                             result => {
                                                                 _logger.Info($"User decided to [{(result == 0 ? "Skip upgrade!" : "Upgrade!")}]");
                                                                 if (result == 1) {
                                                                     if (_doorstopManager.UpgradeManager.FollowToPhaseOne()) {
                                                                         _logger.Info("UpdateUpgradeStage: Outdated -> Phase1 - Success");
                                                                         ShowRestartGameModal("First phase of upgrade finished.\n" +
                                                                                              "Game will be closed, please start again to continue process",
                                                                                              "PatchLoaderMod - Upgrade(1/3)");
                                                                     } else {
                                                                         _logger.Error("UpdateUpgradeStage: Outdated -> Phase1 - Failed");
                                                                         ShowExceptionModal("First phase of upgrade failed.\n" +
                                                                                            "Check Mod options for more info about the problem",
                                                                                            () => {
                                                                                                _logger.Info("Upgrade Failed!");
                                                                                                _settingsUi?.UpdateStatus();
                                                                                            },
                                                                                            "PatchLoaderMod - Upgrade(1/3)");
                                                                     }
                                                                 } else {
                                                                    _settingsUi?.UpdateStatus(); 
                                                                 }
                                                             });
                    break;
                case UpgradeState.Phase1:
                    _logger.Info("UpdateUpgradeStage: Phase1");
                    if (_doorstopManager.UpgradeManager.FollowToPhaseTwo()) {
                        _logger.Info("UpdateUpgradeStage: Phase1 - Success");
                        ShowRestartGameModal("Second phase of upgrade finished.\n" +
                                             "Game will be closed, please start again to continue process",
                                             "PatchLoaderMod - Upgrade(2/3)");
                    } else {
                        _logger.Error("UpdateUpgradeStage: Phase1 -> Phase2 - Failed");
                        ShowExceptionModal("Second phase of upgrade failed.\n" +
                                           "Check Mod options for more info about the problem",
                                           () => {
                                               _logger.Info("Upgrade Failed!");
                                               _settingsUi?.UpdateStatus();
                                           },
                                           "PatchLoaderMod - Upgrade(2/3)");
                    }
                    break;
                case UpgradeState.Phase2:
                    _logger.Info("UpdateUpgradeStage: Phase2");
                    if (_doorstopManager.UpgradeManager.FollowToPhaseThree()) {
                        _logger.Info("UpdateUpgradeStage: Phase2 - Success");
                        ShowExceptionModal("Third phase of upgrade finished.\n" +
                                             "No restart required.\n"+
                                             "Make sure that other mods which use PatchLoaderMod are enabled (e.g. FPS Booster)",
                                             () => {
                                                 _logger.Info("Upgrade Success!");
                                                 _doorstopManager.UpgradeManager.UpdateState();
                                                 _settingsUi?.UpdateStatus();
                                             },
                                             "PatchLoaderMod - Upgrade(3/3)");
                    } else {
                        _logger.Error("UpdateUpgradeStage: Phase2 -> Phase3 - Failed");
                        ShowExceptionModal("Third phase of upgrade failed.\n" +
                                           "Check Mod options for more info about the problem",
                                           () => {
                                               _logger.Info("Upgrade Failed!");
                                                 _doorstopManager.UpgradeManager.UpdateState();
                                               _settingsUi?.UpdateStatus();
                                           },
                                           "PatchLoaderMod - Upgrade(3/3)");
                    }
                    break;
                case UpgradeState.Error:
                    _logger.Info("UpdateUpgradeStage: Error");
                    _doorstopManager.UpgradeManager.HandleError();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_doorstopManager.UpgradeManager.State));
            }
            _settingsUi?.UpdateStatus();
        }

        private void SaveOrUpdateWorkshopPath()
        {
            _logger.Info("Test if workshop path update is required");
            if (_pluginInfo.publishedFileID == PublishedFileId.invalid)
            {
                _logger.Info("Mod not in workshop folder. Cannot detect workshop directory path. Config will not be updated.");
                return; //not a mod from the workshop folder, so we can give up here.
            }

            var workshopPath = Directory.GetParent(PlatformService.workshop.GetSubscribedItemPath(_pluginInfo.publishedFileID)).FullName;
            _logger.Info($"Workshop path: {workshopPath}");
            if (File.Exists(_patchLoaderConfigFilePath))
            {
                var config = _configManager.Load();
                if (config.WorkshopPath != workshopPath)
                {
                    _logger.Info($"Detected different workshop path! Old: [{config.WorkshopPath}] New: [{workshopPath}]. Saving...");
                    config.WorkshopPath = workshopPath;
                    _configManager.Save(config);
                }
            }
            else
            {
                _logger.Info($"Config does not exist. It should never happen! Creating new config file...");
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

            if (_doorstopManager != null) {
                if (_doorstopManager.IsInstalled() && !_pluginInfo.isEnabled) {
                    _doorstopManager.Disable();
                }

                if (_doorstopManager.RequiresRestart) {
                    ShowRestartGameModal($"The '{Name}' was uninstalled.\n{_doorstopManager.UninstallMessage}");
                }

                _doorstopManager = null;
            }
            _pluginInfo = null;
            _logger = null;
            _patchLoaderConfigFilePath = null;
            _configManager = null;

            Debug.Log("PatchLoader disabled");
        }

        // ReSharper disable once InconsistentNaming
        public void OnSettingsUI(UIHelperBase helper)
        {
            _settingsUi = new SettingsUi(_logger);
            _settingsUi.CreateUi(helper, _doorstopManager);
        }
        internal static  void ShowExceptionModal(string message, Action callback, string title = "PatchLoaderMod")
        {
            CoroutineHelper.WaitFor(
                () => UIView.library != null,
                success: () =>
                {
                    UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel", (comp, result) =>
                    {
                        callback?.Invoke();
                    }).SetMessage(title, message, false);
                },
                failure: () =>
                {
                    throw new Exception("PatchLoader could not open an important dialog. Something seems to be seriously broken. Please contact the author.");
                },
                stopPollingAfterInSec: 30f
            );
        }
        
        internal static void ShowRestartGameModal(string message, string title = "PatchLoaderMod")
        {
            CoroutineHelper.WaitFor(
                () => UIView.library != null,
                success: () =>
                {
                    UIView.library.ShowModal<ExceptionPanel>("ExceptionPanel", (comp, result) =>
                    {
                        Debug.Log("[PatchLoaderMod] Trying to quit the game");
                        // LoadingManager.instance.QuitApplication(); doesn't work sometimes
                        Application.Quit();
                    }).SetMessage(title, message, false);
                },
                failure: () =>
                {
                        Debug.Log("[PatchLoaderMod] Fail");
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