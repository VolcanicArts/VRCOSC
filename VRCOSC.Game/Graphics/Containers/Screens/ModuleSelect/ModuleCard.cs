// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Containers.UI.Button;
using VRCOSC.Game.Graphics.Containers.UI.Checkbox;
using VRCOSC.Game.Graphics.Drawables.Triangles;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleSelect;

public sealed class ModuleCard : Container
{
    [Resolved]
    private ScreenManager ScreenManager { get; set; }

    public Module SourceModule { get; init; }

    public ModuleCard()
    {
        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        Size = new Vector2(350, 200);
        Masking = true;
        CornerRadius = 10;
        BorderThickness = 2;
        EdgeEffect = VRCOSCEdgeEffects.BasicShadow;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Checkbox checkbox;
        Container experimentalTag;
        IconButton editButton;

        Children = new Drawable[]
        {
            new TrianglesBackground
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ColourLight = SourceModule.Colour,
                ColourDark = SourceModule.Colour.Darken(0.25f),
                TriangleScale = 2,
                Velocity = 0.8f
            },
            experimentalTag = new Container
            {
                Anchor = Anchor.TopLeft,
                Origin = Anchor.TopLeft,
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.18f),
                FillMode = FillMode.Fit,
                Padding = new MarginPadding(4),
                Depth = float.MinValue,
                Child = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderThickness = 2,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Colour = VRCOSCColour.Red
                        },
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(4),
                            Child = new SpriteIcon
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Icon = FontAwesome.Solid.Exclamation,
                                Shadow = true
                            }
                        }
                    }
                }
            },
            new GridContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 35),
                    new Dimension(GridSizeMode.Absolute, 100),
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
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = ColourInfo.GradientVertical(VRCOSCColour.Invisible, VRCOSCColour.Gray0.Opacity(0.75f))
                                },
                                new Container
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding(7),
                                    Children = new Drawable[]
                                    {
                                        editButton = new IconButton
                                        {
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            RelativeSizeAxes = Axes.Both,
                                            Icon = { Value = FontAwesome.Solid.Get(0xF013) },
                                            FillMode = FillMode.Fit,
                                            CornerRadius = 10,
                                            Action = () => ScreenManager.EditModule(SourceModule)
                                        },
                                        checkbox = new Checkbox
                                        {
                                            Anchor = Anchor.BottomRight,
                                            Origin = Anchor.BottomRight,
                                            RelativeSizeAxes = Axes.Both,
                                            FillMode = FillMode.Fit,
                                            CornerRadius = 10,
                                            State = { Value = SourceModule.DataManager.Enabled },
                                            IconOn = FontAwesome.Solid.PowerOff,
                                            IconOff = FontAwesome.Solid.PowerOff
                                        }
                                    }
                                }
                            }
                        },
                    }
                }
            }
        };

        checkbox.State.BindValueChanged(e =>
        {
            SourceModule.DataManager.SetEnabled(e.NewValue);
        });

        if (!SourceModule.Experimental) experimentalTag.Hide();

        if (!SourceModule.HasAttributes) editButton.Enabled.Value = false;
    }
}
