// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Specialized;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.ChatBox;
using VRCOSC.Game.Graphics.Themes;

namespace VRCOSC.Game.Graphics.ChatBox.SelectedClip;

public partial class SelectedClipStateEditorContainer : Container
{
    [Resolved]
    private ChatBoxManager chatBoxManager { get; set; } = null!;

    private Container statesTitle = null!;
    private FillFlowContainer stateFlow = null!;
    private LineSeparator separator = null!;
    private Container eventsTitle = null!;
    private FillFlowContainer eventFlow = null!;

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
                Child = new BasicScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ClampExtension = 5,
                    ScrollbarVisible = false,
                    Child = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(0, 10),
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0, 5),
                                Children = new Drawable[]
                                {
                                    statesTitle = new Container
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Child = new SpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Text = "States",
                                            Font = FrameworkFont.Regular.With(size: 30),
                                            Colour = ThemeManager.Current[ThemeAttribute.Text]
                                        }
                                    },
                                    stateFlow = new FillFlowContainer
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(0, 5)
                                    }
                                }
                            },
                            separator = new LineSeparator
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                LineColour = ThemeManager.Current[ThemeAttribute.Mid]
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(0, 5),
                                Children = new Drawable[]
                                {
                                    eventsTitle = new Container
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Child = new SpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Text = "Events",
                                            Font = FrameworkFont.Regular.With(size: 30),
                                            Colour = ThemeManager.Current[ThemeAttribute.Text]
                                        }
                                    },
                                    eventFlow = new FillFlowContainer
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
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
            }
        };
    }

    protected override void LoadComplete()
    {
        chatBoxManager.SelectedClip.BindValueChanged(e =>
        {
            if (e.OldValue is not null) e.OldValue.AssociatedModules.CollectionChanged -= associatedModulesOnCollectionChanged;

            if (e.NewValue is not null)
            {
                e.NewValue.AssociatedModules.CollectionChanged += associatedModulesOnCollectionChanged;
                associatedModulesOnCollectionChanged(null, null);
            }
        }, true);
    }

    private void associatedModulesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs? e)
    {
        // TODO Add button to filter states/events of modules where the module is relevant (enabled) or show all states/events
        //gameManager.ModuleManager.GetEnabledModuleNames();

        // Get module states of all associated modules, then filter states that contain all enabled associated modules

        stateFlow.Clear();
        eventFlow.Clear();

        // TODO - Don't regenerate whole

        chatBoxManager.SelectedClip.Value?.States.ForEach(clipState =>
        {
            DrawableState drawableState;

            stateFlow.Add(drawableState = new DrawableState(clipState)
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Masking = true,
                CornerRadius = 5,
            });
            stateFlow.SetLayoutPosition(drawableState, clipState.States.Count);
        });

        chatBoxManager.SelectedClip.Value?.Events.ForEach(clipEvent =>
        {
            eventFlow.Add(new DrawableEvent(clipEvent)
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Masking = true,
                CornerRadius = 5
            });
        });

        statesTitle.Alpha = stateFlow.Children.Count == 0 ? 0 : 1;
        eventsTitle.Alpha = separator.Alpha = eventFlow.Children.Count == 0 ? 0 : 1;
    }
}
