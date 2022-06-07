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

namespace VRCOSC.Game.Graphics.Containers.Screens.ModuleEditScreen;

public class ModuleEditInnerContainer : Container
{
    [Resolved]
    private ScreenManager ScreenManager { get; set; }

    [Resolved]
    private Bindable<Modules.Module> SourceModule { get; set; }

    [BackgroundDependencyLoader]
    private void load()
    {
        SpriteText title;
        SpriteText description;
        SpriteText author;

        LineSeparator moduleEditSettingsContainerLineSeparator;
        ModuleEditSettingsContainer moduleEditSettingsContainer;
        LineSeparator moduleEditParametersContainerLineSeparator;
        ModuleEditParametersContainer moduleEditParametersContainer;

        VRCOSCScrollContainer scrollContainer;

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
                    Icon = { Value = FontAwesome.Solid.Get(0xf00d) },
                    Action = ScreenManager.FinishEditingModule
                },
            },
            scrollContainer = new VRCOSCScrollContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding
                {
                    Horizontal = 26
                },
                ScrollContent = new Drawable[]
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
                    moduleEditSettingsContainerLineSeparator = new LineSeparator
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Size = new Vector2(0.95f, 5)
                    },
                    moduleEditSettingsContainer = new ModuleEditSettingsContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 10),
                        Padding = new MarginPadding(10)
                    },
                    moduleEditParametersContainerLineSeparator = new LineSeparator
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Size = new Vector2(0.95f, 5)
                    },
                    moduleEditParametersContainer = new ModuleEditParametersContainer
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
                moduleEditSettingsContainerLineSeparator.Show();
                moduleEditSettingsContainer.Show();
            }
            else
            {
                moduleEditSettingsContainerLineSeparator.Hide();
                moduleEditSettingsContainer.Hide();
            }

            if (SourceModule.Value.HasParameters)
            {
                moduleEditParametersContainerLineSeparator.Show();
                moduleEditParametersContainer.Show();
            }
            else
            {
                moduleEditParametersContainerLineSeparator.Hide();
                moduleEditParametersContainer.Hide();
            }
        }, true);
    }
}
