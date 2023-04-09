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
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ChatBox.SelectedClip;

public partial class SelectedClipModuleSelector : Container
{
    [Resolved]
    private Bindable<Clip?> selectedClip { get; set; } = null!;

    [Resolved]
    private GameManager gameManager { get; set; } = null!;

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

        foreach (var module in gameManager.ModuleManager)
        {
            DrawableAssociatedModule drawableAssociatedModule;

            moduleFlow.Add(drawableAssociatedModule = new DrawableAssociatedModule
            {
                ModuleName = module.Title
            });

            drawableAssociatedModule.State.BindValueChanged(e =>
            {
                if (e.NewValue)
                {
                    selectedClip.Value!.AssociatedModules.Add(module.Title);
                }
                else
                {
                    selectedClip.Value!.AssociatedModules.Remove(module.Title);
                }
            });
        }

        selectedClip.BindValueChanged(e =>
        {
            if (e.NewValue is null) return;

            foreach (string newModule in e.NewValue.AssociatedModules)
            {
                moduleFlow.Add(new DrawableAssociatedModule
                {
                    ModuleName = newModule,
                    State = { Value = true }
                });
            }
        }, true);
    }
}
