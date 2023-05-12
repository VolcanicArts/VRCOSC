// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.Settings.Cards;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Graphics.UI.Text;

namespace VRCOSC.Game.Graphics.Settings;

public abstract partial class SectionContainer : Container
{
    private const int setting_height = 40;

    private FillFlowContainer flow = null!;

    protected virtual string Title => string.Empty;

    protected VRCOSCConfigManager ConfigManager = null!;

    [BackgroundDependencyLoader]
    private void load(VRCOSCConfigManager configManager)
    {
        ConfigManager = configManager;

        RelativeSizeAxes = Axes.Both;

        Children = new Drawable[]
        {
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                BorderThickness = 2,
                BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                CornerRadius = 10,
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
                        Padding = new MarginPadding(5),
                        Child = new GridContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(GridSizeMode.Absolute, 5),
                                new Dimension()
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new SpriteText
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Font = FrameworkFont.Regular.With(size: 30),
                                        Colour = ThemeManager.Current[ThemeAttribute.Text],
                                        Text = Title
                                    }
                                },
                                null,
                                new Drawable[]
                                {
                                    flow = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(0, 5)
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        GenerateItems();
    }

    protected abstract void GenerateItems();

    protected void AddToggle(string title, string description, Bindable<bool> settingBindable)
    {
        flow.Add(new ToggleSettingCard(title, description, settingBindable));
    }

    protected void AddTextBox<TTextBox, TSetting>(string title, string description, Bindable<TSetting> settingBindable) where TTextBox : ValidationTextBox<TSetting>, new()
    {
        flow.Add(new TextSettingCard<TTextBox, TSetting>(title, description, settingBindable));
    }

    protected void AddDropdown<T>(string title, string description, Bindable<T> settingBindable)
    {
        flow.Add(new DropdownSettingCard<T>(title, description, settingBindable));
    }

    protected void AddButton(string text, Colour4 colour, Action? action = null)
    {
        flow.Add(new Container
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            Size = new Vector2(0.5f, setting_height),
            Child = new TextButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                CornerRadius = 5,
                BackgroundColour = colour,
                Text = text,
                Action = action,
                BorderColour = ThemeManager.Current[ThemeAttribute.Border],
                BorderThickness = 2
            }
        });
    }
}
