using System.Reflection;
using UnityEngine;
using BepInEx;
using UnboundLib;
using HarmonyLib;

namespace SettingsUI
{
    [BepInDependency("pykess.rounds.plugins.pickncards")]
    [BepInPlugin(ModId, ModName, Version)]
    internal class PickNCompat : CompatibilityHandler
    {
        private const string ModId = "com.willuwontu.rounds.rwfsettingsui.compatibility.pickn";
        private const string ModName = "Rounds With Friends Settings UI - Pick N Compatibility Handler";
        public const string Version = "0.0.0"; // What version are we on (major.minor.patch)?

        public override string MenuName => "Pick N Cards";

        public override void GUI(GameObject menu)
        {
            var instance = this.gameObject.GetComponent<PickNCards.PickNCards>();
            var newgui = typeof(PickNCards.PickNCards).GetMethod("NewGUI", BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
            newgui.Invoke(instance, new object[] { menu });
        }
    }
}
