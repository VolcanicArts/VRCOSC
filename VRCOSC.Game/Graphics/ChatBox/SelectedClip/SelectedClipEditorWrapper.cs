// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.ChatBox;

namespace VRCOSC.Game.Graphics.ChatBox.SelectedClip;

public partial class SelectedClipEditorWrapper : Container
{
    [Cached]
    private Bindable<Clip?> selectedClip { get; set; } = new();

    [BackgroundDependencyLoader]
    private void load()
    {
        selectedClip.Value = new Clip();

        Child = new GridContainer
        {
            RelativeSizeAxes = Axes.Both,
            ColumnDimensions = new[]
            {
                new Dimension(GridSizeMode.Relative, 0.15f),
                new Dimension(GridSizeMode.Absolute, 5),
                new Dimension(GridSizeMode.Relative, 0.15f),
                new Dimension(GridSizeMode.Absolute, 5),
                new Dimension()
            },
            Content = new[]
            {
                new Drawable?[]
                {
                    new SelectedClipMetadataEditor
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 10
                    },
                    null,
                    new SelectedClipModuleSelector
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    null,
                    new SelectedClipStateEditor
                    {
                        RelativeSizeAxes = Axes.Both
                    }
                }
            }
        };
    }
}
