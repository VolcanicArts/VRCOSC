using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.Graphics.Containers.Module.ModulOscParameter;
using VRCOSC.Game.Graphics.Drawables;

namespace VRCOSC.Game.Graphics.Containers.Module;

public class ModuleContainer : Container
{
    public Modules.Module SourceModule { get; init; }

    private FillFlowContainer<ModuleSettingContainer> settingsFlow { get; set; }
    private FillFlowContainer<ModuleOscParameterContainer> parameterFlow { get; set; }

    [BackgroundDependencyLoader]
    private void load()
    {
        if (SourceModule == null)
            throw new ArgumentNullException(nameof(SourceModule));

        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = VRCOSCColour.Gray3,
            },
            new Container
            {
                Name = "Content",
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding
                {
                    Vertical = 10,
                    Horizontal = 30
                },
                Child = new BasicScrollContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    ClampExtension = 20,
                    ScrollbarVisible = false,
                    Child = new FillFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(0, 10),
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            new TextFlowContainer(t => t.Font = FrameworkFont.Regular.With(size: 75))
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                TextAnchor = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Text = SourceModule.Title
                            },
                            new TextFlowContainer(t =>
                            {
                                t.Font = FrameworkFont.Regular.With(size: 30);
                                t.Colour = VRCOSCColour.Gray9;
                            })
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                TextAnchor = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Text = SourceModule.Description
                            },
                            new LineSeparator
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                Size = new Vector2(0.95f, 5)
                            },
                            new TextFlowContainer(t => t.Font = FrameworkFont.Regular.With(size: 50))
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                TextAnchor = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Text = "Settings"
                            },
                            settingsFlow = new FillFlowContainer<ModuleSettingContainer>
                            {
                                Name = "Settings",
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0, 5)
                            },
                            new LineSeparator
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                Size = new Vector2(0.95f, 5)
                            },
                            new TextFlowContainer(t => t.Font = FrameworkFont.Regular.With(size: 50))
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                TextAnchor = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Text = "Values"
                            },
                            parameterFlow = new FillFlowContainer<ModuleOscParameterContainer>
                            {
                                Name = "Values",
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0, 5)
                            }
                        }
                    }
                }
            }
        };

        SourceModule.Data.Settings.Keys.ForEach(key =>
        {
            var moduleSettingData = SourceModule.Data.Settings[key];

            switch (moduleSettingData)
            {
                case string:
                    settingsFlow.Add(new ModuleSettingStringContainer
                    {
                        Key = key,
                        SourceModule = SourceModule
                    });
                    break;

                case bool:
                    settingsFlow.Add(new ModuleSettingBoolContainer
                    {
                        Key = key,
                        SourceModule = SourceModule
                    });
                    break;

                case int:
                case long:
                    settingsFlow.Add(new ModuleSettingIntContainer
                    {
                        Key = key,
                        SourceModule = SourceModule
                    });
                    break;
            }
        });

        SourceModule.Data.Parameters.Keys.ForEach(key =>
        {
            parameterFlow.Add(new ModuleOscParameterContainer
            {
                Key = key,
                SourceModule = SourceModule
            });
        });
    }
}
