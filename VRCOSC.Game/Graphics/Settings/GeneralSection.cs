// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Text;

namespace VRCOSC.Game.Graphics.Settings;

public partial class GeneralSection : SectionContainer
{
    private const string chatbox_timespan_url = "https://github.com/VolcanicArts/VRCOSC/wiki/FAQ#my-quick-menu-says-ive-been-timed-out-for-spam";
    private Bindable<float> uiScaleBindable = null!;
    private BindableNumber<float> uiScaleBindableNumber = null!;

    protected override string Title => "General";

    protected override void GenerateItems()
    {
        uiScaleBindable = ConfigManager.GetBindable<float>(VRCOSCSetting.UIScale);

        uiScaleBindableNumber = new BindableNumber<float>
        {
            Default = 1f,
            MinValue = 0.75f,
            MaxValue = 1.5f,
            Value = uiScaleBindable.Value
        };

        uiScaleBindableNumber.BindTo(uiScaleBindable);

        AddDropdown("Theme", "Select a theme and restart to see the effect", ConfigManager.GetBindable<VRCOSCTheme>(VRCOSCSetting.Theme));
        AddFloatSlider("UI Scaling", "Change the UI scale multiplier", uiScaleBindableNumber);
        AddTextBox<IntTextBox, int>("ChatBox Update Rate", "The ChatBox update rate (milliseconds)", ConfigManager.GetBindable<int>(VRCOSCSetting.ChatBoxTimeSpan), chatbox_timespan_url);
        AddToggle("Start In Tray", "Whether VRCOSC should start in the tray", ConfigManager.GetBindable<bool>(VRCOSCSetting.StartInTray));
    }
}
