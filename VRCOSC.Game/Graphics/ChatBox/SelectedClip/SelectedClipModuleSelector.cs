// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.ChatBox.SelectedClip;

public partial class SelectedClipModuleSelector : Container
{
    [Resolved]
    private Bindable<Clip?> selectedClip { get; set; } = null!;

    [Resolved]
    private GameManager gameManager { get; set; } = null!;

    private FillFlowContainer<DrawableAssociatedModule> moduleFlow = null!;

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
                Padding = new MarginPadding(10),
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
                            new VRCOSCScrollContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                ClampExtension = 20,
                                Child = moduleFlow = new FillFlowContainer<DrawableAssociatedModule>
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 20)
                                }
                            }
                        }
                    }
                }
            }
        };

        selectedClip.BindValueChanged(e =>
        {
            if (e.NewValue is null) return;

            moduleFlow.Clear();

            foreach (var module in gameManager.ModuleManager)
            {
                DrawableAssociatedModule drawableAssociatedModule;

                moduleFlow.Add(drawableAssociatedModule = new DrawableAssociatedModule
                {
                    ModuleName = module.Title
                });

                foreach (string moduleName in e.NewValue.AssociatedModules)
                {
                    if (module.SerialisedName == moduleName)
                    {
                        drawableAssociatedModule.State.Value = true;
                    }
                }

                drawableAssociatedModule.State.BindValueChanged(e =>
                {
                    if (e.NewValue)
                        selectedClip.Value!.AssociatedModules.Add(module.SerialisedName);
                    else
                        selectedClip.Value!.AssociatedModules.Remove(module.SerialisedName);
                });
            }
        }, true);
    }
}
