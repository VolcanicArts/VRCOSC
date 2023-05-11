// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.Run;

public sealed partial class ParameterEntry : Container
{
    public string Key { get; init; } = null!;
    public Bindable<string> Value { get; } = new();

    private Box background = null!;
    private SpriteText valueText = null!;

    public ParameterEntry()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new SpriteText
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Font = FrameworkFont.Regular.With(size: 20),
                Colour = ThemeManager.Current[ThemeAttribute.SubText],
                Text = Key
            },
            new Container
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = new Colour4(0, 0, 0, 0)
                    },
                    valueText = new SpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = FrameworkFont.Regular.With(size: 20),
                        Colour = ThemeManager.Current[ThemeAttribute.SubText]
                    }
                }
            }
        };
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        Value.BindValueChanged(e =>
        {
            valueText.Text = e.NewValue;
            background.FlashColour(ThemeManager.Current[ThemeAttribute.Lighter], 500, Easing.OutCubic);
        }, true);
    }
}
