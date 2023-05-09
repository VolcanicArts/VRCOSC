// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.Graphics.ChatBox.Metadata;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Graphics.ChatBox.SelectedClip;

public partial class SelectedClipMetadataEditor : Container
{
    [Resolved]
    private ChatBoxManager chatBoxManager { get; set; } = null!;

    private FillFlowContainer metadataFlow = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
        Masking = true;
        CornerRadius = 10;

        Children = new Drawable[]
        {
            new Box
            {
                Colour = ThemeManager.Current[ThemeAttribute.Darker],
                RelativeSizeAxes = Axes.Both
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(10),
                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Relative, 0.05f),
                        new Dimension(GridSizeMode.Absolute, 10),
                        new Dimension()
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new SpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "Clip Settings",
                                Font = FrameworkFont.Regular.With(size: 30),
                                Colour = ThemeManager.Current[ThemeAttribute.Text]
                            }
                        },
                        null,
                        new Drawable[]
                        {
                            new BasicScrollContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                ScrollbarVisible = false,
                                ClampExtension = 5,
                                Child = metadataFlow = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 5)
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    protected override void LoadComplete()
    {
        chatBoxManager.SelectedClip.BindValueChanged(e => onSelectedClipChange(e.NewValue), true);
    }

    private void onSelectedClipChange(Clip? clip)
    {
        if (clip is null) return;

        metadataFlow.Clear();

        metadataFlow.Add(new MetadataToggle
        {
            Label = "Enabled",
            State = clip.Enabled.GetBoundCopy()
        });

        metadataFlow.Add(new MetadataString
        {
            Label = "Name",
            Current = clip.Name.GetBoundCopy()
        });

        metadataFlow.Add(new SpriteText
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            Text = "Metadata",
            Font = FrameworkFont.Regular.With(size: 25),
            Colour = ThemeManager.Current[ThemeAttribute.Text]
        });

        metadataFlow.Add(new ReadonlyTimeDisplay
        {
            Label = "Start",
            Current = clip.Start.GetBoundCopy()
        });

        metadataFlow.Add(new ReadonlyTimeDisplay
        {
            Label = "End",
            Current = clip.End.GetBoundCopy()
        });

        metadataFlow.Add(new ReadonlyTimeDisplay
        {
            Label = "Priority",
            Current = clip.Priority.GetBoundCopy()
        });
    }
}
