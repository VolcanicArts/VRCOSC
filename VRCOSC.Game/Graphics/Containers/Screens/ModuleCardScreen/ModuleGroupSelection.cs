// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using VRCOSC.Game.Graphics.Containers.UI.Checkbox;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleCardScreen;

public class ModuleGroupSelection : Container
{
    [Resolved]
    private ModuleManager ModuleManager { get; set; }

    [Resolved]
    private ModuleSelection ModuleSelection { get; set; }

    public ModuleGroupSelection()
    {
        Masking = true;
        EdgeEffect = new EdgeEffectParameters
        {
            Colour = Color4.Black.Opacity(0.6f),
            Radius = 5f,
            Type = EdgeEffectType.Shadow
        };
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        FillFlowContainer moduleGroupFlow;
        Checkbox showExperimental;

        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray3
            },
            new GridContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 50),
                    new Dimension(),
                    new Dimension(GridSizeMode.Absolute, 75)
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new SpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = FrameworkFont.Regular.With(size: 30),
                            Text = "Group Filter"
                        }
                    },
                    new Drawable[]
                    {
                        moduleGroupFlow = new FillFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 5)
                        },
                    },
                    new Drawable[]
                    {
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
                                ColumnDimensions = new[]
                                {
                                    new Dimension(),
                                    new Dimension(),
                                    new Dimension()
                                },
                                Content = new[]
                                {
                                    new Drawable?[]
                                    {
                                        new Container
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            RelativeSizeAxes = Axes.Both,
                                            FillMode = FillMode.Fit,
                                            Padding = new MarginPadding(5),
                                            Child = showExperimental = new Checkbox
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                RelativeSizeAxes = Axes.Both
                                            }
                                        },
                                        new Container
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            RelativeSizeAxes = Axes.Both,
                                            FillMode = FillMode.Fill,
                                            FillAspectRatio = 2,
                                            Child = new TextFlowContainer(t => t.Font = FrameworkFont.Regular.With(size: 25))
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                RelativeSizeAxes = Axes.Both,
                                                TextAnchor = Anchor.Centre,
                                                Text = "Show Experimental",
                                            }
                                        },
                                        // set the 3rd column to null and have the 2nd expand to fill it
                                        // ensures a perfect 1:3 ratio
                                        null
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        ModuleManager.ForEach(moduleGroup => moduleGroupFlow.Add(new ModuleGroupCard(moduleGroup.Type)));
        showExperimental.State.BindValueChanged(show => ModuleSelection.ShowExperimental.Value = show.NewValue);
    }
}
