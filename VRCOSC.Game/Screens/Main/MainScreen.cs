// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Screens.Main.Tabs;

namespace VRCOSC.Game.Screens.Main;

public partial class MainScreen : Container
{
    private const int tab_bar_size = 70;
    private BufferedContainer bufferedContainer = null!;

    public Bindable<Visibility> EnableBlur { get; init; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;

        AddInternal(bufferedContainer = new BufferedContainer
        {
            RelativeSizeAxes = Axes.Both,
            BackgroundColour = Colours.BLACK,
            Children = new Drawable[]
            {
                new TabContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding
                    {
                        Left = tab_bar_size
                    }
                },
                new TabBar
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.Y,
                    Width = tab_bar_size
                }
            }
        });

        EnableBlur.BindValueChanged(transitionBlur);
    }

    private void transitionBlur(ValueChangedEvent<Visibility> e)
    {
        bufferedContainer.TransformTo(nameof(BufferedContainer.BlurSigma), e.NewValue == Visibility.Visible ? new Vector2(5) : new Vector2(0), 500);
    }
}
