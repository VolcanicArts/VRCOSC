// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.App;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Modules.Avatar;

namespace VRCOSC.Game.Graphics.ChatBox.SelectedClip;

public partial class SelectedClipModuleSelector : Container
{
    [Resolved]
    private AppManager appManager { get; set; } = null!;

    private FillFlowContainer<DrawableAssociatedModule> moduleFlow = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
        Masking = true;
        CornerRadius = 10;
        BorderThickness = 2;
        BorderColour = ThemeManager.Current[ThemeAttribute.Border];

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
                                Text = "Select Modules",
                                Font = FrameworkFont.Regular.With(size: 25),
                                Colour = ThemeManager.Current[ThemeAttribute.Text]
                            }
                        },
                        null,
                        new Drawable[]
                        {
                            new BasicScrollContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                ClampExtension = 5,
                                ScrollbarVisible = false,
                                ScrollContent =
                                {
                                    Child = moduleFlow = new FillFlowContainer<DrawableAssociatedModule>
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(0, 2)
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        appManager.ChatBoxManager.SelectedClip.BindValueChanged(e =>
        {
            if (e.NewValue is null) return;

            var newClip = e.NewValue;

            moduleFlow.Clear();

            foreach (var module in appManager.ModuleManager.Modules.Where(module => module.GetType().IsSubclassOf(typeof(ChatBoxModule))))
            {
                DrawableAssociatedModule drawableAssociatedModule;

                moduleFlow.Add(drawableAssociatedModule = new DrawableAssociatedModule
                {
                    ModuleName = module.Title
                });

                foreach (string moduleName in newClip.AssociatedModules)
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
