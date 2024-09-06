using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using BepInEx;
using UnboundLib;
using UnboundLib.Utils.UI;
using UnboundLib.Networking;
using HarmonyLib;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using SettingsUI.Patches;
using SettingsUI.Utils;
using UnityEngine.Events;
using RWF;

namespace SettingsUI
{
    [BepInDependency("com.willis.rounds.unbound")]
    [BepInDependency("io.olavim.rounds.rwf")]
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class RWFSettingsUI : BaseUnityPlugin
    {
        private const string ModId = "com.willuwontu.rounds.rwfsettingsui";
        private const string ModName = "Rounds With Friends Settings UI";
        private const string ModConfigName = "RWFSettingsUI";
        public const string Version = "0.0.1"; // What version are we on (major.minor.patch)?

        internal const string ModInitials = "RWFSUI";

        private static Type _RWFPrivateRoomHandler = null;
        internal static Type RWFPrivateRoomHandler {
            get {
                if (_RWFPrivateRoomHandler == null)
                {
                    var types = AccessTools.GetTypesFromAssembly(typeof(RWF.RWFMod).Assembly);

                    foreach (var type in types)
                    {
                        if (type.Name == "PrivateRoomHandler")
                        {
                            _RWFPrivateRoomHandler = type;
                        }
                    }
                }

                return _RWFPrivateRoomHandler;
            }
        }
        internal static Color disabledTextColor { get { var _ = RWFPrivateRoomHandler; return (Color)RWFPrivateRoomHandler.GetField("disabledTextColor", BindingFlags.Default | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetField).GetValue(null); } }
        internal static Color enabledTextColor { get { var _ = RWFPrivateRoomHandler; return (Color)RWFPrivateRoomHandler.GetField("enabledTextColor", BindingFlags.Default | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.GetField).GetValue(null); } }

        internal static RWFSettingsUI instance { get; private set; }

        void Awake()
        {
            instance = this;

            var harmony = new Harmony(ModId);

            PrivateRoomHandler_Patch.Initialize(harmony);
        }
        void Start()
        {
            Unbound.RegisterCredits(ModName, new string[] { "willuwontu" }, new string[] { "github", "Ko-Fi" }, new string[] { "https://github.com/willuwontu/wills-wacky-cards", "https://ko-fi.com/willuwontu" });
        }

        internal static void UnreadyAll()
        {
            var _ = RWFPrivateRoomHandler;
            var privateRoomInstance = RWFPrivateRoomHandler.GetField("instance", BindingFlags.Default | BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField).GetValue(null);
            var UnreadyAllPlayers = RWFSettingsUI.RWFPrivateRoomHandler.GetMethod("UnreadyAllPlayers", BindingFlags.Default | BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod);
            UnreadyAllPlayers.Invoke(privateRoomInstance, new object[] { });
        }

        internal static void ResyncLobby()
        {
            if (!(PhotonNetwork.IsMasterClient || PhotonNetwork.OfflineMode))
            {
                return;
            }

            Type SyncModClients = null;
            var types = AccessTools.GetTypesFromAssembly(typeof(Unbound).Assembly);

            foreach (var type in types)
            {
                if (type.Name == "SyncModClients")
                {
                    SyncModClients = type;
                }
            }

            NetworkingManager.RPC(SyncModClients, "SyncLobby", new object[] { });
        }

        internal static void RequestJoinedRoom()
        {
            if (!PhotonNetwork.IsMasterClient) { return; }
            NetworkingManager.RPC(typeof(RWFSettingsUI), nameof(RWFSettingsUI.URPC_JoinedRoom), new object[] { });
        }

        [UnboundRPC]
        internal static void URPC_JoinedRoom()
        {
            NetworkEventCallbacks[] callbacks = Unbound.Instance.GetComponents<NetworkEventCallbacks>();

            for (int i = callbacks.Length - 1; i >= 0; i--)
            {
                callbacks[i].OnJoinedRoom();
            }
        }

        [UnboundRPC]
        internal static void URPC_UnreadySelf()
        {
            LobbyCharacter[] localCharacters = PhotonNetwork.LocalPlayer.GetProperty<LobbyCharacter[]>("players");

            for (int i = 0; i < localCharacters.Count(); i++)
            {
                localCharacters[i]?.SetReady(false);
            }
            PhotonNetwork.LocalPlayer.SetProperty("players", localCharacters);
        }

        internal static void BackActions()
        {
            RWFSettingsUI.instance.StartCoroutine(IDoBackActions());
        }

        internal static IEnumerator IDoBackActions()
        {
            //UnreadyAll();
            RequestJoinedRoom();
            ResyncLobby();

            int syncTime = UnityEngine.Mathf.Clamp(5 * ((int)PhotonNetwork.LocalPlayer.CustomProperties["Ping"] + PhotonNetwork.CurrentRoom.Players.Select(kv => (int)kv.Value.CustomProperties["Ping"]).Max()), 200, int.MaxValue);

            yield return new WaitForSecondsRealtime(syncTime/1000f);

            UnreadyAll();
            //RequestJoinedRoom();

            syncTime = UnityEngine.Mathf.Clamp(5 * ((int)PhotonNetwork.LocalPlayer.CustomProperties["Ping"] + PhotonNetwork.CurrentRoom.Players.Select(kv => (int)kv.Value.CustomProperties["Ping"]).Max()), 200, int.MaxValue);

            yield return new WaitForSecondsRealtime(syncTime / 1000f);

            UnreadyAll();

            yield break;
        }

        internal static void InjectUIElements()
        {
            var uiGo = GameObject.Find("/Game/UI");
            var gameGo = uiGo.transform.Find("UI_Game").Find("Canvas").gameObject;
            var privateRoomGo = gameGo.transform.Find("PrivateRoom");
            var buttonGroupGo = privateRoomGo.transform.Find("Main").Find("Group");

            if (!buttonGroupGo.Find("Mod Settings"))
            {
                var settingMenuGo = MenuHandler.CreateMenu("Mod Settings", () => { }, buttonGroupGo.transform.parent.gameObject, size: 60, parentForMenu: privateRoomGo.gameObject, siblingIndex: buttonGroupGo.childCount - 1);
                var settingsHandler = settingMenuGo.AddComponent<LobbySettingsHandler>();

                var settingsButtonGo = buttonGroupGo.Find("Mod Settings");

                settingsHandler.settingsButton = settingsButtonGo.gameObject;

                settingsButtonGo.GetComponent<LayoutElement>().minHeight = 92f;
                settingsButtonGo.GetComponent<LayoutElement>().minWidth = 5000;
                settingsButtonGo.GetComponent<ListMenuButton>().setBarHeight = 92f;
                settingsButtonGo.GetComponent<Button>().enabled = false;
                settingsButtonGo.GetComponent<Button>().onClick.AddListener(settingsHandler.Open);
                settingsButtonGo.GetComponent<Button>().onClick.AddListener(URPC_UnreadySelf);
                settingsHandler.gameSettingsText = settingsButtonGo.GetComponentInChildren<TextMeshProUGUI>();
                settingsHandler.gameSettingsText.enableAutoSizing = false;
                settingsHandler.gameSettingsText.fontSize = 60;
                settingsHandler.gameSettingsText.color = (PhotonNetwork.CurrentRoom != null) ? enabledTextColor : disabledTextColor;


                var backGo = settingMenuGo.transform.Find("Group").Find("Back");
                var backButton = backGo.GetComponent<Button>();
                backButton.onClick.AddListener(settingsHandler.Close);
                backButton.onClick.AddListener(BackActions);
                backButton.onClick.AddListener(settingsHandler.DoReturnToLobbyActions);
            }
        }

        public static void RegisterMenu(string name, Action<GameObject> guiAction, bool hostOnly = true, UnityAction enterMenuAction = null, UnityAction returnToLobbyAction = null)
        {
            LobbySettingsHandler.RegisterMenu(name, guiAction, hostOnly, enterMenuAction, returnToLobbyAction);
        }
    }
}
