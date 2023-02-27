# Rounds With Friends Settings UI

Provides functionality for adding mod menus to the RWF lobby.

* [Change Log](#change-log)
* [Usage](#usage)
  * [Hard Dependency](#hard-dependency)
  * [Soft Dependency](#soft-dependency)
* [Documentation](#documentation)
* [FAQ](#faq)

---

<details>
<summary><h2>Change Log</h2></summary>

### v 0.0.1
- Public Release
</details>

---

## Usage

While having the ability to change settings within a lobby is nice, a mod may not necessarily want to depend on having Rounds With Friends installed, let alone require having this mod installed as well. With this in mind, here's the two ways to depend on the mod.

For either method used, you need to add this mod as a reference/dependency inside of Visual Studio.

### Hard Dependency
A hard dependency requires that you add `[BepInDependency("com.willuwontu.rounds.rwfsettingsui")]` to the `BaseUnityPlugin` of your mod.

Additionally, you need to add this mod to your thunderstore manifest.

This will allow you to make use of [`RWFSettingsUI.RegisterMenu()`](#registermenu) within your mod without any worries of crashes due to having code not loaded in.

#### Examples
An example of how to call this function can be seen in [Will's Wacky Game Modes](https://github.com/willuwontu/WillsWackyGamemodes/blob/main/WillsWackyGamemodes/WillsWackyGameModes.cs#L80).
```CSHARP
SettingsUI.RWFSettingsUI.RegisterMenu(ModName, NewGUI);
```

This registers a menu with the following parameters:
- The menu's name is "Will's Wacky GameModes" (`ModName`)
- It sets the action used to create the contents of the menu as the [`NewGUI`](https://github.com/willuwontu/WillsWackyGamemodes/blob/main/WillsWackyGamemodes/WillsWackyGameModes.cs#L217) method.

### Soft Dependency
A soft dependency makes use of how `BaseUnityPlugin`'s work to load in another mod from the same assembly.

To do so, it requires that you create a class that inherits from the [`CompatibilityHandler`](#compatibilityhandler) class, and add the `[BepInPlugin]` and `[BepInDependency]` attributes to it. Note, the class already has a `[BepInDependency("com.willuwontu.rounds.rwfsettingsui")]` attribute, so you only need to add a `[BepInDependency]` for your mod.

#### Example
Examples of how to use the class can be seen in the [CompatibilityHandlers](https://github.com/willuwontu/RWFSettingsUI/tree/main/SettingsUI/CompatibilityHandlers) folder. In this case, the Pick N' Cards compatibility handler looks like this:
```CSHARP
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
```

The `[BepInPlugin]` and `[BepInDependency]` attributes declare the class as a plugin that should be loaded in, and let it know that it needs to wait for pick n cards to be loaded in first. Note, the ModId needs to be unique to the compatibility handler.

We then declare the name for the menu by overriding the [`MenuName`](#menuname) property of the class.

Finally, we declare the [`GUI`](#gui) method for the class. In this case, since we're attempting to use a private method outside of our assembly, we need to use `System.Reflection` in order to access it. Normally though, since you'd be creating this class in the same assembly as your mod, you'd be able to simply run the action you normally use to set-up your menu. 

However, this does allow you to create different menu's for players while they're in a lobby, or make use of the [`EnterMenuAction`](#entermenuaction) method to adjust how the menu looks based on if they're the master client or not.

--- 

## Documentation

### RWFSettingsUI
```CSHARP
public Class RWFSettingsUI : BaseUnityPlugin
```

#### Description

The base class for this mod, used to register the menus via Hard Dependency.

#### Methods

##### RegisterMenu
```CSHARP
public static void RegisterMenu(string name, Action<GameObject> guiAction, bool hostOnly = true, UnityAction enterMenuAction = null, UnityAction returnToLobbyAction = null)
```

###### Description
Registers a menu to add to the list of lobby menus.

###### Parameters
- *string* `name` the name displayed for the menu in the lobby menu. Internally used to sort the menus.
- *Action<GameObject>* `guiAction` the action run to build the menu.
- *bool* `hostOnly` whether the menu should only be visible for the host while in lobby, or if all players should have access to it. `true` by default.
- *UnityAction* `enterMenuAction` an action to run when the button to enter the mod's menu is clicked. `null` by default.
- *UnityAction* `returnToLobbyAction` an action to run when a player returns back to the main lobby. `null` by default.

### CompatibilityHandler
```CSHARP
public abstract class CompatibilityHandler : BaseUnityPlugin
```

#### Description

A class designed to make it simple to have a soft dependency on this mod. As a `BaseUnityPlugin`, it's necessary to define the `BepInPlugin` attribute for it, along with a `BepInDependency` for the mod that it's creating a soft dependency for.

#### Properties

##### MenuName
```CSHARP
public abstract string MenuName { get; }
```

###### Description
Returns the name of the menu of the menu to be created.

##### HostOnly
```CSHARP
public virtual bool HostOnly => true;
```

###### Description
Returns whether the created menu should be host only or not.

#### Methods

##### EnterMenuAction
```CSHARP
public virtual void EnterMenuAction()
```

###### Description
An action that's run when the button for the mod's menu is clicked. This is different from the button for entering the settings menu in a lobby.

##### GUI
```CSHARP
public abstract void GUI(GameObject menu);
```

###### Description
The action run to set the menu up.

###### Parameters
- *GameObject* `menu` the gameobject for the menu setup to be run on.

##### ReturnToLobbyAction
```CSHARP
public virtual void ReturnToLobbyAction()
```

###### Description
An action that's run when the player returns to the lobby from the settings menu.

---

## FAQ

### How does this mod affect synchronization of settings across all clients?

When a player enters the lobby menu and then exits, it retriggers the handshakes that are registered to unbound that occur when a player first joins a lobby, as well as any `OnJoinedRoom` events for any `UnboundLib.NetworkEventCallbacks` that are on the 'BepInEx_Manager' object that bepinex creates. If these are insufficient, you'll need to make use of the [`ReturnToLobbyAction`](#returntolobbyaction) method in [`CompatibilityHandler`](#compatibilityhandler) or the `returnToLobbyAction` parameter in [`RWFSettingsUI.RegisterMenu()`](#registermenu).

### Do I need to be worried about the game starting while a player is in a menu?

Yes and No. Once a player has joined a lobby with their character, entering the settings menu will cause their character to unready. However, since a player doesn't have to have joined the lobby with their character to enter the settings menu, the game may start without them. Also note that when the host exits the menu, it will unready all players once again.