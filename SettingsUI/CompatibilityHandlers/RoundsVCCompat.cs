using System.Reflection;
using UnityEngine;
using BepInEx;
using UnboundLib;
using HarmonyLib;
using DeckCustomization;
using System.Collections;

namespace SettingsUI
{
    [BepInDependency("pykess-and-root.plugins.rounds.vc")]
    [BepInPlugin(ModId, ModName, Version)]
    internal class RoundsVCCompat : CompatibilityHandler
    {
        private const string ModId = "com.willuwontu.rounds.rwfsettingsui.compatibility.roundsvc";
        private const string ModName = "Rounds With Friends Settings UI - Rounds VC Compatibility Handler";
        public const string Version = "0.0.0"; // What version are we on (major.minor.patch)?

        public override string MenuName => "Rounds VC";

        public override bool HostOnly => false;

        public override void EnterMenuAction()
        {
            var instance = this.gameObject.GetComponent<RoundsVC.RoundsVC>();
            FieldInfo optionsMenuDemoCO = typeof(RoundsVC.RoundsVC).GetField("OptionsMenuDemoCO", BindingFlags.Default | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField);
            Coroutine currentCO = (Coroutine)optionsMenuDemoCO.GetValue(null);
            if (currentCO != null ) 
            { 
                instance.StopCoroutine(currentCO);
            }
            var demoUI = typeof(RoundsVC.RoundsVC).GetMethod("DemoUI", BindingFlags.Default | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
            optionsMenuDemoCO.SetValue(null, instance.StartCoroutine((IEnumerator)demoUI.Invoke(null, new object[] { })));
        }

        public override void GUI(GameObject menu)
        {
            var newgui = typeof(RoundsVC.RoundsVC).GetMethod("GUI", BindingFlags.Default | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
            newgui.Invoke(null, new object[] { menu });
        }
    }
}
