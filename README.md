# SAMMI Webhook Plugin

https://github.com/user-attachments/assets/ac61a657-abfd-4fab-be13-769d7b1301f0
Plugin for sending game information to SAMMI via webhook, allowing for automated OBS actions based on in-game events. This plugin only handles the sending of webhooks, the specific reactions (e.g., animations/sound bytes played, stream interactions, etc.) are created independently within [SAMMI](https://sammi.solutions/) and [OBS](https://obsproject.com/).

## Currently Implemented Features

* Character info (xiv_charUpdate)
  * Name
  * Max HP
  * Current HP
  * Max MP
  * Current MP
* Flying Text info (xiv_flyTextUpdate)
  * Text content
  * Damage dealt
  * Critical/Direct Hit status
  * Buffs received/expired
  * Debuffs received/expired
* Action used (xiv_actionUpdate)
  * Action type
  * Action ID
  * Action name

## How To Use

### Prerequisites

SAMMI Webhook Plugin requires [SAMMI](https://sammi.solutions/) and [OBS](https://obsproject.com/).

### Getting Started

Open SAMMI and click **"Settings"** in the bottom left corner. Under **"Connections"**, make sure **"Open Local API Server"** is enabled. Click Save and Close.

Inside SAMMI, click the "+" button and add a new deck, then a new button in the deck. Right click the button and select **"Edit Triggers"** from the context menu. Click the "+" button and select **"Webhook Trigger"**. Inside the textbox, the "*" will be replaced with whichever webhook you are interested in (xiv_charUpdate, xiv_flyTextUpdate, or xiv_actionUpdate). Click Save and the button should now have a gun icon attached to indicate it contains a trigger.

Double-click the button, click the "+" button, and select **"Trigger Pull Data"**. This will save all the data from the webhook into a variable of your choosing within SAMMI. For example, in a button listening for the xiv_charUpdate webhook, saving the "Trigger Pull Data" to a variable named xivCharacter will save both the character name and character hp under xivCharacter.data.name and xivCharacter.data.hp. Note that when accessing this data from outside the button will require you to include the button name as well (e.g., Button1.xivCharacter.data.name). Saved variables can be checked by clicking "Open Variable Window" inside a button, or the "Variable Viewer" in SAMMI Core.

Now you can do whatever you want with the variables, like playing a media source in OBS when character HP reaches 0, rolling a random effect when an attack crits, apply an effect to your webcam when a vulnerability stack is gained, etc.

### Activating in-game
By default, all features are disabled, and each webhook category is activated individually within the plugin.

1. Launch the game and use `/psammi` in chat or click the cog icon next to the SAMMI Webhook Plugin inside `/xlplugins`.
2. Double check the SAMMI API Address box and the Password box are correct. The default values correspond with the default values in SAMMI.
3. Check each box corresponding to the webhook you want enabled. E.g., if you want to track character HP, enable xiv_charUpdate.
