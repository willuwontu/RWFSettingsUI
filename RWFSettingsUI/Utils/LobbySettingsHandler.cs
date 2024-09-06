using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnboundLib.Utils.UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SettingsUI.Utils
{
    internal class LobbySettingsHandler : MonoBehaviourPunCallbacks
    {
        public static LobbySettingsHandler instance;

        internal GameObject settingsButton;
        internal TextMeshProUGUI gameSettingsText;

        private static List<ModMenu> modMenus = new List<ModMenu>();
        //private static Dictionary<string, ModMenu> modMenus = new Dictionary<string, ModMenu>();
        private Dictionary<ModMenu, GameObject> menuButtons = new Dictionary<ModMenu, GameObject>();
        private Dictionary<GameObject, bool> createdMenus = new Dictionary<GameObject, bool>();

        private GameObject _linksUi;
        private GameObject _pingUi;
        private GameObject _codeUi;
        private GameObject _timerUi;
        private GameObject _roundUi;

        private Transform ButtonContainer { get { return transform.Find("Group/Grid/Scroll View/Viewport/Content"); } }

        private bool IsOpen { get { return this?.settingsButton?.transform?.parent?.gameObject?.activeSelf ?? false; } }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            this.gameSettingsText.color = (PhotonNetwork.CurrentRoom != null) ? RWFSettingsUI.enabledTextColor : RWFSettingsUI.disabledTextColor;
        }

        public override void OnJoinedRoom()
        {
            if (!IsOpen) 
            { 
                return; 
            }

            this.gameSettingsText.color = RWFSettingsUI.enabledTextColor;
            this.settingsButton.GetComponent<Button>().enabled = true;
        }

        public void Open()
        {
            if (!_linksUi) _linksUi = GameObject.Find("Links(Clone)");
            if (!_pingUi) _pingUi = GameObject.Find("UIHolder");
            if (!_codeUi) _codeUi = GameObject.Find("LobbyImprovementsBG");
            if (!_timerUi) _timerUi = GameObject.Find("TimerLobbyUI(Clone)");
            if (!_roundUi) _roundUi = GameObject.Find("RoundCounterSmall");

            if (_linksUi) _linksUi.gameObject.SetActive(false);
            if (_pingUi) _pingUi.gameObject.SetActive(false);
            if (_codeUi) _codeUi.gameObject.SetActive(false);
            if (_timerUi) _timerUi.gameObject.SetActive(false);
            if (_roundUi) _roundUi.gameObject.SetActive(false);

            foreach (var menu in modMenus.OrderBy(modMenu => modMenu.menuName))
            {
                if (!menuButtons.ContainsKey(menu))
                {
                    var mmenu = MenuHandler.CreateMenu(menu.menuName, menu.enterMenuAction, this.gameObject, out GameObject menuButton, 60, true, false, this.transform.parent.gameObject);

                    createdMenus.Add(menuButton, menu.hostOnly);
                    menuButtons.Add(menu, menuButton);

                    try
                    {
                        menu.guiAction(mmenu);
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError($"Exception thrown when attempting to build menu '{menu.menuName}', see log below for details.");
                        UnityEngine.Debug.LogException(e);
                    } 
                }
            }

            foreach (var kvp in createdMenus)
            {
                bool flag = ((!kvp.Value) || PhotonNetwork.OfflineMode || PhotonNetwork.IsMasterClient);

                kvp.Key.SetActive(flag);
                kvp.Key.GetComponent<Button>().enabled = flag;
            }
        }

        public void Close()
        {
            if (_linksUi) _linksUi.gameObject.SetActive(true);
            if (_pingUi) _pingUi.gameObject.SetActive(true);
            if (_codeUi) _codeUi.gameObject.SetActive(true);
            if (_timerUi) _timerUi.gameObject.SetActive(true);
            if (_roundUi) _roundUi.gameObject.SetActive(true);
        }

        public void DoReturnToLobbyActions()
        {
            foreach (var kvp in menuButtons)
            {
                try
                {
                    kvp.Key.returnToLobbyAction?.Invoke();
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"The menu {kvp.Key.menuName} threw an exception when executing it's 'Return To Lobby Action'. See Log Below.");
                    UnityEngine.Debug.LogException(e);
                }
            }
        }

        public static void RegisterMenu(string name, Action<GameObject> guiAction, bool hostOnly = true, UnityAction enterMenuAction = null, UnityAction returnToLobbyAction = null)
        {
            //if (modMenus.ContainsKey(name))
            //{
            //    try
            //    {
            //        throw new ArgumentException($"A menu named '{name}' has already been registered. Menu Names must be unique.");
            //    }
            //    catch (Exception e)
            //    {
            //        UnityEngine.Debug.LogException(e);
            //    }

            //    return;
            //}

            //modMenus[name] = new ModMenu(name, buttonAction, guiAction, hostOnly);

            modMenus.Add(new ModMenu(name, guiAction, hostOnly, enterMenuAction, returnToLobbyAction));
        }

        internal class ModMenu
        {
            public string menuName;
            public UnityAction enterMenuAction;
            public UnityAction returnToLobbyAction;
            public Action<GameObject> guiAction;
            public bool hostOnly;

            public ModMenu(string menuName, Action<GameObject> guiAction, bool hostOnly = true, UnityAction enterMenuAction = null, UnityAction returnToLobbyAction = null)
            {
                this.menuName = menuName;
                this.enterMenuAction = enterMenuAction;
                this.guiAction = guiAction;
                this.hostOnly = hostOnly;
                this.returnToLobbyAction = returnToLobbyAction;
            }
        }
    }
}
