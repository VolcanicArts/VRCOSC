// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using VRCOSC.Game.ChatBox;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.ChatBox.SelectedClip;

public partial class SelectedClipEditorWrapper : Container
{
    [Resolved]
    private ChatBoxManager chatBoxManager { get; set; } = null!;

    private Container noClipContent = null!;
    private GridContainer gridContent = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new Container
            {
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
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(5),
                        Children = new Drawable[]
                        {
                            noClipContent = new Container
                            {
                                Alpha = 0,
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
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
                                    new Dimension(),
                                    new Dimension(GridSizeMode.Absolute, 5),
                                    new Dimension(GridSizeMode.Relative, 0.15f),
                                },
                                Content = new[]
                                {
                                    new Drawable?[]
                                    {
                                        new SelectedClipMetadataEditor(),
                                        null,
                                        new SelectedClipModuleSelector(),
                                        null,
                                        new SelectedClipStateEditorContainer(),
                                        null,
                                        new SelectedClipVariableContainer()
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        chatBoxManager.SelectedClip.BindValueChanged(e => selectBestVisual(e.NewValue), true);
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
