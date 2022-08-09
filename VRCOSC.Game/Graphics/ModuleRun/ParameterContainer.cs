// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Drawables;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ModuleRun;

public sealed class ParameterContainer : Container
{
    private ParameterDisplay outgoingParameterDisplay = null!;
    private ParameterDisplay incomingParameterDisplay = null!;

    public ParameterContainer()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
    }

    [BackgroundDependencyLoader]
    private void load(ModuleManager moduleManager)
    {
        Child = new GridContainer
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
            RowDimensions = new[]
            {
                new Dimension(GridSizeMode.Absolute, 60),
                new Dimension(),
                new Dimension()
            },
            Content = new[]
            {
                new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new SpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Parameters",
                                Font = FrameworkFont.Regular.With(size: 40),
                            },
                            new LineSeparator
                            {
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Size = new Vector2(0.975f, 0.075f)
                            }
                        }
                    }
                },
                new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding
                        {
                            Horizontal = 15,
                            Bottom = 15 / 2f,
                            Top = 15
                        },
                        Child = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            BorderThickness = 3,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = VRCOSCColour.Gray2,
                                },
                                new Container
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding(1.5f)
                                },
                                outgoingParameterDisplay = new ParameterDisplay
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding
                                    {
                                        Vertical = 1.5f,
                                        Horizontal = 3
                                    },
                                    Title = "Outgoing"
                                }
                            }
                        }
                    }
                },
                new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding
                        {
                            Horizontal = 15,
                            Bottom = 15,
                            Top = 15 / 2f
                        },
                        Child = new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            BorderThickness = 3,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = VRCOSCColour.Gray2,
                                },
                                new Container
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding(1.5f)
                                },
                                incomingParameterDisplay = new ParameterDisplay
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding
                                    {
                                        Vertical = 1.5f,
                                        Horizontal = 3
                                    },
                                    Title = "Incoming"
                                }
                            }
                        }
                    }
                }
            }
        };

        moduleManager.OscClient.OnParameterSent += (key, value) => outgoingParameterDisplay.AddEntry(key, value);
        moduleManager.OscClient.OnParameterReceived += (key, value) => incomingParameterDisplay.AddEntry(key, value);
    }

    public void ClearParameters()
    {
        outgoingParameterDisplay.ClearContent();
        incomingParameterDisplay.ClearContent();
    }
}
