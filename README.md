# VRCOSC
A modular OSC provider primarily made for VRChat.

## Modules
- HypeRate: Takes HypeRate.io heartrate values and sends multiple OSC values into VRChat.
- Clock: Takes your current local time and sends multiple OSC values into VRChat.

## Getting Started
- To create a module, fork this repo and create a new folder inside the [Modules](https://github.com/VolcanicArts/VRCOSC/tree/master/VRCOSC.Game/Modules/Modules) folder.
- Create a class and have it extend the Module class.
- You can now override `Start()`, `Update()`, and `Stop()`.
- Each `Update()` call can be controlled via overriding the `UpdateDelta` property.
- You can also change the colour of your module by overriding the `Colour` property. This alters how it will appear inside the app.
- Make sure to sign your module with your name by overriding the `Author` property.
- Modules also have a `ModuleType`. If your module fits into an existing type, use that. If you believe your module needs a new type, make one by editing the `ModuleType` enum.
- To create settings for your module (such as having the user enter an API key), can be done by calling the `CreateSetting()` method inside your module's constructor.
- The same can be done for parameters, however, you're required to create a parameter enum.
- Each setting and parameter has an option for a title and description to let the user know what does what in your module.
- Finally, make a pull request and if your module is accepted it will become part of the program!

Notes:
- Make sure to call `base.Start()` and `base.Stop()` if you do override them. Core functionality for a module before your code is called may need to run and your module may function in an unexpected way if those are not called.
- Make sure to call `LoadData()` at the end of your constructor, else the user's overridden saved settings and parameters will not load.
- Modules do not have to be inside one class only. Feel free to make as many as you need for your module's functionally!

## Examples
If you'd like to see some examples of existing modules, then you can find them inside the [Modules](https://github.com/VolcanicArts/VRCOSC/tree/master/VRCOSC.Game/Modules/Modules) folder.
