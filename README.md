# VRCOSC
A modular OSC provider primarily made for [VRChat](https://vrchat.com) built on top of the [osu!framework](https://github.com/ppy/osu-framework)

<p align="center"><img src="https://user-images.githubusercontent.com/29819296/170736407-3359db07-52a0-443b-9750-5a4c9770e421.png" width=70% height=70%></p>

## Getting Started
To download VRCOSC, head to the [Releases](https://github.com/VolcanicArts/VRCOSC/releases/latest) section of this repo.

If you'd like to develop a module, check the [How To Create A Module](https://github.com/VolcanicArts/VRCOSC#how-to-create-a-module) section of this README.

Avatar prefabs for select modules will be available in the future.

## Modules
| Module | Description | Notes |
| :---: | :--- | :--- |
| HypeRate | Sends [HypeRate.io](https://www.hyperate.io/supported-devices) heartrate values | Requires a free API key which can be obtained from HypeRate's [Discord Server](https://discord.gg/eTwfgU29cU) |
| Clock | Sends your current local time in 2 different formats | |
| Computer Stats | Sends your system stats. Currently CPU, GPU, and RAM | Only works on Windows |
| Random | Sends a random float between 0 and 1 every second | |

## How To Create A Module
- To create a module, fork this repo and create a new folder inside the [Modules](https://github.com/VolcanicArts/VRCOSC/tree/master/VRCOSC.Game/Modules/Modules) folder.
- Create a class with your module's name and have it extend the Module class.
- Override `Title`, `Description`, `Author`, `Colour`, `ModuleType`, and `UpdateDelta` to change your module's metadata.
- Override `OnStart()`, `OnUpdate()`, and `OnStop()` to run your module code on each event.
- NOTE: The `UpdateDelta` property alters the time between each `OnUpdate()` call in milliseconds.
- NOTE: If your module doesn't fit any current module type, make a new type by editing the `ModuleType` enum.
- To create settings (such as having the user enter an API key), call the `CreateSetting()` method inside your module's constructor and fill in the required information.
- To create OSC parameters, call the `CreateParameter()` method inside your module's constructor and fill in the required information.
- Settings and OSC parameters both require enums as keys so it's recommended to create two enums titled `[ModuleName]Setting` and `[ModuleName]Parameter` inside your module's .cs file.
- You can access settings by calling the `GetSettingAs<T>()` method where `T` is the setting's type.
- To send data over OSC, call the `SendParameter()` method.
- Finally, make a pull request to submit your module.

## Examples
If you'd like to see some examples of existing modules, then you can find them inside the [Modules](https://github.com/VolcanicArts/VRCOSC/tree/master/VRCOSC.Game/Modules/Modules) folder.
