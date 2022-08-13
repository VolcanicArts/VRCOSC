// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.ModuleListing;
using VRCOSC.Game.Graphics.UI;
using VRCOSC.Game.Graphics.UI.Button;

namespace VRCOSC.Game.Graphics.Settings;

public class SectionContainer : Container
{
    private const int setting_height = 40;

    private FillFlowContainer flow = null!;

    protected virtual string Title => string.Empty;

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.TopLeft;
        Origin = Anchor.TopLeft;
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;
        Width = 0.5f;
        Child = flow = new FillFlowContainer
        {
            Anchor = Anchor.TopLeft,
            Origin = Anchor.TopLeft,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Direction = FillDirection.Vertical,
            Padding = new MarginPadding(5),
            Spacing = new Vector2(0, 5),
        };

        flow.Add(new SpriteText
        {
            Anchor = Anchor.TopLeft,
            Origin = Anchor.TopLeft,
            Font = FrameworkFont.Regular.With(size: 35),
            Text = Title
        });
    }

    protected void Add(string label, Drawable drawable)
    {
        flow.Add(new Container
        {
            Anchor = Anchor.TopLeft,
            Origin = Anchor.TopLeft,
            RelativeSizeAxes = Axes.X,
            AutoSizeAxes = Axes.Y,
            Children = new Drawable[]
            {
                new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Text = label,
                    Font = FrameworkFont.Regular.With(size: 25)
                },
                new Container
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Width = 0.5f,
                    Child = drawable
                }
            }
        });
    }

    protected override void LoadComplete()
    {
        GenerateItems();
        Load();
    }

    protected virtual void GenerateItems() { }

    protected virtual void Load() { }

    protected virtual void Save() { }

    protected VRCOSCTextBox GenerateTextBox()
    {
        var textBox = new VRCOSCTextBox
        {
            Anchor = Anchor.TopLeft,
            Origin = Anchor.TopLeft,
            RelativeSizeAxes = Axes.X,
            Height = setting_height,
            CornerRadius = 5,
            CommitOnFocusLost = true,
            ReleaseFocusOnCommit = false
        };

        textBox.OnCommit += (_, _) => Save();

        return textBox;
    }

    protected VRCOSCTextBox GenerateIntTextBox()
    {
        var textBox = GenerateTextBox();

        textBox.Current.ValueChanged += e =>
        {
            if (string.IsNullOrEmpty(e.NewValue))
            {
                textBox.Current.Value = "0";
                return;
            }

            textBox.Current.Value = int.TryParse(e.NewValue, out var intValue) ? intValue.ToString() : e.OldValue;
        };

        return textBox;
    }

    protected ToggleButton GenerateToggle()
    {
        var toggle = new ToggleButton
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            Size = new Vector2(setting_height),
            ShouldAnimate = false,
            CornerRadius = 7
        };

        toggle.State.ValueChanged += _ => Save();

        return toggle;
    }

    protected VRCOSCDropdown<T> GenerateDropdown<T>()
    {
        var dropdown = new VRCOSCDropdown<T>()
        {
            Anchor = Anchor.TopRight,
            Origin = Anchor.TopRight,
            RelativeSizeAxes = Axes.X,
            Items = Enum.GetValues(typeof(T)).Cast<T>()
        };

        dropdown.Current.ValueChanged += _ => Save();

        return dropdown;
    }

    protected VRCOSCButton GenerateButton(string text, Colour4 colour, Action? action = null)
    {
        return new TextButton
        {
            Anchor = Anchor.CentreRight,
            Origin = Anchor.CentreRight,
            RelativeSizeAxes = Axes.X,
            Width = 0.25f,
            Height = setting_height,
            Colour = colour,
            Text = text,
            Action = action
        };
    }
}
