// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.Containers.UI;
using VRCOSC.Game.Graphics.Drawables;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing;

public class ModuleEditContent : Container
{
    [Resolved]
    private ScreenManager ScreenManager { get; set; }

    [Resolved]
    private Bindable<Module> SourceModule { get; set; }

    [BackgroundDependencyLoader]
    private void load()
    {
        SpriteText title;
        SpriteText description;
        SpriteText author;

        LineSeparator settingsSeparator;
        SettingsFlow settingsFlow;
        LineSeparator parametersSeparator;
        ParametersFlow parametersFlow;

        BasicScrollContainer scrollContainer;

        Children = new Drawable[]
        {
            new Container
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Size = new Vector2(80),
                Padding = new MarginPadding(10),
                Depth = float.MinValue,
                Child = new IconButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    CornerRadius = 10,
                    Icon = { Value = FontAwesome.Solid.Get(0xf00d) },
                    Action = ScreenManager.FinishEditingModule
                },
            },
            scrollContainer = new BasicScrollContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                ScrollbarVisible = false,
                ClampExtension = 20,
                Padding = new MarginPadding
                {
                    Horizontal = 26
                },
                Child = new FillFlowContainer
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new FillFlowContainer<SpriteText>
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 5),
                            Children = new[]
                            {
                                title = new SpriteText
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Font = FrameworkFont.Regular.With(size: 75),
                                },
                                description = new SpriteText
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Font = FrameworkFont.Regular.With(size: 40),
                                    Colour = VRCOSCColour.Gray9,
                                },
                                author = new SpriteText
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Font = FrameworkFont.Regular.With(size: 30),
                                    Colour = VRCOSCColour.Gray9,
                                }
                            }
                        },
                        settingsSeparator = new LineSeparator
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            Size = new Vector2(0.95f, 5)
                        },
                        settingsFlow = new SettingsFlow
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 10),
                            Padding = new MarginPadding(10)
                        },
                        parametersSeparator = new LineSeparator
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            Size = new Vector2(0.95f, 5)
                        },
                        parametersFlow = new ParametersFlow
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0, 10),
                            Padding = new MarginPadding(10)
                        }
                    }
                }
            }
        };

        SourceModule.BindValueChanged(_ =>
        {
            if (SourceModule.Value == null) return;

            scrollContainer.ScrollToStart();

            title.Text = SourceModule.Value.Title;
            description.Text = SourceModule.Value.Description;
            author.Text = $"Made by {SourceModule.Value.Author}";

            if (SourceModule.Value.HasSettings)
            {
                settingsSeparator.Show();
                settingsFlow.Show();
            }
            else
            {
                settingsSeparator.Hide();
                settingsFlow.Hide();
            }

            if (SourceModule.Value.HasParameters)
            {
                parametersSeparator.Show();
                parametersFlow.Show();
            }
            else
            {
                parametersSeparator.Hide();
                parametersFlow.Hide();
            }
        }, true);
    }
}
