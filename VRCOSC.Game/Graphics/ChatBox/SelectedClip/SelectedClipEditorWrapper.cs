// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.ChatBox.SelectedClip;

public partial class SelectedClipEditorWrapper : Container
{
    [Resolved]
    private Bindable<Clip?> selectedClip { get; set; } = null!;

    private Container noClipContent = null!;
    private GridContainer gridContent = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            noClipContent = new Container
            {
                Alpha = 0,
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 10,
                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = ThemeManager.Current[ThemeAttribute.Dark],
                        RelativeSizeAxes = Axes.Both
                    },
                    new SpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Font = FrameworkFont.Regular.With(size: 40),
                        Text = "Select a clip to edit"
                    }
                }
            },
            gridContent = new GridContainer
            {
                Alpha = 0,
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
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = 10
                        },
                        null,
                        new SelectedClipStateEditor
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = 10
                        }
                    }
                }
            }
        };

        selectedClip.BindValueChanged(e => selectBestVisual(e.NewValue), true);
    }

    private void selectBestVisual(Clip? clip)
    {
        if (clip is null)
        {
            gridContent.FadeOut(250, Easing.OutQuad);
            noClipContent.FadeIn(250, Easing.InQuad);
        }
        else
        {
            noClipContent.FadeOut(250, Easing.OutQuad);
            gridContent.FadeIn(250, Easing.InQuad);
        }
    }
}
