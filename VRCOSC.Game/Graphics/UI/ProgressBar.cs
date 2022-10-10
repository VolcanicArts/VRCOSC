// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;

namespace VRCOSC.Game.Graphics.UI;

public sealed class ProgressBar : BasicSliderBar<float>
{
    public override bool HandlePositionalInput => false;
    public override bool HandleNonPositionalInput => false;

    public ProgressBar()
    {
        BackgroundColour = VRCOSCColour.Gray3;
        SelectionColour = VRCOSCColour.Gray5;
        Current = new BindableNumber<float>
        {
            MinValue = 0f,
            MaxValue = 1f,
            Precision = 0.01f
        };
    }
}
