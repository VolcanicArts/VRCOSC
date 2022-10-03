// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Config;
using VRCOSC.Game.Graphics.Settings.Cards;
using VRCOSC.Game.Graphics.UI.Button;

namespace VRCOSC.Game.Graphics.Settings;

public abstract class SectionContainer : Container
{
    private const int setting_height = 40;

    private FillFlowContainer flow = null!;

    protected virtual string Title => string.Empty;

    protected VRCOSCConfigManager ConfigManager = null!;

    [BackgroundDependencyLoader]
    private void load(VRCOSCConfigManager configManager)
    {
        ConfigManager = configManager;

        Anchor = Anchor.TopCentre;
        Origin = Anchor.TopCentre;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Width = 0.5f;
        Child = flow = new FillFlowContainer
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Padding = new MarginPadding(5),
            Spacing = new Vector2(0, 5),
        };

        flow.Add(new SpriteText
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            Font = FrameworkFont.Regular.With(size: 35),
            Text = Title
        });
    }

    protected void AddToggle(string title, string description, Bindable<bool> settingBindable)
    {
        flow.Add(new ToggleSettingCard(title, description, settingBindable));
    }

    protected void AddTextBox(string title, string description, Bindable<string> settingBindable)
    {
        flow.Add(new TextSettingCard(title, description, settingBindable));
    }

    protected void AddIntTextBox(string title, string description, Bindable<int> settingBindable)
    {
        flow.Add(new IntTextSettingCard(title, description, settingBindable));
    }

    protected void AddDropdown<T>(string title, string description, Bindable<T> settingBindable)
    {
        flow.Add(new DropdownSettingCard<T>(title, description, settingBindable));
    }

    protected override void LoadComplete()
    {
        GenerateItems();
    }

    protected abstract void GenerateItems();

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
                BorderColour = Colour4.Black,
                BorderThickness = 2
            }
        });
    }
}
