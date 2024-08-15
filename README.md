<div align="center">

# VRCOSC
A modular OSC program creator and toolkit made for [VRChat](https://vrchat.com) built on top of the [osu!framework](https://github.com/ppy/osu-framework)

<img src="https://github.com/VolcanicArts/VRCOSC/assets/29819296/a6828e41-ad72-4068-a195-42dc6508ceff" width=70% height=70%>

[![release version](https://img.shields.io/github/v/release/VolcanicArts/VRCOSC?style=for-the-badge)](https://github.com/VolcanicArts/VRCOSC/releases/latest)
[![downloads](https://img.shields.io/github/downloads/VolcanicArts/VRCOSC/total?style=for-the-badge&label=Downloads%20Total)](https://github.com/VolcanicArts/VRCOSC/releases/latest)
[![downloads@latest](https://img.shields.io/github/downloads/VolcanicArts/VRCOSC/latest/total?style=for-the-badge&label=Downloads%20For%20Latest)](https://github.com/VolcanicArts/VRCOSC/releases/latest)
[![commits](https://img.shields.io/github/commit-activity/m/VolcanicArts/VRCOSC/v2-wpf?style=for-the-badge)](https://github.com/VolcanicArts/VRCOSC/commits/v2-wpf)
<br>
[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/O5O8FF9YO)
<br>
[![discord](https://discordapp.com/api/guilds/1000862183963496519/widget.png?style=shield)](https://discord.gg/vj4brHyvT5)

</div>

## V2
V2 is a rewrite of the app currently in beta. It'll be released when it's ready, but is a huge advancement over the current version.

Featuring:
- Module packages: Release a module on GitHub and have it appear in the app for anyone to download
- Profiles: For those that want different modules per avatar, profiles can automatically change how the modules behave based on which avatar you're wearing
- Better performance: With virtually 0 CPU usage and no GPU usage, it can quietly run in the background while you're in VR with no performance hit
- Module generalisation: Have a prefab you've wanted to connect to VROSC? Now you can. All the parameters are generic and customisable
- More module features: All the modules have been upgraded with more customisability, and more have been added
- ChatBox V4: An improved animation system with truly limitless customisation
- Improved SDK: For those that want to make and distribute modules with our framework, it's easier than ever

If you're interested in beta testing the app, grab the V2 Beta role in the [Discord Server](https://discord.gg/vj4brHyvT5). Any and all feedback is welcomed!

## About

VRCOSC is intended to act as a wrapper around VRChat's OSC system to make creating OSC programs easier, provide interfaces for other APIs and frameworks that might be useful to an OSC project, and act as a centralised source for useful OSC programs so that a user will only need to download a single application. Our framework supports developing your own modules to save you the trouble of having to setup everything yourself, as well as allowing other people to seamlessly use your module on their PC. Check out how to create a module [here](https://github.com/VolcanicArts/VRCOSC/wiki/Module-Creation).

VRCOSC's powerful ChatBox design system allows you to display what you want, when you want, how you want. Check out the ChatBox-Config forum channel of our [Discord Server](https://discord.gg/vj4brHyvT5) to see some of the configs people have created!
The ChatBox uses a [community-created list](https://github.com/cyberkitsune/chatbox-club-blacklist/blob/master/npblacklist.json) to block all modules apart from the accessibility and health related modules (SpeechToText, Pulsoid, and Hyperate) from using the ChatBox when in club/event worlds as a courtesy to performers and event staff. You can turn this off in VRCOSC's settings, but we recommend you keep it on for respect.

VRCOSC's router lets you route other OSC programs through our app to stop port binding issues. Common setups can be found on the [Routing Wiki](https://github.com/VolcanicArts/VRCOSC/wiki/VRCOSC-Router) page.

We have a [Discord Server](https://discord.gg/vj4brHyvT5) for posting suggestions or to get help with anything to do with VRCOSC.

If you like VRCOSC, please star the repo. It really helps!

Featuring:
- Responsive GUI generation
- Automated configuration management
- A powerful ChatBox design system
- An in-built router to work with other OSC apps
- Program modularity for easy development
- Automatic updates
- Common API interfaces
- Drag-and-drop Unity prefabs

## Getting Started
- Download `VRCOSCSetup.exe` from the [Releases](https://github.com/VolcanicArts/VRCOSC/releases/latest) page
- Tick the modules you'd like to use
- Download any prefabs you want and add them to your avatar (Guides are available inside each prefab)
- Press the run button!

Check the [FAQ](https://github.com/VolcanicArts/VRCOSC/wiki/FAQ) if you have any issues with installing or using any of the prefabs.

## Official Modules
If you have a module idea join the [Discord Server](https://discord.gg/vj4brHyvT5) and tell us or [create it yourself](https://github.com/VolcanicArts/VRCOSC/wiki/Module-Creation)!

Note: The VRCOSC-Controls.unitypackage prefab is global controls for VRCOSC. It does not require a module to use.

All prefabs listed can be downloaded from the [Releases](https://github.com/VolcanicArts/VRCOSC/releases/latest) page

| Module | Description | Prefab |
| :---: | :--- | :---: |
| Media | Allows for media display in the ChatBox and full control over media from your action menu | VRCOSC-Media.unitypackage |
| Pulsoid [![pulsoid](https://pulsoid.net/s/github-badge)](https://pulsoid.net/) | Connects to [Pulsoid](https://pulsoid.net/) to display your live heartrate in-game - [Supported Devices](https://www.blog.pulsoid.net/monitors) | VRCOSC-Heartrate.unitypackage |
| HypeRate | Connects to [HypeRate.io](https://www.hyperate.io/) to display your live heartrate in-game - [Supported Devices](https://www.hyperate.io/supported-devices) | VRCOSC-Heartrate.unitypackage |
| Speech To Text | Run Speech To Text using any language model and display the result in your ChatBox | |
| Clock | Sends your local time as hours, minutes, and seconds to be displayed on a wrist watch | VRCOSC-Watch.unitypackage |
| PiShock | Allow you to control groups of PiShock shockers directly from your avatar | VRCOSC-PiShock.unitypackage |
| Hardware Stats | Gather's hardware stats to send to your avatar and display in the ChatBox | |
| Haptic Control | Allows for triggering controller haptics using variables from your avatar | |
| OpenVR Statistics | Gets statistics from your OpenVR (SteamVR) session | VRCOSC-Trackers.unitypackage |
| OpenVR Controller Statistics | Gets controller statistics from your OpenVR (SteamVR) session | |
| Gesture Extensions | Allows for custom gestures to be sent to VRChat from your Index controllers | |
| AFK Display | Displays how long you've been AFK for in the ChatBox | |
| Process Manager | Open and close apps on your PC using avatar parameters | |
| Weather | Gets weather from a postcode/zipcode/city to display in the ChatBox | |
| Counter | Counts how many times a parameter on your avatar changes to be displayed in the ChatBox | |
| ChatBox Text | Displays custom text in the ChatBox that can also function like a ticker tape | |
| Discord | Allows for toggling of mute and deafen from the action menu | VRCOSC-Discord.unitypackage |
| Exchange Rate | Retrieves exchange rate information for currencies and displays them in the ChatBox | |

## License
This program is licensed under the [GNU General Public License V3](https://www.gnu.org/licenses/gpl-3.0.en.html). Please see [the license file](LICENSE) for more information.

Other libraries included in this project may contain different licenses. See the license files in their repos for more information.
