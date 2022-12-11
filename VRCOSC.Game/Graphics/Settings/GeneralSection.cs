// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.Settings;

public partial class GeneralSection : SectionContainer
{
    protected override string Title => "General";

    protected override void GenerateItems()
    {
        AddDropdown("Theme", "Select a theme and restart to see the effect", ConfigManager.GetBindable<ColourTheme>(VRCOSCSetting.Theme));
        AddIntTextBox("ChatBox Time Span", "The delay between the ChatBox updating (milliseconds)\nIf you're experiencing ChatBox timeouts, increase this number by a few hundred milliseconds", ConfigManager.GetBindable<int>(VRCOSCSetting.ChatBoxTimeSpan));
    }
}
