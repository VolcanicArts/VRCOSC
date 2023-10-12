// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using VRCOSC.Game.Screens.Main.Tabs;

namespace VRCOSC.Game.Screens.Main;

public partial class MainScreen : Screen
{
    [BackgroundDependencyLoader]
    private void load()
    {
        AddInternal(new TabContainer
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            Padding = new MarginPadding
            {
                Left = 50
            }
        });

        AddInternal(new TabBar
        {
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
            RelativeSizeAxes = Axes.Y,
            Width = 50
        });
    }

    public override void OnEntering(ScreenTransitionEvent e)
    {
        this.FadeInFromZero(1000, Easing.OutQuint);
    }
}
