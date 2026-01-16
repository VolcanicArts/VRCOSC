<div align="center">

# VRCOSC
A modular node-programming language, program creator, animation system, toolkit, router, and debugger made for [VRChat](https://vrchat.com)

<img src="https://github.com/user-attachments/assets/61945157-d185-4134-a655-d6a9923eba23" width=75% height=75%>

[![downloads](https://img.shields.io/github/downloads/VolcanicArts/VRCOSC/total?style=for-the-badge&label=Downloads%20Total)](https://github.com/VolcanicArts/VRCOSC/releases/latest)
[![downloads@latest](https://img.shields.io/github/downloads/VolcanicArts/VRCOSC/latest/total?style=for-the-badge&label=Downloads%20For%20Latest)](https://github.com/VolcanicArts/VRCOSC/releases/latest)
[![commits](https://img.shields.io/github/commit-activity/m/VolcanicArts/VRCOSC/dev?style=for-the-badge)](https://github.com/VolcanicArts/VRCOSC/commits/dev)
<br>
[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/volcanicarts)
<br/>
[![discord](https://discordapp.com/api/guilds/1000862183963496519/widget.png?style=shield)](https://vrcosc.com/discord)
[![docs](https://img.shields.io/badge/Documentation-2b2b2b?logo=docusaurus)](https://vrcosc.com/docs/welcome)

</div>

## About

VRCOSC acts as a wrapper around VRChat's OSC system to make creating OSC programs easier through modules and node-programming,
provide interfaces for other APIs and frameworks that might be useful to an OSC project,
and act as a centralised source for useful OSC programs so that a user will only need to download a single application.
Our framework supports developing your own modules to save you the trouble of having to set up everything yourself,
as well as allowing other people to seamlessly use your module on their PC. See how to create a module [here](https://vrcosc.com/docs/v2/sdk/getting-started).

Featuring:
- Custom node-programming language
- ChatBox animation system
- Per-avatar profiles
- Built-in router
- Automatic updates
- Common API interfaces
- Drag-and-drop Unity prefabs
- Debug tools

### Pulse
VRCOSC contains a custom node-programming language called Pulse, built to allow users to make custom behaviour without making a module.
It allows for connecting APIs to VRChat, the speech engine to parameters, doing complex maths, and more!
You can check out Pulse [here](https://vrcosc.com/docs/v2/pulse/getting-started)!

<div align="center">
<img src="https://vrcosc.com/assets/images/example-change-avatar-23cea8b274929a1b4220d0ce81d7fb6e.png" width=50%>
</div>

### ChatBox
VRCOSC's ChatBox animation system allows you to display what you want, when you want, how you want. Check out the ChatBox-Config forum channel of our [Discord](https://discord.gg/vj4brHyvT5) server to see some of the configs people have created or you can make your own by following [the docs](https://vrcosc.com/docs/v2/chatbox)!
The ChatBox uses a [community-created list](https://github.com/cyberkitsune/chatbox-club-blacklist/blob/master/npblacklist.json) to block the ChatBox from being used in certain worlds. You can turn this off in the app settings, but we recommend you keep it on out of respect.

## Getting Started
- Download and run `VRCOSCSetup.exe` from the [latest releases](https://github.com/VolcanicArts/VRCOSC/releases/latest)
- Enable the modules you'd like to use
- Press the run button!

Check the [FAQ](https://vrcosc.com/docs/faq) if you have any issues with installing, using the app, or using any of the prefabs.
Join the [Discord](https://vrcosc.com/discord) server if you need any other help.

Optionally, you can download any first-party prefabs you want and add them to your avatar from [here](https://vrcosc.com/docs/downloads#prefabs).

## Official Modules
If you have a module idea join the [Discord](https://vrcosc.com/discord) server and tell us or [create it yourself](https://vrcosc.com/docs/v2/sdk/getting-started)!

All modules support sending generic parameters that are standardised so that public avatars can utilise these features.
Most modules have ChatBox support, providing generic variables for you to use.

You can find the Official Modules source code [here](https://github.com/VolcanicArts/VRCOSC-Modules).

|       Module Name        | Description                                                                                                                                                                                           |
|:------------------------:|:------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
|          Media           | Allows for media (Spotify, YouTube, etc) display in the ChatBox and control over the media sessions                                                                                                   |
|          Twitch          | Connects Twitch events to Pulse, and allows Pulse to control your Twitch                                                                                                                              
|         Pulsoid          | Connects to [Pulsoid](https://pulsoid.net/) to get your heartrate - [Supported Devices](https://www.blog.pulsoid.net/monitors) [![pulsoid](https://pulsoid.net/s/github-badge)](https://pulsoid.net/) |
|         HypeRate         | Connects to [HypeRate](https://www.hyperate.io/) to get your heartrate - [Supported Devices](https://www.hyperate.io/supported-devices)                                                               |
|      ParameterSync       | Allows for saving and syncing parameters between avatars                                                                                                                                              
|      Speech To Text      | Listens to your speech and displays the result in the ChatBox                                                                                                                                         |
|      Voice Commands      | Control avatar parameters with your voice                                                                                                                                                             |
|         DateTime         | Converts the date and time to useful avatar parameters                                                                                                                                                |
|         Counter          | Counts how many times a parameter on your avatar has changed to be displayed in the ChatBox                                                                                                           |
|         PiShock          | Control groups of PiShock shockers from your avatar or your voice                                                                                                                                     |
|          Maths           | Define complex equations to drive avatar parameters                                                                                                                                                   |
|         Keybinds         | Trigger keybinds using avatar parameters                                                                                                                                                              |
|      Client Events       | Listens for events from VRChat and triggers parameters on your avatar                                                                                                                                 |
|      Hardware Stats      | Gathers hardware stats to send to your avatar and ChatBox                                                                                                                                             |
|      SteamVR Stats       | Gets statistics from your SteamVR session                                                                                                                                                             |
|  SteamVR Haptic Control  | Control SteamVR haptics on your devices                                                                                                                                                               |
| Index Gesture Extensions | Allows for custom gestures to be sent to VRChat from your Index controllers                                                                                                                           |
|      AFK Detection       | Detects when you're AFK in VRChat or SteamVR                                                                                                                                                          |
|     Process Manager      | Open and close apps on your PC using avatar parameters                                                                                                                                                |
|        Stopwatch         | A simple stopwatch                                                                                                                                                                                    |
|         Weather          | Gets weather from a postcode/zipcode/city                                                                                                                                                             |

## License
This program is licensed under the [GNU General Public License V3](https://www.gnu.org/licenses/gpl-3.0.en.html). Please see [the license file](LICENSE) for more information.

Other libraries included in this project may contain different licenses. See the license files in their repos for more information.