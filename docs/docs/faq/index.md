---
sidebar_position: 4
sidebar_label: FAQ
---

# FAQ

## The facial expression does not change when I change the hand gesture.

Please check the following points:

|<center>Check Points</center>|<center>Approaches</center>|
|:-|:-|
| "FaceEmo" is not added to the VRChat Expression Menu | Follow the [tutorial](../tutorials/) to apply the facial expression menu to your avatar. |
| The expression pattern is not correctly selected in the "FaceEmo" menu | Please select the appropriate expression pattern. |
| The "FaceEmo" → "Emote Select" menu is open | Please close the "Emote Select" menu with "Back". <br/> (Check the VRChat Expression Menu for both left and right hands) |
| "FaceEmo" → "Emote Select" → "Emote Lock ON" is enabled | Please disable "Emote lock ON". |
| "FaceEmo" → "Settings" → "Emote Lock ON" is enabled | Please disable "Emote lock ON". |

Reference: [Optional features > Lock Expressions](../optional-functions/emote-lock/)

## Can I use the facial expression menu with the expressions originally set on the avatar?

To prevent interference with expressions, expressions originally set on the avatar will be disabled.  
Please add similar expression settings to the facial expression menu of this tool.

## How can I remove the facial expression menu from the avatar?

Please remove the "FaceEmoPrefab" added in the avatar.  
Reference: [Tutorial > Creating a Simple Facial Expression Menu](../tutorials/simple-menu/)

## Can I display a dialog box again after checking "Don't show again"?

You can change the display status of the dialog box from the "Editor Settings" in the Inspector.

## The UI is displayed incorrectly.

If you are experiencing any of the following problems, you may be able to resolve the problem by going to Play mode once and then back to Edit mode.

- Width of each view is not adjusted correctly.
- Expression thumbnails are not rendered.

## How to automatically organize the emote select menu into folders

Please enable "Optional Settings" -> "Avatar Application Settings" -> "Automatically Create Folders When More Than 8 Expressions".
