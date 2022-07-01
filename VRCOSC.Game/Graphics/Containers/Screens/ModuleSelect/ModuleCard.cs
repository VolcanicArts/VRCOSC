// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osuTK;
using VRCOSC.Game.Graphics.Containers.UI.Dynamic;
using VRCOSC.Game.Graphics.Containers.UI.Static;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleSelect;

public sealed class ModuleCard : Container, IFilterable
{
    private readonly Module sourceModule;

    public ModuleCard(Module sourceModule)
    {
        this.sourceModule = sourceModule;

        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        Size = new Vector2(350, 200);
        Masking = true;
        CornerRadius = 10;

        List<LocalisableString> filters = new List<LocalisableString>
        {
            this.sourceModule.Title,
            this.sourceModule.Author
        };
        this.sourceModule.Tags.ForEach(tag => filters.Add(new LocalisableString(tag)));
        FilterTerms = filters;
    }

    [BackgroundDependencyLoader]
    private void load(ScreenManager screenManager)
    {
        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray2
            },
            new GridContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 10),
                    new Dimension(),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new Box
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            RelativeSizeAxes = Axes.Both,
                            Colour = sourceModule.Colour
                        },
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(5),
                            Child = new GridContainer
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                RowDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.Absolute, 35),
                                    new Dimension(),
                                    new Dimension(GridSizeMode.Absolute, 65)
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
                                            Text = sourceModule.Title
                                        }
                                    },
                                    new Drawable[]
                                    {
                                        new TextFlowContainer(t =>
                                        {
                                            t.Font = FrameworkFont.Regular.With(size: 25);
                                        })
                                        {
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            RelativeSizeAxes = Axes.Both,
                                            TextAnchor = Anchor.TopCentre,
                                            Padding = new MarginPadding
                                            {
                                                Horizontal = 5
                                            },
                                            Text = sourceModule.Description
                                        },
                                    },
                                    new Drawable[]
                                    {
                                        new Container
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            RelativeSizeAxes = Axes.Both,
                                            Padding = new MarginPadding(4),
                                            Children = new Drawable[]
                                            {
                                                new Container
                                                {
                                                    Anchor = Anchor.BottomLeft,
                                                    Origin = Anchor.BottomLeft,
                                                    RelativeSizeAxes = Axes.Both,
                                                    FillMode = FillMode.Fit,
                                                    Child = new IconButton
                                                    {
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.Centre,
                                                        RelativeSizeAxes = Axes.Both,
                                                        Icon = FontAwesome.Solid.Get(0xF013),
                                                        CornerRadius = 10,
                                                        Action = () => screenManager.EditModule(sourceModule),
                                                        Enabled = { Value = sourceModule.HasAttributes }
                                                    }
                                                },
                                                new TextFlowContainer(t =>
                                                {
                                                    t.Font = FrameworkFont.Regular.With(size: 20);
                                                    t.Colour = Colour4.White.Opacity(0.9f);
                                                })
                                                {
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    RelativeSizeAxes = Axes.Both,
                                                    TextAnchor = Anchor.Centre,
                                                    Text = $"Pairs with\n{sourceModule.Prefab}",
                                                    Alpha = string.IsNullOrEmpty(sourceModule.Prefab) ? 0 : 1
                                                },
                                                new Container
                                                {
                                                    Anchor = Anchor.BottomRight,
                                                    Origin = Anchor.BottomRight,
                                                    RelativeSizeAxes = Axes.Both,
                                                    FillMode = FillMode.Fit,
                                                    Child = new StatefulIconButton
                                                    {
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.Centre,
                                                        RelativeSizeAxes = Axes.Both,
                                                        CornerRadius = 10,
                                                        State = (BindableBool)sourceModule.Enabled.GetBoundCopy(),
                                                        IconStateTrue = FontAwesome.Solid.PowerOff,
                                                        IconStateFalse = FontAwesome.Solid.PowerOff
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                    }
                }
            }
        };
    }

    public IEnumerable<LocalisableString> FilterTerms { get; }

    public bool MatchingFilter
    {
        set
        {
            if (value)
                this.FadeIn();
            else
                this.FadeOut();
        }
    }

    public bool FilteringActive { get; set; }
}
