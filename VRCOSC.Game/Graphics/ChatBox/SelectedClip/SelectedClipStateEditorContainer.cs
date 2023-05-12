// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osuTK;
using VRCOSC.Game.Graphics.Themes;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Managers;

namespace VRCOSC.Game.Graphics.ChatBox.SelectedClip;

public partial class SelectedClipStateEditorContainer : Container
{
    private const string chatbox_v3_wiki_url = @"https://github.com/VolcanicArts/VRCOSC/wiki/ChatBox-V3#clips";

    [Resolved]
    private GameHost host { get; set; } = null!;

    [Resolved]
    private ChatBoxManager chatBoxManager { get; set; } = null!;

    private Container statesTitle = null!;
    private FillFlowContainer<DrawableState> stateFlow = null!;
    private LineSeparator separator = null!;
    private Container eventsTitle = null!;
    private FillFlowContainer<DrawableEvent> eventFlow = null!;

    private readonly Bindable<bool> showRelevantStates = new(true);

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
        Masking = true;
        CornerRadius = 10;
        BorderThickness = 2;
        BorderColour = ThemeManager.Current[ThemeAttribute.Border];

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
                Padding = new MarginPadding
                {
                    Horizontal = 10,
                    Top = 10,
                    Bottom = 50
                },
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
                                    stateFlow = new FillFlowContainer<DrawableState>
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
                                    eventFlow = new FillFlowContainer<DrawableEvent>
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
            },
            new Container
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.X,
                Height = 50,
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                RelativeSizeAxes = Axes.Both,
                                FillMode = FillMode.Fit,
                                Padding = new MarginPadding(5),
                                Children = new Drawable[]
                                {
                                    new ToggleButton
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        RelativeSizeAxes = Axes.Both,
                                        State = showRelevantStates.GetBoundCopy()
                                    }
                                }
                            },
                            new SpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Text = "Show relevant states only (Based on enabled modules)",
                                Colour = ThemeManager.Current[ThemeAttribute.SubText]
                            }
                        }
                    }
                }
            },
            new Container
            {
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Size = new Vector2(40),
                Padding = new MarginPadding(3),
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
                    IconPadding = 6,
                    Action = () => host.OpenUrlExternally(chatbox_v3_wiki_url)
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

        ((ModuleManager)chatBoxManager.GameManager.ModuleManager).OnModuleEnabledChanged += filterFlows;
        showRelevantStates.BindValueChanged(_ => filterFlows(), true);
    }

    private void filterFlows()
    {
        if (chatBoxManager.SelectedClip.Value is null) return;

        if (showRelevantStates.Value)
        {
            var enabledModuleNames = chatBoxManager.GameManager.ModuleManager.GetEnabledModuleNames().Where(moduleName => chatBoxManager.SelectedClip.Value!.AssociatedModules.Contains(moduleName)).ToList();
            enabledModuleNames.Sort();

            stateFlow.ForEach(drawableState =>
            {
                var sortedClipModuleNames = drawableState.ClipState.ModuleNames;
                sortedClipModuleNames.Sort();

                drawableState.Alpha = enabledModuleNames.SequenceEqual(sortedClipModuleNames) ? 1 : 0;
            });

            eventFlow.ForEach(drawableEvent =>
            {
                var moduleName = drawableEvent.ClipEvent.Module;
                drawableEvent.Alpha = enabledModuleNames.Contains(moduleName) ? 1 : 0;
            });
        }
        else
        {
            stateFlow.ForEach(drawableState => drawableState.Alpha = 1);
            eventFlow.ForEach(drawableState => drawableState.Alpha = 1);
        }
    }

    private void associatedModulesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs? e)
    {
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
                BorderThickness = 2,
                BorderColour = ThemeManager.Current[ThemeAttribute.Border],
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
                CornerRadius = 5,
                BorderThickness = 2,
                BorderColour = ThemeManager.Current[ThemeAttribute.Border],
            });
        });

        statesTitle.Alpha = stateFlow.Children.Count == 0 ? 0 : 1;
        eventsTitle.Alpha = eventFlow.Children.Count == 0 ? 0 : 1;
        separator.Alpha = stateFlow.Children.Count == 0 || eventFlow.Children.Count == 0 ? 0 : 1;

        filterFlows();
    }
}
