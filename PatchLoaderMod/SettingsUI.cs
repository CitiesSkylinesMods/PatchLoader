using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace PatchLoaderMod {
    public class SettingsUi {
        private readonly PatchLoaderMod _mod;
        
        private UIButton _button;
        private UIHelper _helper;
        private UIScrollablePanel _panel;
        private UILabel _label;

        public SettingsUi(PatchLoaderMod mod)
        {
            _mod = mod;
        }

        public void CreateUi(UIHelperBase helper) {
            _helper = helper as UIHelper;
            var group = helper.AddGroup("Patch Loader");
            _button = group.AddButton("<>", EnabledButtonClicked) as UIButton;
            _panel = _helper.self as UIScrollablePanel;
            _label = _panel.AddUIComponent<UILabel>();
            _label.relativePosition = new Vector3(10, 0, 10);
            _label.text = "OK";
            UpdateStatus();
        }

        private void EnabledButtonClicked() {
            if (_mod.CheckIfInstalled()) {
                _mod.ToggleEnabled();
            } else {
                _mod.Install();
            }

            UpdateStatus();
        }

        private void UpdateStatus() {
            if (_mod.CheckIfInstalled()) {
                _button.text = _mod.Enabled ? "Disable" : "Enable";
                _label.text = _mod.Enabled ? "Loader installed and enabled" : "Loader installed but disabled";
                _label.textColor = _mod.Enabled ? Color.green : Color.yellow;
            } else {
                _label.text = "Loader not installed";
                _label.textColor = Color.white;
                _button.text = "Install";
            }

            if (_mod.RestartRequired) {
                _label.text = "To apply changes game restart is required";
                _label.textColor = Color.cyan;
                _button.textColor = Color.gray;
                _button.isInteractive = false;
            }
        }

        public void InMenu() {
            _button.textColor = Color.white;
            _button.isInteractive = true;
        }

        public void InGame() {
            _button.textColor = Color.gray;
            _button.isInteractive = false;
        }
    }
}