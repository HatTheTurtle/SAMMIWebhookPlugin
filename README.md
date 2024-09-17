# SAMMI Webhook Plugin

Plugin for sending game information to SAMMI via webhook. This plugin only handles the sending of webhooks, the specific reactions (e.g., animations/sound bytes played, stream interactions, etc.) are created independently within [SAMMI](https://sammi.solutions/) and [OBS](https://obsproject.com/).

## Currently Implemented Features
* Character info (xiv_charUpdate)
  * Name
  * HP
* Flying Text info (xiv_flyTextUpdate)
  * Abilities used
  * Damage dealt
  * Critical/Direct Hit status
  * Buffs received/expired
* (WIP)Action used (xiv_actionUpdate)
  * Action type
  * Action ID

## How To Use

### Prerequisites

SAMMI Webhook Plugin is built for use with [SAMMI](https://sammi.solutions/) and [OBS](https://obsproject.com/).

### Getting Started

Open SAMMI and click **"Settings"** in the bottom left corner. Under **"Connections"**, make sure **"Open Local API Server"** is enabled. The plugin does not currently support SAMMI API requests with a password, so make sure this textbox is blank. Click Save and Close.

Inside SAMMI, click the "+" button and add a new deck, then a new button in the deck. Right click the button and select **"Edit Triggers"** from the context menu. Click the "+" button and select **"Webhook Trigger"**. Inside the textbox, the "*" will be replaced with whichever webhook you are interested in (xiv_charUpdate, xiv_flyTextUpdate, or xiv_actionUpdate). Click Save and the button should now have a gun icon attached to indicate it contains a trigger.

Double-click the button, click the "+" button, and select **"Trigger Pull Data"**. This will save all the data from the webhook into a variable of your choosing within SAMMI. For example, in a button listening for the xiv_charUpdate webhook, saving the "Trigger Pull Data" to a variable named xivCharacter will save both the character name and character hp under xivCharacter.data.name and xivCharacter.data.hp. Note that when accessing this data from outside the button will require you to include the button name as well (e.g., Button1.xivCharacter.data.name). Saved variables can be checked by clicking "Open Variable Window" inside a button, or the "Variable Viewer" in SAMMI Core.

Now you can do whatever you want with the variables, like playing a media source in OBS when character HP reaches 0, rolling a random effect when an attack crits, replacing your webcam with a pre-recorded video, etc.

### Activating in-game
By default, all features are disabled, and each webhook is activated individually within the plugin.

1. Launch the game and use `/psammi` in chat or click the cog icon next to the SAMMI Webhook Plugin inside `/xlplugins`.
2. Check each box corresponding to the webhook you want enabled.

### Installing the plugin
The plugin is not currently finished or submitted to the official Dalamud plugin repository. Using it in its current state requires users to build the plugin on their own, and adding it to Dalamud as a dev plugin using the instructions below:

1. Launch the game and use /xlsettings in chat or xlsettings in the Dalamud Console to open up the Dalamud settings.
    * In here, go to Experimental, and add the full path to the SammiPlugin.dll to the list of Dev Plugin Locations.
2. Next, use /xlplugins (chat) or xlplugins (console) to open up the Plugin Installer.
    * In here, go to Dev Tools > Installed Dev Plugins, and the SammiPlugin should be visible. Enable it.
3. You should now be able to use /psammi (chat) or psammi (console)!

Note that you only need to add it to the Dev Plugin Locations once (Step 1); it is preserved afterwards. You can disable, enable, or load your plugin on startup through the Plugin Installer.
