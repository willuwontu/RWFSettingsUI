using HarmonyLib;
using System.Reflection;
namespace SettingsUI.Patches
{
    internal class PrivateRoomHandler_Patch
    {
        public static void Initialize(Harmony harmony)
        {
            MethodBase buildUIMethod = RWFSettingsUI.RWFPrivateRoomHandler.GetMethod("BuildUI", BindingFlags.Default | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod);
            harmony.Patch(buildUIMethod, postfix: new HarmonyMethod(typeof(PrivateRoomHandler_Patch), nameof(PrivateRoomHandler_Patch.OnBuildUI)));
        }

        [HarmonyPostfix]
        [HarmonyPatch("BuildUI")]
        static void OnBuildUI()
        {
            RWFSettingsUI.InjectUIElements();
        }

        //[HarmonyPostfix]
        //[HarmonyPatch("SomeMethod")]
        //static void MyMethodName()
        //{

        //}
    }
}