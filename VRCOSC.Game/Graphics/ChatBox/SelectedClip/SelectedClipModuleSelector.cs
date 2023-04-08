// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.ChatBox.SelectedClip;

public partial class SelectedClipModuleSelector : Container
{
    [Resolved]
    private Bindable<Clip?> selectedClip { get; set; } = null!;

    private FillFlowContainer moduleFlow = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new Box
            {
                Colour = ThemeManager.Current[ThemeAttribute.Dark],
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
                        new Dimension(GridSizeMode.Relative, 0.15f),
                        new Dimension(GridSizeMode.Absolute, 5),
                        new Dimension()
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new TextFlowContainer(t => t.Font = FrameworkFont.Regular.With(size: 30))
                            {
                                RelativeSizeAxes = Axes.Both,
                                Text = "Select ChatBox-enabled\nModules",
                                TextAnchor = Anchor.TopCentre
                            }
                        },
                        null,
                        new Drawable[]
                        {
                            new BasicScrollContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Child = moduleFlow = new FillFlowContainer
                                {
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 5)
                                }
                            }
                        }
                    }
                }
            }
        };

        selectedClip.BindValueChanged(e =>
        {
            if (e.NewValue is null)
            {
                e.OldValue?.AssociatedModules.UnbindBindings();
                return;
            }

            e.NewValue.AssociatedModules.BindCollectionChanged((_, e) =>
            {
                if (e.NewItems is null) return;

                moduleFlow.Clear();

                foreach (string newModule in e.NewItems)
                {
                    moduleFlow.Add(new SpriteText
                    {
                        Font = FrameworkFont.Regular.With(size: 20),
                        Text = newModule
                    });
                }
            }, true);
        }, true);
    }
}
