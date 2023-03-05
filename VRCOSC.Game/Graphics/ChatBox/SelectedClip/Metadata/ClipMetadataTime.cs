// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.Graphics.UI.Text;

namespace VRCOSC.Game.Graphics.ChatBox.SelectedClip.Metadata;

public partial class ClipMetadataTime : Container
{
    public required string Label { get; init; }
    public required Bindable<TimeSpan> Current { get; init; }

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.X;
        AutoSizeAxes = Axes.Y;

        IntTextBox inputTextBox;

        Children = new Drawable[]
        {
            new Container
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Width = 0.5f,
                Child = new SpriteText
                {
                    Font = FrameworkFont.Regular.With(size: 25),
                    Text = Label
                }
            },
            new Container
            {
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                RelativeSizeAxes = Axes.Both,
                Width = 0.5f,
                Child = inputTextBox = new IntTextBox
                {
                    RelativeSizeAxes = Axes.Both,
                    Text = ((int)Current.Value.TotalSeconds).ToString()
                }
            }
        };

        inputTextBox.OnValidEntry += value => Current.Value = TimeSpan.FromSeconds(value);
    }
}
