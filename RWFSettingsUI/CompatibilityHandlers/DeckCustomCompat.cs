using System.Reflection;
using UnityEngine;
using BepInEx;
using UnboundLib;
using HarmonyLib;
using System.Collections;

namespace SettingsUI
{
    [BepInDependency("pykess.rounds.plugins.deckcustomization")]
    [BepInPlugin(ModId, ModName, Version)]
    internal class DeckCustomCompat : CompatibilityHandler
    {
        private const string ModId = "com.willuwontu.rounds.rwfsettingsui.compatibility.DeckCustomization";
        private const string ModName = "Rounds With Friends Settings UI - Deck Customization Compatibility Handler";
        public const string Version = "0.0.0"; // What version are we on (major.minor.patch)?

        public override string MenuName => "Deck Customization";

        public override void GUI(GameObject menu)
        {
            var instance = this.gameObject.GetComponent<DeckCustomization.DeckCustomization>();
            var newgui = typeof(DeckCustomization.DeckCustomization).GetMethod("SetupGUI", BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
            Unbound.Instance.StartCoroutine((IEnumerator)newgui.Invoke(instance, new object[] { menu }));
        }
    }
}
