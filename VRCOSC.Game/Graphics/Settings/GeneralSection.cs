// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Text;

namespace VRCOSC.Game.Graphics.Settings;

public partial class GeneralSection : SectionContainer
{
    private const string chatbox_timespan_url = "https://github.com/VolcanicArts/VRCOSC/wiki/FAQ#my-quick-menu-says-ive-been-timed-out-for-spam";

    protected override string Title => "General";

    protected override void GenerateItems()
    {
        AddDropdown("Theme", "Select a theme and restart to see the effect", ConfigManager.GetBindable<ColourTheme>(VRCOSCSetting.Theme));
        AddTextBox<IntTextBox, int>("ChatBox Update Rate", "The ChatBox update rate (milliseconds)", ConfigManager.GetBindable<int>(VRCOSCSetting.ChatBoxTimeSpan), chatbox_timespan_url);
    }
}
