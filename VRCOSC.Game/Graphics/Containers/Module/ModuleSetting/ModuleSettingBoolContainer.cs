// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.Containers.UI.Checkbox;

namespace VRCOSC.Game.Graphics.Containers.Module;

public class ModuleSettingBoolContainer : ModuleSettingContainer
{
    [BackgroundDependencyLoader]
    private void load()
    {
        ToggleSwitch checkBox;

        SettingContainer.Child = checkBox = new ToggleSwitch
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            RelativeSizeAxes = Axes.Both,
            FillMode = FillMode.Fit,
            FillAspectRatio = 2,
            State = { Value = SourceModule.DataManager.GetSettingAs<bool>(Key) }
        };
        checkBox.State.BindValueChanged((e) => SourceModule.DataManager.UpdateBoolSetting(Key, e.NewValue));
    }
}
