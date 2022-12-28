<div align="center">

# VRCOSC
A modular OSC program creator made for [VRChat](https://vrchat.com) built on top of the [osu!framework](https://github.com/ppy/osu-framework).

<img src="https://user-images.githubusercontent.com/29819296/206914863-700474f7-58b9-435d-aae4-36babc03c84e.png" width=70% height=70%>

[![release version](https://img.shields.io/github/v/release/VolcanicArts/VRCOSC?style=for-the-badge)](https://github.com/VolcanicArts/VRCOSC/releases/latest)
[![downloads](https://img.shields.io/github/downloads/VolcanicArts/VRCOSC/total?style=for-the-badge&label=Downloads%20Total)](https://github.com/VolcanicArts/VRCOSC/releases/latest)
[![downloads@latest](https://img.shields.io/github/downloads/VolcanicArts/VRCOSC/latest/total?style=for-the-badge&label=Downloads%20For%20Latest)](https://github.com/VolcanicArts/VRCOSC/releases/latest)
[![commits](https://img.shields.io/github/commit-activity/m/VolcanicArts/VRCOSC?style=for-the-badge)](https://github.com/VolcanicArts/VRCOSC/commits/main)
<br>
[![discord](https://discordapp.com/api/guilds/1000862183963496519/widget.png?style=shield)](https://discord.gg/vj4brHyvT5)

</div>

VRCOSC is intended to act as a wrapper around VRChat's OSC system to make creating OSC programs easier, provide interfaces for other APIs and frameworks that might be useful to an OSC project, and act as a centralised source for useful OSC programs so that a user will only need to download a single application.

Featuring:
- Responsive GUI generation
- Automated configuration management
- Program modularity for easy development
- Automatic updates
- Common API interfaces
- Drag-and-drop Unity prefabs

If you like VRCOSC, please star the repo. It really helps!

We have a [Discord Server](https://discord.gg/vj4brHyvT5) for posting suggestions or to get help with anything to do with VRCOSC.

If you'd like to make your own module, check the [Wiki](https://github.com/VolcanicArts/VRCOSC/wiki/Module-Creation) page.

## Getting Started
- Download `VRCOSCSetup.exe` from the [Releases](https://github.com/VolcanicArts/VRCOSC/releases/latest) page
- Tick the modules you'd like to use
- Download any prefabs you want and add them to your avatar (Guides are available inside each prefab)
- Press the run button!

Check the [Prefab FAQ](https://github.com/VolcanicArts/VRCOSC/discussions/16) if you have any issues with installing or using any of the prefabs.

## Modules
If you have a module idea, join the [Discord Server](https://discord.gg/vj4brHyvT5) and tell us!

P.S. The VRCOSC-Controls.unitypackage prefab is global controls for VRCOSC. It does not require a module to use.

| Module | Description | Notes | Prefab |
| :---: | :---: |:---:| :---: |
| Media | Windows Media integration. Allows for full control over Windows Media from your action menu | Previously Spotify integration | VRCOSC-Media.unitypackage |
| Hardware Stats | Displays your hardware's stats in the ChatBox | Requires VRCOSC to be run as administrator to display CPU temps | |
| HypeRate | Connects to [HypeRate.io](https://www.hyperate.io/) to display your live heartrate in-game | [Supported Devices](https://www.hyperate.io/supported-devices). Compatible with WearOS, Apple Watch, and all major dedicated heartrate monitors | VRCOSC-Heartrate.unitypackage |
| Pulsoid | Connects to [Pulsoid](https://pulsoid.net/) to display your live heartrate in-game | [Supported Devices](https://www.blog.pulsoid.net/monitors). Compatible with 200+ devices including WearOS, Apple Watch, and all dedicated heartrate monitors | VRCOSC-Heartrate.unitypackage |
| OpenVR Statistics | Gets statistics from your OpenVR (SteamVR) session | | VRCOSC-Trackers.unitypackage |
| OpenVR Controller Statistics | Gets controller statistics from your OpenVR (SteamVR) session | | |
| Gesture Extensions | Allows for custom gestures to be sent to VRChat from your Index controllers | | |
| ChatBox Text | Display custom text in the ChatBox | | |
| Clock | Sends your local time as hours, minutes, and seconds to be displayed on a wrist watch | | VRCOSC-Watch.unitypackage |
| Discord | Discord integration. Allows for toggling of mute and deafen from the action menu | Requires the Discord desktop app | VRCOSC-Discord.unitypackage |
| Random (Bool/Float/Int) | Sends a random value with adjustable update rate | | |

## License
This program is licensed under the [GNU General Public License V3](https://www.gnu.org/licenses/gpl-3.0.en.html). Please see [the license file](LICENSE) for more information.

Other libraries included in this project may contain different licenses. See the license files in their repos for more information.
