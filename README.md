<div align="center">

# VRCOSC
A modular OSC program creator made for [VRChat](https://vrchat.com) built on top of the [osu!framework](https://github.com/ppy/osu-framework).

<img src="https://user-images.githubusercontent.com/29819296/198274038-58f9fc91-7369-4b9a-a8ad-19009d9ff1fa.png" width=70% height=70%>

[![release version](https://img.shields.io/github/v/release/VolcanicArts/VRCOSC?style=for-the-badge)](https://github.com/VolcanicArts/VRCOSC/releases/latest)
[![downloads](https://img.shields.io/github/downloads/VolcanicArts/VRCOSC/total?style=for-the-badge&label=Downloads%20Total)](https://github.com/VolcanicArts/VRCOSC/releases/latest)
[![downloads@latest](https://img.shields.io/github/downloads/volcanicarts/vrcosc/latest/total?style=for-the-badge&label=Downloads%20For%20Latest)](https://github.com/VolcanicArts/VRCOSC/releases/latest)
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
| Module | Description | Notes | Prefab |
| :---: | :---: |:---:| :---: |
| Media | Windows Media integration. Allows for complete control over Windows Media. | Previously Spotify integration | VRCOSC-Media.unitypackage |
| Discord | Discord integration. Allows for toggling of mute and deafen from the action menu | Requires the Discord desktop app | VRCOSC-Discord.unitypackage |
| HypeRate | Connects to [HypeRate.io](https://www.hyperate.io/) to display your live heartrate in-game | [Supported Devices](https://www.hyperate.io/supported-devices). Compatible with WearOS, Apple Watch, and all major dedicated heartrate monitors | VRCOSC-Heartrate.unitypackage |
| Pulsoid | Connects to [Pulsoid](https://pulsoid.net/) to display your live heartrate in-game | [Supported Devices](https://www.blog.pulsoid.net/monitors). Compatible with 200+ devices including Apple Watch and all dedicated heartrate monitors | VRCOSC-Heartrate.unitypackage |
| Clock | Sends your local time as hours, minutes, and seconds | | VRCOSC-Watch.unitypackage |
| Hardware Stats | Sends your system stats | | |
| Random (Bool/Float/Int) | Sends a random value with adjustable update rate | | |

## License
This program is licensed under the [GNU General Public License V3](https://www.gnu.org/licenses/gpl-3.0.en.html). Please see [the license file](LICENSE) for more information.

Other libraries included in this project may contain different licenses. See the license files in their repos for more information.
