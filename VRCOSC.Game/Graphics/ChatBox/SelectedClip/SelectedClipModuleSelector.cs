// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.ChatBox;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Modules;
using VRCOSC.Game.Modules.ChatBox;

namespace VRCOSC.Game.Graphics.ChatBox.SelectedClip;

public partial class SelectedClipModuleSelector : Container
{
    [Resolved]
    private ChatBoxManager chatBoxManager { get; set; } = null!;

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
                                    Spacing = new Vector2(0, 10)
                                }
                            }
                        }
                    }
                }
            }
        };

        chatBoxManager.SelectedClip.BindValueChanged(e =>
        {
            if (e.NewValue is null) return;

            var newClip = e.NewValue;

            moduleFlow.Clear();

            foreach (var module in gameManager.ModuleManager.Where(module => module.GetType().IsSubclassOf(typeof(ChatBoxModule))))
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
                        newClip.AssociatedModules.Add(module.SerialisedName);
                    else
                        newClip.AssociatedModules.Remove(module.SerialisedName);
                });
            }
        }, true);
    }
}
