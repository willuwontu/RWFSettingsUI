using UnityEngine;
using BepInEx;

namespace SettingsUI
{
    [BepInProcess("Rounds.exe")]
    [BepInDependency("com.willis.rounds.unbound")]
    [BepInDependency("com.willuwontu.rounds.rwfsettingsui")]
    public abstract class CompatibilityHandler : BaseUnityPlugin
    {
        public virtual void Start()
        {
            SettingsUI.RWFSettingsUI.RegisterMenu(MenuName, GUI, HostOnly, EnterMenuAction, ReturnToLobbyAction);
        }

        public abstract string MenuName { get; }

        public virtual bool HostOnly => true;

        public virtual void EnterMenuAction()
        {

        }

        public abstract void GUI(GameObject menu);

        public virtual void ReturnToLobbyAction()
        {

        }
    }
}
