<div align="center">

# VRCOSC
A modular OSC program creator, animation system, toolkit, and debugger made for [VRChat](https://vrchat.com)

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

VRCOSC is intended to act as a wrapper around VRChat's OSC system to make creating OSC programs easier,
provide interfaces for other APIs and frameworks that might be useful to an OSC project,
and act as a centralised source for useful OSC programs so that a user will only need to download a single application.
Our framework supports developing your own modules to save you the trouble of having to set up everything yourself,
as well as allowing other people to seamlessly use your module on their PC. See how to create a module [here](https://vrcosc.com/docs/v2/sdk/getting-started).

VRCOSC's powerful ChatBox design system allows you to display what you want, when you want, how you want. Check out the ChatBox-Config forum channel of our [Discord Server](https://discord.gg/vj4brHyvT5) to see some of the configs people have created or you can make your own by following [the docs](https://vrcosc.com/docs/v2/chatbox)!
The ChatBox uses a [community-created list](https://github.com/cyberkitsune/chatbox-club-blacklist/blob/master/npblacklist.json) to block the ChatBox from being used in certain worlds. You can turn this off in the app settings, but we recommend you keep it on for respect.

Featuring:
- Modern GUI
- Automated configuration management
- A powerful ChatBox animation system
- Profiles that can link to avatars
- A built-in router
- Program modularity for easy development
- Automatic updates
- Common API interfaces
- Drag-and-drop Unity prefabs
- Debug tools

## Getting Started
- Download and run `VRCOSCSetup.exe` from the [Releases](https://github.com/VolcanicArts/VRCOSC/releases/latest) page
- Enable the modules you'd like to use
- (Optional) Download any prefabs you want and add them to your avatar from [here](https://vrcosc.com/docs/downloads#prefabs)
- Press the run button!

Check the [FAQ](https://vrcosc.com/docs/faq) if you have any issues with installing, using the app, or using any of the prefabs.
Join the [Discord Server](https://vrcosc.com/discord) if you need any other help.

## Official Modules
If you have a module idea join the [Discord Server](https://vrcosc.com/discord) and tell us or [create it yourself](https://vrcosc.com/docs/v2/sdk/getting-started)!

All modules support sending generic parameters that are standardised so that public avatars can utilise these features.
Most modules have ChatBox support, providing generic variables for you to customise in limitless ways.

The official modules source code is located [here](https://github.com/VolcanicArts/VRCOSC-Modules).

|       Module Name        | Description                                                                                                                                                                                                            |
|:------------------------:|:-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
|          Media           | Allows for media (Spotify, YouTube, etc) display in the ChatBox and full control over media from your action menu                                                                                                      |
|         Pulsoid          | Connects to [Pulsoid](https://pulsoid.net/) to display your live heartrate in-game - [Supported Devices](https://www.blog.pulsoid.net/monitors) [![pulsoid](https://pulsoid.net/s/github-badge)](https://pulsoid.net/) |
|         HypeRate         | Connects to [HypeRate.io](https://www.hyperate.io/) to display your live heartrate in-game - [Supported Devices](https://www.hyperate.io/supported-devices)                                                            |
|      Speech To Text      | Listens to your speech and displays the result in the ChatBox                                                                                                                                                          |
|      Voice Commands      | Control avatar parameters with your voice                                                                                                                                                                              |
|         DateTime         | Sends your local date/time as day, month, year, hours, minutes, and seconds to avatar parameters                                                                                                                       |
|         Counter          | Counts how many times a parameter on your avatar has changed to be displayed in the ChatBox                                                                                                                            |
|         PiShock          | Control groups of PiShock shockers directly from your avatar, or with your voice                                                                                                                                       |
|          Maths           | Define complex equations to drive avatar parameters                                                                                                                                                                    |
|         Keybinds         | Trigger keybinds using avatar parameters                                                                                                                                                                               |
|      Client Events       | Listens for events from VRChat and triggers parameters on your avatar                                                                                                                                                  |
|      Hardware Stats      | Gathers hardware stats to send to your avatar and ChatBox                                                                                                                                                              |
|      SteamVR Stats       | Gets statistics from your SteamVR session                                                                                                                                                                              |
|  SteamVR Haptic Control  | Control SteamVR haptics on your devices                                                                                                                                                                                |
| Index Gesture Extensions | Allows for custom gestures to be sent to VRChat from your Index controllers                                                                                                                                            |
|      AFK Detection       | Detects when you're AFK in VRChat or SteamVR                                                                                                                                                                           |
|     Process Manager      | Open and close apps on your PC using avatar parameters                                                                                                                                                                 |
|        Stopwatch         | A simple stopwatch                                                                                                                                                                                                     |
|         Weather          | Gets weather from a postcode/zipcode/city to display in the ChatBox                                                                                                                                                    |

## License
This program is licensed under the [GNU General Public License V3](https://www.gnu.org/licenses/gpl-3.0.en.html). Please see [the license file](LICENSE) for more information.

Other libraries included in this project may contain different licenses. See the license files in their repos for more information.