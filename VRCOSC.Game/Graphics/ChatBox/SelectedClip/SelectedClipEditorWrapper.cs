// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osuTK;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Graphics.ChatBox.SelectedClip;

public partial class SelectedClipEditorWrapper : Container
{
    private const string chatbox_v3_wiki_url = @"https://github.com/VolcanicArts/VRCOSC/wiki/ChatBox-V3";

    [Resolved]
    private GameHost host { get; set; } = null!;

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
                BorderThickness = 2,
                BorderColour = ThemeManager.Current[ThemeAttribute.Border],
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
                                    new TextFlowContainer(t => t.Font = FrameworkFont.Regular.With(size: 35))
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        TextAnchor = Anchor.Centre,
                                        RelativeSizeAxes = Axes.Both,
                                        Text = "Left click a clip to edit\nRight click a clip for options\nRight click the timeline for options",
                                        Colour = ThemeManager.Current[ThemeAttribute.SubText]
                                    },
                                    new Container
                                    {
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                        Size = new Vector2(80),
                                        Padding = new MarginPadding(5),
                                        Child = new IconButton
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            RelativeSizeAxes = Axes.Both,
                                            Icon = FontAwesome.Solid.Question,
                                            BackgroundColour = ThemeManager.Current[ThemeAttribute.Action],
                                            IconShadow = true,
                                            Masking = true,
                                            Circular = true,
                                            Action = () => host.OpenUrlExternally(chatbox_v3_wiki_url)
                                        }
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
                                    new Dimension(GridSizeMode.Relative, 0.2f),
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
