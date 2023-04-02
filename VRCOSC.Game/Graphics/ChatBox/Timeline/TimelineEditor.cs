// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using VRCOSC.Game.ChatBox;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.ChatBox.Timeline;

[Cached]
public partial class TimelineEditor : Container
{
    [Resolved]
    private Bindable<Clip?> selectedClip { get; set; } = null!;

    [Resolved]
    private ChatBoxManager chatBoxManager { get; set; } = null!;

    private Container<DrawableClip> clipContainer = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new Box
            {
                Colour = ThemeManager.Current[ThemeAttribute.Dark],
                RelativeSizeAxes = Axes.Both,
            },
            clipContainer = new Container<DrawableClip>
            {
                RelativeSizeAxes = Axes.Both
            }
        };

        chatBoxManager.Clips.ForEach(clip => clipContainer.Add(new DrawableClip(clip)));
    }

    protected override bool OnClick(ClickEvent e)
    {
        selectedClip.Value = null;
        return true;
    }
}
