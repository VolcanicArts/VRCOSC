// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Collections.Specialized;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.ChatBox;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.ChatBox.SelectedClip;

public partial class SelectedClipVariableContainer : Container
{
    [Resolved]
    private ChatBoxManager chatBoxManager { get; set; } = null!;

    private FillFlowContainer moduleVariableFlow = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
        Masking = true;
        CornerRadius = 10;

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
                Padding = new MarginPadding(10),
                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Relative, 0.05f),
                        new Dimension(GridSizeMode.Absolute, 10),
                        new Dimension()
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new SpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Available Variables",
                                Font = FrameworkFont.Regular.With(size: 30),
                                Colour = ThemeManager.Current[ThemeAttribute.Text]
                            }
                        },
                        null,
                        new Drawable[]
                        {
                            new BasicScrollContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                ScrollbarVisible = false,
                                ClampExtension = 5,
                                Child = moduleVariableFlow = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
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
        chatBoxManager.SelectedClip.BindValueChanged(e =>
        {
            if (e.OldValue is not null) e.OldValue.AvailableVariables.CollectionChanged -= availableVariablesOnCollectionChanged;

            if (e.NewValue is not null)
            {
                e.NewValue.AvailableVariables.CollectionChanged += availableVariablesOnCollectionChanged;
                availableVariablesOnCollectionChanged(null, null);
            }
        }, true);
    }

    private void availableVariablesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs? e)
    {
        moduleVariableFlow.Clear();

        // TODO Don't regenerate whole

        var groupedVariables = new Dictionary<string, List<ClipVariableMetadata>>();

        chatBoxManager.SelectedClip.Value?.AvailableVariables.ForEach(clipVariable =>
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
    }
}
