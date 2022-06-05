# VRCOSC
A modular OSC provider primarily made for [VRChat](https://vrchat.com) built on top of the [osu!framework](https://github.com/ppy/osu-framework)

<p align="center"><img src="https://user-images.githubusercontent.com/29819296/171054967-3d4c3726-b854-4c54-9e3a-8d928b172647.png" width=70% height=70%></p>

## Getting Started
To download VRCOSC, head to the [Releases](https://github.com/VolcanicArts/VRCOSC/releases/latest) section of this repo.

NOTE: VRCOSC is a Windows only program due to low level key bindings and statistic retrieval.

## Modules
| Module | Description |                                                     Notes                                                     | Prefab |
| :---: | :---: |:-------------------------------------------------------------------------------------------------------------:| :---: |
| HypeRate | Sends [HypeRate.io](https://www.hyperate.io/supported-devices) heartrate values | Requires a free API key which can be obtained from HypeRate's [Discord Server](https://discord.gg/eTwfgU29cU) | VRCOSC-Heartrate |
| Discord | Discord integration |                   Requires the Discord desktop app. Allows for toggling of mute and deafen                    | VRCOSC-Discord |
| Spotify | Spotify integration |          Requires the Spotify desktop app. Allows for play/pause, next/previous, and volume up/down           | VRCOSC-Spotify |
| Calculator | Calculator module |              Uses the built-in Windows calculator. Allows for the use of the calculator in-game               | |
| Clock | Sends your current local time in 2 different formats |                                                                                                               | |
| Computer Stats | Sends your system stats. Currently CPU, GPU, and RAM |                                                                                                               | |
| Random | Sends a random float between 0 and 1 every second |                                                                                                               | |

## Creating a module
### First Steps
- Fork this repo and create a new folder inside the [Modules](https://github.com/VolcanicArts/VRCOSC/tree/master/VRCOSC.Game/Modules/Modules) folder.
- NOTE: Other modules are present in this folder and can be used as templates or examples.
### Metadata
- Create a class with your module's name and have it extend the Module class.
- Override `Title`, `Description`, `Author`, `Colour`, `ModuleType`, and `UpdateDelta` to change your module's metadata.
- NOTE: The `UpdateDelta` property alters the time between each `OnUpdate()` call in milliseconds.
- NOTE: If your module doesn't fit any current module type, make a new type by editing the `ModuleType` enum.
### Module Events
- Override `OnStart()`, `OnUpdate()`, and `OnStop()` to run your module code on each event.
### Settings and Parameters
- Override `CreateAttributes()`. This is where your attribute creation (settings and parameters) will occur.
- To create settings (such as having the user enter an API key), call the `CreateSetting()` method and fill in the required information.
- To create OSC parameters, call the `CreateParameter()` method and fill in the required information.
- Settings and OSC parameters both require enums as keys so it's recommended to create two enums titled `[ModuleName]Setting` and `[ModuleName]Parameter` inside your module's .cs file.
### Retrieving settings
- You can access settings by calling the `GetSettingAs<T>()` method where `T` is the setting's type.
### Sending data
- To send data over OSC, call the `SendParameter()` method.
### Receiving data
- To retrieve OSC data from VRChat, override `InputParameters`. This is a list of Enums which will be converted into `/avatar/parameters/ENUM_NAME` addresses at runtime.
- An enum of the default avatar parameters VRChat provides has been provided for you to use named `VRChatInputParameters`.
- NOTE: `InputParameters` can take a combination of different enums. They're only used as reference keys.
- To listen for events from these parameters, override `OnParameterReceived()`.
### Final Steps
- Make a pull request to submit your module.

## Integration Modules
- Integration modules are special types of modules. They allow for executing keyboard shortcuts on specific processes within Windows.
- Examples on how to create integration modules are in the [Modules](https://github.com/VolcanicArts/VRCOSC/tree/master/VRCOSC.Game/Modules/Modules) folder under `Discord` or `Spotify`
