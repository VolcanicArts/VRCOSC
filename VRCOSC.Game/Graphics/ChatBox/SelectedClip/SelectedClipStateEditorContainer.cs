// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.ChatBox;
using VRCOSC.Game.ChatBox.Clips;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI;

namespace VRCOSC.Game.Graphics.ChatBox.SelectedClip;

public partial class SelectedClipStateEditorContainer : Container
{
    [Resolved]
    private ChatBoxManager chatBoxManager { get; set; } = null!;

    private Clip clip;
    private Container statesTitle;
    private FillFlowContainer stateFlow;
    private Container eventsTitle;
    private FillFlowContainer eventFlow;

    public SelectedClipStateEditorContainer(Clip clip)
    {
        this.clip = clip;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new Box
            {
                Colour = ThemeManager.Current[ThemeAttribute.Darker],
                RelativeSizeAxes = Axes.Both
            },
            new VRCOSCScrollContainer
            {
                RelativeSizeAxes = Axes.Both,
                ClampExtension = 5,
                ShowScrollbar = false,
                Child = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(5),
                    Spacing = new Vector2(0, 5),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                statesTitle = new Container
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding(5),
                                    Child = new SpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Text = "States",
                                        Font = FrameworkFont.Regular.With(size: 30)
                                    }
                                },
                                stateFlow = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 5),
                                    Padding = new MarginPadding
                                    {
                                        Horizontal = 5,
                                        Bottom = 5,
                                        Top = 40
                                    }
                                }
                            }
                        },
                        new LineSeparator(),
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                eventsTitle = new Container
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Padding = new MarginPadding(5),
                                    Child = new SpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Text = "Events",
                                        Font = FrameworkFont.Regular.With(size: 30)
                                    }
                                },
                                eventFlow = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 5),
                                    Padding = new MarginPadding
                                    {
                                        Horizontal = 5,
                                        Bottom = 5,
                                        Top = 40
                                    }
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
        clip.AssociatedModules.BindCollectionChanged((_, _) =>
        {
            stateFlow.Clear();
            eventFlow.Clear();

            clip.States.ForEach(clipState =>
            {
                DrawableState drawableState;

                stateFlow.Add(drawableState = new DrawableState(clipState)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    Masking = true,
                    CornerRadius = 5,
                    Height = 40
                });
                stateFlow.SetLayoutPosition(drawableState, clipState.States.Count);
            });

            clip.Events.ForEach(clipEvent =>
            {
                eventFlow.Add(new DrawableEvent(clipEvent)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    Masking = true,
                    CornerRadius = 5,
                    Height = 40
                });
            });

            statesTitle.Alpha = stateFlow.Children.Count == 0 ? 0 : 1;
            eventsTitle.Alpha = eventFlow.Children.Count == 0 ? 0 : 1;
        }, true);
    }
}
