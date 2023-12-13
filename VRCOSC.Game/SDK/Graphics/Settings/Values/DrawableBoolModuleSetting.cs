// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osuTK;
using VRCOSC.Graphics.UI;
using VRCOSC.SDK.Attributes.Settings;

namespace VRCOSC.SDK.Graphics.Settings.Values;

public partial class DrawableBoolModuleSetting : DrawableValueModuleSetting<BoolModuleSetting>
{
    public DrawableBoolModuleSetting(BoolModuleSetting moduleSetting)
        : base(moduleSetting)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        AddSide(new CheckBox
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            Size = new Vector2(40),
            State = ModuleSetting.Attribute.GetBoundCopy()
        });
    }
}
