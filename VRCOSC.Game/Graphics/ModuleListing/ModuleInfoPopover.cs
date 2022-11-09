// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Graphics.ModuleListing;

public class ModuleInfoPopover : PopoverScreen
{
    private readonly SpriteText title;
    private readonly TextFlowContainer description;
    private readonly ParameterContainer outgoingParameters;
    private readonly ParameterContainer inputParameters;

    [Resolved(name: "InfoModule")]
    private Bindable<Module?> infoModule { get; set; } = null!;

    public ModuleInfoPopover()
    {
        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray4
            },
            new FillFlowContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 5),
                Padding = new MarginPadding(5),
                Children = new Drawable[]
                {
                    title = new SpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Font = FrameworkFont.Regular.With(size: 75),
                        Margin = new MarginPadding
                        {
                            Vertical = 10
                        }
                    },
                    new Container
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Width = 0.75f,
                        Masking = true,
                        CornerRadius = 5,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Colour = VRCOSCColour.Gray2,
                                RelativeSizeAxes = Axes.Both
                            },
                            description = new TextFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Padding = new MarginPadding(5),
                                TextAnchor = Anchor.Centre
                            }
                        }
                    },
                    new GridContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        ColumnDimensions = new[]
                        {
                            new Dimension(),
                            new Dimension()
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new Container
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding(10),
                                    Child = outgoingParameters = new ParameterContainer("Sends")
                                },
                                new Container
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding(10),
                                    Child = inputParameters = new ParameterContainer("Expects")
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    private sealed class ParameterContainer : Container<ParameterData>
    {
        protected override FillFlowContainer<ParameterData> Content { get; }

        public ParameterContainer(string title)
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Masking = true;
            CornerRadius = 5;
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = VRCOSCColour.Gray3,
                    RelativeSizeAxes = Axes.Both,
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(10),
                    Children = new Drawable[]
                    {
                        new SpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = title,
                            Font = FrameworkFont.Regular.With(size: 30)
                        },
                        new VRCOSCScrollContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            Height = 500,
                            ClampExtension = 0,
                            Child = Content = new FillFlowContainer<ParameterData>
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0, 5),
                                Padding = new MarginPadding
                                {
                                    Vertical = 5,
                                    Left = 5,
                                    Right = 15
                                }
                            }
                        },
                    }
                }
            };
        }
    }

    private sealed class ParameterData : Container
    {
        public string Name { get; init; } = null!;
        public string Description { get; init; } = null!;
        public string Type { get; init; } = null!;

        public ParameterData()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Masking = true;
            CornerRadius = 5;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = VRCOSCColour.Gray2,
                    RelativeSizeAxes = Axes.Both
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding(5),
                    Children = new Drawable[]
                    {
                        new SpriteText
                        {
                            Text = $"Name: {Name}"
                        },
                        new TextFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Text = $"Description: {Description}"
                        },
                        new SpriteText
                        {
                            Text = $"Type: {Type}"
                        }
                    }
                }
            };
        }
    }

    protected override void LoadComplete()
    {
        base.LoadComplete();

        infoModule.ValueChanged += e =>
        {
            if (e.NewValue is null)
                Hide();
            else
                Show();
        };
    }

    public override void Show()
    {
        title.Text = $"{infoModule.Value!.Title} Info";
        description.Text = infoModule.Value.Description;

        outgoingParameters.Clear();
        inputParameters.Clear();

        infoModule.Value.OutgoingParameters.Values.ForEach(parameter =>
        {
            outgoingParameters.Add(new ParameterData
            {
                Name = parameter.Name,
                Description = parameter.Description,
                Type = parameter.ExpectedType.ToReadableName()
            });
        });

        infoModule.Value.IncomingParameters.Values.ForEach(parameter =>
        {
            inputParameters.Add(new ParameterData
            {
                Name = parameter.Name,
                Description = parameter.Description,
                Type = parameter.ExpectedType.ToReadableName()
            });
        });

        base.Show();
    }

    public override void Hide()
    {
        base.Hide();
        infoModule.Value = null;
    }
}
