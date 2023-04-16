// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.ChatBox.SelectedClip;

public partial class SelectedClipStateEditorWrapper : Container
{
    [Resolved]
    private Bindable<Clip?> selectedClip { get; set; } = null!;

    private Container stateEditorContainer = null!;
    private Container variableContainer = null!;

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
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(5),
                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 5),
                        new Dimension(GridSizeMode.Relative, 0.25f),
                    },
                    Content = new[]
                    {
                        new Drawable?[]
                        {
                            stateEditorContainer = new Container
                            {
                                RelativeSizeAxes = Axes.Both
                            },
                            null,
                            variableContainer = new Container
                            {
                                RelativeSizeAxes = Axes.Both
                            }
                        }
                    }
                }
            }
        };

        selectedClip.BindValueChanged(clip =>
        {
            if (clip.NewValue is null) return;

            stateEditorContainer.Child = new SelectedClipStateEditorContainer(clip.NewValue)
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 5
            };

            variableContainer.Child = new SelectedClipVariableContainer(clip.NewValue)
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 5
            };
        }, true);
    }
}
