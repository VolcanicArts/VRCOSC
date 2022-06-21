// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osuTK;
using VRCOSC.Game.Graphics.Containers.UI.Dynamic;
using VRCOSC.Game.Graphics.Containers.UI.Static;
using VRCOSC.Game.Graphics.Drawables.Triangles;
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
            new TrianglesBackground
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ColourLight = sourceModule.Colour,
                ColourDark = sourceModule.Colour.Darken(0.25f),
                TriangleScale = 2,
                Velocity = 0.8f
            },
            new GridContainer
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
                            Padding = new MarginPadding(5),
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
                                        new SpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Font = FrameworkFont.Regular.With(size: 20),
                                            Colour = Colour4.White.Opacity(0.9f),
                                            Text = $"Pairs with {sourceModule.Prefab}",
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
