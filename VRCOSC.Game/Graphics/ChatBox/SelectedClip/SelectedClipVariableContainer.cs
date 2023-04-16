// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI;

namespace VRCOSC.Game.Graphics.ChatBox.SelectedClip;

public partial class SelectedClipVariableContainer : Container
{
    private readonly Clip? clip;
    private FillFlowContainer moduleVariableFlow = null!;

    public SelectedClipVariableContainer(Clip? clip)
    {
        this.clip = clip;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new Box
            {
                Colour = ThemeManager.Current[ThemeAttribute.Darker],
                RelativeSizeAxes = Axes.Both
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(5),
                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Relative, 0.05f),
                        new Dimension(GridSizeMode.Absolute, 5),
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
                                RelativeSizeAxes = Axes.Both,
                                Child = new SpriteText
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Text = "Available Variables",
                                    Font = FrameworkFont.Regular.With(size: 30)
                                }
                            }
                        },
                        null,
                        new Drawable[]
                        {
                            new VRCOSCScrollContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                ShowScrollbar = false,
                                ClampExtension = 5,
                                Child = moduleVariableFlow = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Padding = new MarginPadding(5),
                                    Spacing = new Vector2(0, 10)
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    protected override void LoadComplete()
    {
        clip?.AvailableVariables.BindCollectionChanged((_, _) =>
        {
            moduleVariableFlow.Clear();

            var groupedVariables = new Dictionary<string, List<ClipVariableMetadata>>();

            clip.AvailableVariables.ForEach(clipVariable =>
            {
                if (!groupedVariables.ContainsKey(clipVariable.Module)) groupedVariables.Add(clipVariable.Module, new List<ClipVariableMetadata>());
                groupedVariables[clipVariable.Module].Add(clipVariable);
            });

            groupedVariables.ForEach(pair =>
            {
                var (moduleName, clipVariables) = pair;

                moduleVariableFlow.Add(new DrawableModuleVariables(moduleName, clipVariables)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y
                });
            });
        }, true);
    }
}
