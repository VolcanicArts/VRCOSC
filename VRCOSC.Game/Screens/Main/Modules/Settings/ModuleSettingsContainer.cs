// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Modules.SDK;

namespace VRCOSC.Game.Screens.Main.Modules.Settings;

public partial class ModuleSettingsContainer : VisibilityContainer
{
    protected override bool OnMouseDown(MouseDownEvent e) => true;
    protected override bool OnClick(ClickEvent e) => true;
    protected override bool OnHover(HoverEvent e) => true;
    protected override bool OnScroll(ScrollEvent e) => true;

    private FillFlowContainer settingsFlow = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Child = new Container
        {
            RelativeSizeAxes = Axes.Both,
            Masking = true,
            CornerRadius = 10,
            BorderThickness = 3,
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colours.GRAY1
                },
                new Container
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(13),
                    Children = new Drawable[]
                    {
                        new TextButton
                        {
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                            RelativeSizeAxes = Axes.Y,
                            Width = 200,
                            BackgroundColour = Colours.BLUE0,
                            TextContent = "Reset To Default",
                            TextFont = Fonts.REGULAR.With(size: 25),
                            TextColour = Colours.WHITE0
                        },
                        new IconButton
                        {
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Size = new Vector2(36),
                            CornerRadius = 5,
                            Icon = FontAwesome.Solid.Undo,
                            IconSize = 24,
                            IconColour = Colours.WHITE0,
                            BackgroundColour = Colours.RED0,
                            Action = Hide
                        }
                    }
                },
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding
                    {
                        Vertical = 3
                    },
                    Child = new BasicScrollContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.Both,
                        ClampExtension = 0,
                        Width = 0.5f,
                        ScrollbarVisible = false,
                        ScrollContent =
                        {
                            Child = settingsFlow = new FillFlowContainer
                            {
                                Name = "Settings Flow",
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Padding = new MarginPadding
                                {
                                    Vertical = 10
                                },
                                Spacing = new Vector2(0, 10)
                            }
                        }
                    }
                }
            }
        };
    }

    public void SetModule(Module? module)
    {
        settingsFlow.Clear();
        if (module is null) return;

        var settingsInGroup = new List<string>();

        module.Groups.ForEach(groupPair =>
        {
            var moduleSettingsGroupContainer = new ModuleSettingsGroupContainer(groupPair.Key);

            groupPair.Value.ForEach(settingLookup =>
            {
                settingsInGroup.Add(settingLookup);

                var moduleSetting = module.Settings[settingLookup];
                moduleSettingsGroupContainer.Add(moduleSetting.GetDrawableModuleAttribute());
            });

            settingsFlow.Add(moduleSettingsGroupContainer);
        });

        var miscModuleSettingsGroupContainer = new ModuleSettingsGroupContainer(module.Groups.Any() ? "Miscellaneous" : string.Empty);
        module.Settings.Where(settingPair => !settingsInGroup.Contains(settingPair.Key))
              .Select(settingPair => settingPair.Value)
              .ForEach(moduleSetting => miscModuleSettingsGroupContainer.Add(moduleSetting.GetDrawableModuleAttribute()));

        settingsFlow.Add(miscModuleSettingsGroupContainer);
    }

    protected override void PopIn()
    {
        this.FadeInFromZero(250, Easing.OutCubic);
    }

    protected override void PopOut()
    {
        this.FadeOutFromOne(250, Easing.OutCubic).Finally(_ => SetModule(null));
    }
}
