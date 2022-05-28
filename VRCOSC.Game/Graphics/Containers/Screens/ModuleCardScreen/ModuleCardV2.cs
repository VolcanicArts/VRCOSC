// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Drawables.Triangles;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleCardScreen;

public sealed class ModuleCardV2 : Container
{
    [Resolved]
    private ScreenManager ScreenManager { get; set; }

    public Modules.Module SourceModule { get; init; }

    public ModuleCardV2()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        Size = new Vector2(350, 200);
        Masking = true;
        CornerRadius = 10;
        EdgeEffect = VRCOSCEdgeEffects.BasicShadow;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new TrianglesBackground
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ColourLight = SourceModule.Colour,
                ColourDark = SourceModule.Colour.Darken(0.25f)
            },
            new GridContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 35),
                    new Dimension(GridSizeMode.Absolute, 60),
                    new Dimension()
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new SpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Font = FrameworkFont.Regular.With(size: 35),
                            Shadow = true,
                            Text = SourceModule.Title
                        }
                    },
                    new Drawable[]
                    {
                        new TextFlowContainer(t =>
                        {
                            t.Font = FrameworkFont.Regular.With(size: 25);
                            t.Shadow = true;
                        })
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.Both,
                            TextAnchor = Anchor.TopCentre,
                            Padding = new MarginPadding(5),
                            Text = SourceModule.Description
                        },
                    },
                    new Drawable[]
                    {
                    }
                }
            }
        };
    }
}
