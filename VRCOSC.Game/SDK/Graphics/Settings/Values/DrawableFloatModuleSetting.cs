// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using VRCOSC.Game.Graphics.UI.Text;
using VRCOSC.Game.SDK.Attributes.Settings;

namespace VRCOSC.Game.SDK.Graphics.Settings.Values;

public partial class DrawableFloatModuleSetting : DrawableValueModuleSetting<FloatModuleSetting>
{
    public DrawableFloatModuleSetting(FloatModuleSetting moduleSetting)
        : base(moduleSetting)
    {
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(new FloatTextBox
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Height = 35,
            ValidCurrent = ModuleSetting.Attribute.GetBoundCopy()
        });
    }
}
