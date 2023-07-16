---
sidebar_position: 2
sidebar_label: Changing expressions based on trigger squeezing
---

# Changing expressions based on trigger squeezing

In expressions including Fist as a condition, you can change the expression according to the squeezing applied to the trigger.

:::tip
In VRChat, you can get the analog input value of the trigger when the hand gesture is Fist.
:::

- Check "Use Left Trigger" or "Use Right Trigger" to enable the trigger settings
- Set the expressions for when the "Left Trigger", "Right Trigger", or "Both Triggers" are squeezed

![Trigger settings](trigger.png)

:::caution
### Notes on trigger settings
- Even if you reverse the left and right gestures in the "Gesture Settings" of the [Settings Menu](../setting-menu), the left and right of the trigger will not be reversed
- Even if you disable the left and right gestures in the "Gesture Settings" of the [Settings Menu](../setting-menu), the change in trigger squeezing will not be disabled
- If you change the gesture to something other than Fist while squeezing the trigger, the trigger squeezing value remains the same
    - If you want to reset the trigger squeezing value to 0, slowly remove your hand from the trigger while keeping it in Fist
- If there are expressions that use both triggers, please enable "Smooth Analog Fist" in the "Avatar Application Settings"
    - "Smooth Analog Fist" is enabled by default
:::
