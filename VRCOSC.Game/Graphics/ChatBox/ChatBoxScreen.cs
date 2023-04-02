// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.Graphics.ChatBox.SelectedClip;
using VRCOSC.Game.Graphics.ChatBox.Timeline;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.ChatBox;

public partial class ChatBoxScreen : Container
{
    [Cached]
    private Bindable<Clip?> selectedClip { get; set; } = new();

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;

        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = ThemeManager.Current[ThemeAttribute.Light]
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(10),
                Children = new Drawable[]
                {
                    new SelectedClipEditorWrapper
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.Both,
                        Height = 0.5f,
                        Padding = new MarginPadding
                        {
                            Bottom = 2.5f
                        }
                    },
                    new TimelineEditorWrapper
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        RelativeSizeAxes = Axes.Both,
                        Height = 0.5f,
                        Padding = new MarginPadding
                        {
                            Top = 2.5f,
                        }
                    }
                }
            }
        };
    }
}
