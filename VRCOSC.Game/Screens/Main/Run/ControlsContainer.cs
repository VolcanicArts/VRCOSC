// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Graphics.UI;

namespace VRCOSC.Game.Screens.Main.Run;

public partial class ControlsContainer : Container
{
    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY0
            },
            new FillFlowContainer
            {
                Name = "Left Flow",
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Padding = new MarginPadding(10),
                Spacing = new Vector2(10, 0),
                Children = new Drawable[]
                {
                    new IconButton
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Y,
                        Width = 150,
                        BackgroundColour = Colours.GREEN0,
                        Icon = FontAwesome.Solid.Play,
                        CornerRadius = 10
                    }
                }
            },
            new FillFlowContainer
            {
                Name = "Right Flow",
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Padding = new MarginPadding(10),
                Spacing = new Vector2(10, 0),
                Children = new Drawable[]
                {
                    new IconButton
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.Y,
                        Width = 150,
                        BackgroundColour = Colours.RED0,
                        Icon = FontAwesome.Solid.Stop,
                        CornerRadius = 10
                    },
                    new IconButton
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.Y,
                        Width = 150,
                        BackgroundColour = Colours.BLUE0,
                        Icon = FontAwesome.Solid.Redo,
                        CornerRadius = 10
                    }
                }
            }
        };
    }
}
