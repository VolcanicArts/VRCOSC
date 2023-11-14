// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Screens.Main.Run;

public partial class ViewSwitcher : Container
{
    private Container background = null!;

    /// <summary>
    /// False: OSC View. True: Module View
    /// </summary>
    public Bindable<bool> State = new(true);

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY4
            },
            background = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                RelativePositionAxes = Axes.X,
                Width = 0.5f,
                Padding = new MarginPadding(5),
                Child = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 5,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colours.GRAY6
                    }
                }
            },
            new Container
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.Both,
                Width = 0.5f,
                Child = new SpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = Fonts.BOLD.With(size: 20),
                    Text = "OSC View"
                }
            },
            new Container
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                RelativeSizeAxes = Axes.Both,
                Width = 0.5f,
                Child = new SpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Font = Fonts.BOLD.With(size: 20),
                    Text = "Module View"
                }
            }
        };

        State.BindValueChanged(onStateChange, true);
    }

    private void onStateChange(ValueChangedEvent<bool> e)
    {
        background.MoveTo(new Vector2(e.NewValue ? 0.25f : -0.25f, 0), 150, Easing.OutQuint);
    }

    protected override bool OnClick(ClickEvent e) => State.Value = !State.Value;
}
