using ColossalFramework.Plugins;
using ColossalFramework.UI;
using ICities;
using PatchLoaderMod.Utils;
using System.Reflection;
using UnityEngine;
using Utils;

namespace PatchLoaderMod
{
    public class PatchLoaderMod : LoadingExtensionBase, IUserMod {
        private DoorstopManager _doorstopManager;
        private PluginManager.PluginInfo _pluginInfo;

        public string Name => "Patch Loader Mod";
        public string Description => "Automatically loads Patches implementing IPatch API.";

        public void OnEnabled() {
            _pluginInfo = PluginManager.instance.FindPluginInfo(Assembly.GetExecutingAssembly());

            var expectedTargetAssemblyPath = PathExtensions.Combine(
                _pluginInfo.modPath,
                "PatchLoader",
                "PatchLoader.dll"
            );
            _doorstopManager = DoorstopManager.Create(expectedTargetAssemblyPath);
            
            if (!_doorstopManager.IsInstalled())
            {
                _doorstopManager.Install();
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

        public void OnDisabled() {
            if (_doorstopManager.IsInstalled() && !_pluginInfo.isEnabled)
            {
                _doorstopManager.Disable();
            }

            if (_doorstopManager.RequiresRestart)
            {
                ShowRestartGameModal();
            }

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