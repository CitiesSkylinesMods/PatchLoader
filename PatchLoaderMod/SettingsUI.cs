using System.Collections.Generic;
using System.Data;
using System.Text;
using ColossalFramework.UI;
using ICities;
using PatchLoader;
using PatchLoaderMod.Doorstop;
using PatchLoaderMod.DoorstopUpgrade;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PatchLoaderMod {
    public class SettingsUi {
        private const string colorOk = "#00FF00";
        private const string colorError = "#FF0000";
        private const string colorWarning = "#FF8C00";
        private UILabel _statusLabel;
        private UIButton _upgradeButton;
        private DoorstopManager _manager;
        private Utils.Logger _logger;

        public SettingsUi(Utils.Logger logger) {
            _logger = logger;
        }
        
        public void CreateUi(UIHelperBase helper, DoorstopManager manager) {
            _manager = manager;
            var uiHelper = helper as UIHelper;
            var panel = uiHelper.self as UIScrollablePanel;
            var label = panel.AddUIComponent<UILabel>();
            label.relativePosition = new Vector3(10, 0, 10);
            label.processMarkup = true;
            
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string,PatchStatus> patchStatus in PatchLoaderStatusInfo.Statuses) {
                builder.Append("   ")
                    .Append(patchStatus.Key)
                    .Append(": <color ").Append(patchStatus.Value.HasError ? colorError : colorOk).Append(">")
                    .Append(patchStatus.Value.HasError ? "Error (" : "OK")
                    .Append(patchStatus.Value.HasError ? patchStatus.Value.ErrorMessage : "")
                    .AppendLine(patchStatus.Value.HasError ? ") </color>" : "</color>");
            }
            
            label.text = "Statuses:\n\n" + (builder.Length > 0 ? builder.ToString() : "No patches processed.");

            _logger?.Info("Statuses:\n"+ builder);
            
            UIHelper loaderGroup = helper.AddGroup("Loader") as UIHelper;
            var uiPanel = loaderGroup.self as UIPanel;
            _statusLabel = uiPanel.AddUIComponent<UILabel>();
            _statusLabel.processMarkup = true;
            _upgradeButton = loaderGroup.AddButton("Upgrade", OnUpgrade) as UIButton;
            UpdateStatus();
        }

        private void OnUpgrade() {
            _upgradeButton.isInteractive = false;
            _upgradeButton.Disable();
            if (_manager.UpgradeManager.FollowToPhaseOne()) {
                PatchLoaderMod.ShowRestartGameModal("First phase of upgrade finished.\n" +
                                     "Game will be closed, please start again to continue process",
                                     "PatchLoaderMod - Upgrade(1/3)");
            } else {
                PatchLoaderMod.ShowExceptionModal("First phase of upgrade failed.\n" +
                                   "Check Mod options for more info about the problem",
                                   null,
                                   "PatchLoaderMod - Upgrade(1/3)");
            }
        }

        public void UpdateStatus() {
            bool isLatest = _manager.UpgradeManager.State == UpgradeState.Latest;
            bool isOutdated = _manager.UpgradeManager.State == UpgradeState.Outdated;
            var builder = new StringBuilder();
            builder.Append(": <color ").Append(isLatest ? colorOk: colorWarning).Append(">");
            builder.Append(isLatest ? "Latest" : "Outdated").Append("</color>");
            string sceneName = SceneManager.GetActiveScene().name;
            _upgradeButton.isVisible = isOutdated && (sceneName.Equals("MainMenu") || sceneName.Equals("IntroScreen")); 
            _statusLabel.prefix = "Patch Processor version: ";
            _statusLabel.text = builder.ToString();
            _statusLabel.suffix = isLatest
                ? " |  No actions required"
                : !isOutdated
                    ? $" |  Current State {_manager.UpgradeManager.State}"
                    : "";
        }
    }
}