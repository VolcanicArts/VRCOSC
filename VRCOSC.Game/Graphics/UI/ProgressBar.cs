// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;

namespace VRCOSC.Game.Graphics.UI;

public class ProgressBar : BasicSliderBar<float>
{
    public override bool HandlePositionalInput => false;
    public override bool HandleNonPositionalInput => false;

    private string text = string.Empty;

    public string Text
    {
        get => text;
        set
        {
            text = value;
            if (loadingSpriteText is not null)
                loadingSpriteText.CurrentText.Value = text;
        }
    }

    private LoadingSpriteText? loadingSpriteText;

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

    protected override void LoadComplete()
    {
        base.LoadComplete();
        Add(loadingSpriteText = new LoadingSpriteText
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Font = FrameworkFont.Regular.With(size: Parent.Height - 4),
            CurrentText = { Value = text },
            Shadow = true
        });
    }
}
