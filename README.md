# Valheim - Player Trading

# About

Tired of throwing your items to other players? This mod implements real trading between players. In order to start a new trade instance, you must first **interact** with the target player to send them a **trade request**. If the other player **accepts your trade request** (by also interacting with you), the instance will start.

During the trade instance, two windows will be visible, the first window will hold the **items you wish to offer**, the second will be a preview of the **items you will receive**. Once a player is happy with the trade they can choose to **Accept**, **both players** must have accepted before the trade can be finalized. Any changes to the trade will automatically reset both player's Accept state in order to prevent malicious behaviour.

# Features

* Quick and easy trading between players.
* Client-side only! No server installations needed.
* Works with custom items!
* Configure the mod UI by pressing F11 (key is configurable)
* Use it anywhere! As long as the player you're trading also has the mod installed.
* Full and continued gamepad support.
* Familiar keybinds will continue to work! (Splitting stacks, Quick-Select etc).

# Installation

You must have **BepinEx** installed before attempting to install this mod.
Move the **.dll file** into your **Valheim\BepInEx\plugins** folder.

In order to configure the trade UI window placement, press **F11** (default key) and drag the windows/buttons to their desired location.
While in placement mode, elements that can be moved will be highlighted purple.

If you want to translate this mod into another language, I've provided an additional config file for string localization named PlayerTradingStrings.txt. This file is provided in a JSON format and each entry is defined as **"Key" : "Value"**. To configure this file simply change the **Value** of each entry. Be careful not to change the **Key** string otherwise the mod may break.

# Feedback

I appreciate any feedback that you might have. If you encounter a bug please report it whenever you can so that I can fix it in the following update.
If you discover a mod incompatibility then I will attempt to make it compatible. Thanks!

## Video Preview:

[![Valheim - Player Trading Mod](https://i.imgur.com/vUdpT3j.png)](https://www.youtube.com/watch?v=jc0tMuEjXbM)

## Screenshots:

![alt text](https://i.imgur.com/JLERNyJ.png "Screenshot 1")

![alt text](https://i.imgur.com/6jnxlXj.png "Screenshot 2")

![alt text](https://i.imgur.com/HoDWZlH.png "Screenshot 3")

![alt text](https://i.imgur.com/2rqd5SN.png "Screenshot 4")

# Changelog
        Version 1.2.1
            Hotfix for missing directory exception on initialisation.
        Version 1.2.0
            Added string localization file into the config folder (must be configured manually).
            Fixed trade requests being sent while typing in the chat.
            Attempt to fix bug where trade requests are sent to nearby players.
        Version 1.1.1
            Added config option to use modifier key when sending/receiving trade requests
        Version 1.1.0
            Added Window Edit Mode - toggled on/off using F11 (configurable).
            Fixed bug where inventory grid misaligns when using custom Valheim Plus inventory config.
            Disable E (interact) closing the current trade instance.
        Version 1.0.1
            Added Gamepad support.
        Version 1.0.0
            Player Trading initial release.
