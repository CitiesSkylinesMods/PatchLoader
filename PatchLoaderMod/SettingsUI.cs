using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace PatchLoaderMod {
    public class SettingsUi {
        private UIButton _button;
        private UIHelper _helper;
        private UIScrollablePanel _panel;
        private UILabel _label;

        public void CreateUi(UIHelperBase helper) {
            _helper = helper as UIHelper;
            var group = helper.AddGroup("Patch Loader");
            _button = group.AddButton("<>", ButtonClicked) as UIButton;
            _panel = _helper.self as UIScrollablePanel;
            _label = _panel.AddUIComponent<UILabel>();
            _label.relativePosition = new Vector3(10, 0, 10);
            _label.text = "OK";
            UpdateStatus();
        }

        private void ButtonClicked() {
            if (Manager.Instance.Installed) {
                if (Manager.Instance.Enabled) {
                    Manager.Instance.Disable();
                } else {
                    Manager.Instance.Enable();
                }
            } else {
                Manager.Instance.Install();
            }

            UpdateStatus();
        }

        private void UpdateStatus() {
            if (Manager.Instance.Installed) {
                _button.text = Manager.Instance.Enabled ? "Disable" : "Enable";
                _label.text = Manager.Instance.Enabled ? "Loader installed and enabled" : "Loader installed but disabled";
                _label.textColor = Manager.Instance.Enabled ? Color.green : Color.yellow;
            } else {
                _label.text = "Loader not installed";
                _label.textColor = Color.white;
                _button.text = "Install";
            }

            if (PatchLoaderMod.RestartRequired) {
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