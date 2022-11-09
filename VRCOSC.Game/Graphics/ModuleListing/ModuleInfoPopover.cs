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
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Graphics.ModuleListing;

public class ModuleInfoPopover : PopoverScreen
{
    private readonly SpriteText title;
    private readonly TextFlowContainer description;
    private readonly FillFlowContainer<ParameterData> outgoingParamaters;
    private readonly FillFlowContainer<ParameterData> inputParameters;

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
            new BasicScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                ScrollbarVisible = false,
                ClampExtension = 20,
                Padding = new MarginPadding
                {
                    Horizontal = 26
                },
                Child = new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 5),
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
                            Width = 0.75f,
                            Height = 60,
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
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding(5),
                                    TextAnchor = Anchor.Centre
                                }
                            }
                        },
                        new GridContainer
                        {
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
                                    new FillFlowContainer
                                    {
                                        Padding = new MarginPadding(5),
                                        Spacing = new Vector2(0, 5),
                                        Direction = FillDirection.Vertical,
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Children = new Drawable[]
                                        {
                                            new SpriteText
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                Font = FrameworkFont.Regular.With(size: 25),
                                                Text = "Sends"
                                            },
                                            outgoingParamaters = new FillFlowContainer<ParameterData>
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Direction = FillDirection.Vertical,
                                                Spacing = new Vector2(0, 1)
                                            }
                                        }
                                    },
                                    new FillFlowContainer
                                    {
                                        Padding = new MarginPadding(5),
                                        Spacing = new Vector2(0, 5),
                                        Direction = FillDirection.Vertical,
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Children = new Drawable[]
                                        {
                                            new SpriteText
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                Font = FrameworkFont.Regular.With(size: 25),
                                                Text = "Expects"
                                            },
                                            inputParameters = new FillFlowContainer<ParameterData>
                                            {
                                                Anchor = Anchor.TopCentre,
                                                Origin = Anchor.TopCentre,
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Direction = FillDirection.Vertical,
                                                Spacing = new Vector2(0, 1)
                                            }
                                        }
                                    },
                                }
                            }
                        }
                    }
                }
            }
        };
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

        infoModule.Value.OutgoingParameters.ForEach(parameter =>
        {
            var parameterCasted = ((ModuleAttributeSingle)parameter.Value);
            var value = parameterCasted.Attribute.Value;
            var name = value.ToString()!.Remove(0, Module.VRChatOscPrefix.Length);
            var description = parameterCasted.Metadata.Description;
            var type = value.GetType().ToReadableName();

            outgoingParamaters.Add(new ParameterData
            {
                Name = name,
                Description = description,
                Type = type
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
