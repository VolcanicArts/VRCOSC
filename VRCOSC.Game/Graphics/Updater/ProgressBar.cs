// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;

namespace VRCOSC.Game.Graphics.Updater;

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
            loadingSpriteText.CurrentText.Value = text;
        }
    }

    private LoadingSpriteText loadingSpriteText = null!;

    public ProgressBar()
    {
        BackgroundColour = VRCOSCColour.Gray3;
        SelectionColour = VRCOSCColour.Green;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Add(loadingSpriteText = new LoadingSpriteText
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Font = FrameworkFont.Regular.With(size: 18),
            Shadow = true
        });
    }
}
