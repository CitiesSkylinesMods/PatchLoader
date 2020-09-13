using System.Collections.Generic;
using System.Text;
using ColossalFramework.UI;
using ICities;
using PatchLoader;
using UnityEngine;

namespace PatchLoaderMod {
    public class SettingsUi {
        private const string colorOk = "#00FF00";
        private const string colorError = "#FF0000";
        
        public void CreateUi(UIHelperBase helper) {
            var uiHelper = helper as UIHelper;
            var panel = uiHelper.self as UIScrollablePanel;
            var label = panel.AddUIComponent<UILabel>();
            label.relativePosition = new Vector3(10, 0, 10);
            label.processMarkup = true;
            
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string,PatchStatus> patchStatus in PatchLoaderStatusInfo.Statuses) {
                builder.AppendLine($"   {patchStatus.Key} <color {(patchStatus.Value.HasError ? colorError : colorOk)}>{(patchStatus.Value.HasError? "Error": "OK")}</color>");
            }
            
            label.text = "Processed patches:\n\n" + (builder.Length > 0 ? builder.ToString() : "Nothing found.");
        }
    }
}