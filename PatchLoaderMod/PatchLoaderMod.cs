using System.Reflection;
using ColossalFramework.Plugins;
using ICities;
using UnityEngine;

namespace PatchLoaderMod {
    public class PatchLoaderMod : IUserMod {
        public string Name => "Patch Loader Mod";
        public string Description => "Automatically Loads Patches implementing IPatch API";

        public static SettingsUi SettingsUi;
        internal static PluginManager.PluginInfo PluginInfo{ get; private set; } 
        public static bool RestartRequired { get; private set; }

        public void OnEnabled() {
            Debug.Log("PatchLoader enabled");
            PluginInfo = PluginManager.instance.FindPluginInfo(Assembly.GetExecutingAssembly());
            Manager.Instance.UpdateStatus();
        }

        public void OnDisabled() {
            Debug.Log("PatchLoader disabled");
        }

        // ReSharper disable once InconsistentNaming
        public void OnSettingsUI(UIHelperBase helper) {
            SettingsUi = new SettingsUi();
            SettingsUi.CreateUi(helper);
        }

        public static void RequireRestart() {
            RestartRequired = true;
        }
    }
}