// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.ChatBox;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.ChatBox.Timeline;

public partial class TimelineMetadataEditor : Container
{
    [Resolved]
    private ChatBoxManager chatBoxManager { get; set; } = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        // TODO - Add snap resolution

        Children = new Drawable[]
        {
            new Box
            {
                Colour = ThemeManager.Current[ThemeAttribute.Dark],
                RelativeSizeAxes = Axes.Both,
            },
            new FillFlowContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(10),
                Spacing = new Vector2(0, 10),
                Children = new Drawable[]
                {
                    new SpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Font = FrameworkFont.Regular.With(size: 30),
                        Text = "Timeline"
                    },
                    // new MetadataTime
                    // {
                    //     Anchor = Anchor.TopCentre,
                    //     Origin = Anchor.TopCentre,
                    //     Label = "Length",
                    //     Current = chatBoxManager.TimelineLength
                    // }
                }
            }
        };
    }
}
