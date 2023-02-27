using System.Reflection;
using UnityEngine;
using BepInEx;
using UnboundLib;
using HarmonyLib;
using SetRoundsPlugin;

namespace SettingsUI
{
    [BepInDependency("com.ascyst.rounds.setrounds")]
    [BepInPlugin(ModId, ModName, Version)]
    internal class SetRoundsCompat : CompatibilityHandler
    {
        private const string ModId = "com.willuwontu.rounds.rwfsettingsui.compatibility.setrounds";
        private const string ModName = "Rounds With Friends Settings UI - Set Rounds Compatibility Handler";
        public const string Version = "0.0.0"; // What version are we on (major.minor.patch)?

        public override string MenuName => "Set Rounds";

        public override void GUI(GameObject menu)
        {
            var instance = this.gameObject.GetComponent<SetRounds>();
            var newgui = typeof(SetRounds).GetMethod("NewGUI", BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
            newgui.Invoke(instance, new object[] { menu });
        }
    }
}
