# VRCOSC
A modular OSC provider primarily made for [VRChat](https://vrchat.com) built on top of the [osu!framework](https://github.com/ppy/osu-framework)

<p align="center"><img src="https://user-images.githubusercontent.com/29819296/170842562-e42d1fc8-de28-46c0-b793-dd66715349bf.png" width=70% height=70%></p>

## Getting Started
To download VRCOSC, head to the [Releases](https://github.com/VolcanicArts/VRCOSC/releases/latest) section of this repo.

## Modules
| Module | Description | Notes | Prefab |
| :---: | :---: | :---: | :---: |
| HypeRate | Sends [HypeRate.io](https://www.hyperate.io/supported-devices) heartrate values | Requires a free API key which can be obtained from HypeRate's [Discord Server](https://discord.gg/eTwfgU29cU) | VRCOSC-Heartrate |
| Discord Voice | Toggle your Discord microphone | No prefab is provided, but the parameter works best with an action menu button | |
| Clock | Sends your current local time in 2 different formats | | |
| Computer Stats | Sends your system stats. Currently CPU, GPU, and RAM | Only works on Windows | |
| Random | Sends a random float between 0 and 1 every second | | |

## Creating a module
### First Steps
- Fork this repo and create a new folder inside the [Modules](https://github.com/VolcanicArts/VRCOSC/tree/master/VRCOSC.Game/Modules/Modules) folder.
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

## Examples
If you'd like to see some examples of existing modules, then you can find them inside the [Modules](https://github.com/VolcanicArts/VRCOSC/tree/master/VRCOSC.Game/Modules/Modules) folder.
