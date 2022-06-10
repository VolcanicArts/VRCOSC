// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Containers.UI;
using VRCOSC.Game.Graphics.Containers.UI.Button;
using VRCOSC.Game.Graphics.Drawables;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleCardScreen;

public class ModuleListingFooter : Container
{
    [Resolved]
    private ScreenManager ScreenManager { get; set; }

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new LineSeparator
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.X,
                Size = new Vector2(0.95f, 5),
                Colour = Colour4.Black.Opacity(0.5f),
            },
            new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(7),
                Child = new IconButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    CornerRadius = 5,
                    FillMode = FillMode.Fit,
                    FillAspectRatio = 4,
                    BackgroundColour = { Value = VRCOSCColour.GreenDark },
                    Icon = { Value = FontAwesome.Solid.Play },
                    Action = ScreenManager.ShowTerminal
                }
            }
        };
    }
}
