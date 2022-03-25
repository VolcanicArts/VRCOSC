// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.Containers.UI.TextBox;

namespace VRCOSC.Game.Graphics.Containers.Module;

public class ModuleSettingIntContainer : ModuleSettingContainer
{
    [BackgroundDependencyLoader]
    private void load()
    {
        VRCOSCTextBox textBox;

        SettingContainer.Child = textBox = new VRCOSCTextBox
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            RelativeSizeAxes = Axes.Both,
            BorderThickness = 3,
            Text = SourceModule.DataManager.GetSettingAs<int>(Key).ToString()
        };

        textBox.OnCommit += (_, _) =>
        {
            if (int.TryParse(textBox.Text, out var newValue))
                SourceModule.DataManager.UpdateIntSetting(Key, newValue);
        };
    }
}
