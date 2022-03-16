# VRCOSC
A modular OSC provider primarily made for VRChat.

## Modules
- HypeRate: Takes HypeRate.io heartrate values and sends multiple OSC values into VRChat.
- Clock: Takes your current local time and sends multiple OSC values into VRChat.
- Computer Stats: Takes your system stats (CPU, GPU, and RAM usage) and sends them into VRChat normalised between 0 and 1

## Getting Started
Right now there are no releases as the application isn't read for production. If you'd still like to develop a module, check the [How To Create A Module](https://github.com/VolcanicArts/VRCOSC#how-to-create-a-module) section of this README.

## How To Create A Module
- To create a module, fork this repo and create a new folder inside the [Modules](https://github.com/VolcanicArts/VRCOSC/tree/master/VRCOSC.Game/Modules/Modules) folder.
- Create a class and have it extend the Module class.
- Override the `Title` property to set a title.
- Override the `Description` property to set a description.
- Override the `Author` property to sign your module.
- Override the `Colour` property to alter the colour of background of your module.
- Override the `ModuleType` property to classify your module. If you believe your module requires a new type, make one by editing the `ModuleType` enum.
- Override the `UpdateDelta` property to alter the time between each `OnUpdate()` call.
- Override `OnStart()` to execute code when your module is started.
- Override `OnUpdate()` to execute code on each update call.
- Override `OnStop()` to execute code on when your module is stopped.
- To create settings (such as having the user enter an API key), call the `CreateSetting()` method inside your module's constructor and fill in the required information.
- To create OSC parameters, call the `CreateParameter()` method inside your module's constructor and fill in the required information.
- Settings and OSC parameters both require enums as keys so it's recommended to create two enums titled `[ModuleName]Setting` and `[ModuleName]Parameter`.
- You can access settings by calling the `GetSettingAs<T>()` method where `T` is the setting's type.
- To send data over OSC, call the `SendParameter()` method.
- Finally, make a pull request to submit your module for review.

## Examples
If you'd like to see some examples of existing modules, then you can find them inside the [Modules](https://github.com/VolcanicArts/VRCOSC/tree/master/VRCOSC.Game/Modules/Modules) folder.
