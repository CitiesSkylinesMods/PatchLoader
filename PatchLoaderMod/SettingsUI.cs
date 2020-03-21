using ColossalFramework.UI;
using ICities;
using System;
using UnityEngine;

namespace PatchLoaderMod {
    public class SettingsUi {
        public void CreateUi(UIHelperBase helper) {
            var uiHelper = helper as UIHelper;
            var group = uiHelper.AddGroup("Patch Loader");
            var panel = uiHelper.self as UIScrollablePanel;
            var label = panel.AddUIComponent<UILabel>();
            label.relativePosition = new Vector3(10, 0, 10);
            label.text = "TODO: Show all applied patches.";
        }
    }
}