// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace VRCOSC.Game.Graphics.ChatBox;

public partial class SelectedClipEditorWrapper : Container
{
    [BackgroundDependencyLoader]
    private void load()
    {
        Child = new GridContainer
        {
            RelativeSizeAxes = Axes.Both,
            ColumnDimensions = new[]
            {
                new Dimension(GridSizeMode.Relative, 0.15f),
                new Dimension(GridSizeMode.Absolute, 10),
                new Dimension(GridSizeMode.Relative, 0.15f),
                new Dimension(GridSizeMode.Absolute, 10),
                new Dimension(),
            },
            Content = new[]
            {
                new Drawable?[]
                {
                    new SelectedClipMetadataEditor
                    {
                        RelativeSizeAxes = Axes.Both
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
