// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using VRCOSC.Game.ChatBox;
using VRCOSC.Game.Graphics.ChatBox.SelectedClip.Metadata;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.ChatBox.SelectedClip;

public partial class SelectedClipMetadataEditor : Container
{
    [Resolved]
    private Bindable<Clip?> selectedClip { get; set; } = null!;

    private FillFlowContainer metadataFlow = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new Box
            {
                Colour = ThemeManager.Current[ThemeAttribute.Dark],
                RelativeSizeAxes = Axes.Both
            },
            metadataFlow = new FillFlowContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(10),
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 5)
            }
        };

        selectedClip.BindValueChanged(e => onSelectedClipChange(e.NewValue), true);
    }

    private void onSelectedClipChange(Clip? clip)
    {
        metadataFlow.Clear();

        if (clip is null) return;

        metadataFlow.Add(new ClipMetadataToggle
        {
            Label = "Enabled",
            State = clip.Enabled
        });
    }
}
