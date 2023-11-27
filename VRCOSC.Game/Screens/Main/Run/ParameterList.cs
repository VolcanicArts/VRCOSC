// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.OSC.VRChat;

namespace VRCOSC.Game.Screens.Main.Run;

public partial class ParameterList : Container
{
    [Resolved]
    private AppManager appManager { get; set; } = null!;

    private readonly string title;

    private FillFlowContainer flowWrapper = null!;
    private BasicScrollContainer scrollContainer = null!;
    private FillFlowContainer<DrawableParameter> listingFlow = null!;

    private readonly SortedDictionary<string, DrawableParameter> listingCache = new();

    public ParameterList(string title)
    {
        this.title = title;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            flowWrapper = new FillFlowContainer
            {
                Name = "Flow Wrapper",
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Masking = true,
                CornerRadius = 5,
                Children = new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = 40,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Colours.GRAY0
                            },
                            new TextFlowContainer(t =>
                            {
                                t.Font = Fonts.BOLD.With(size: 30);
                                t.Colour = Colours.WHITE2;
                            })
                            {
                                RelativeSizeAxes = Axes.Both,
                                TextAnchor = Anchor.Centre,
                                Text = title
                            }
                        }
                    },
                    scrollContainer = new BasicScrollContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        ClampExtension = 0,
                        ScrollbarVisible = false,
                        AutoSizeEasing = Easing.OutQuint,
                        AutoSizeDuration = 100,
                        ScrollContent =
                        {
                            Children = new Drawable[]
                            {
                                listingFlow = new FillFlowContainer<DrawableParameter>
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    LayoutDuration = 100,
                                    LayoutEasing = Easing.OutQuint
                                }
                            }
                        }
                    },
                    new Box
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = 5,
                        Colour = Colours.GRAY0
                    }
                }
            }
        };

        appManager.State.BindValueChanged(onAppManagerStateChange);
    }

    private void onAppManagerStateChange(ValueChangedEvent<AppManagerState> e)
    {
        if (e.NewValue == AppManagerState.Starting)
        {
            listingCache.Clear();
            listingFlow.Clear();
        }
    }

    protected override void Update()
    {
        var depth = 0f;

        foreach (var sortedDrawableParameter in listingCache.Values)
        {
            listingFlow.ChangeChildDepth(sortedDrawableParameter, depth);
            listingFlow.SetLayoutPosition(sortedDrawableParameter, depth);
            sortedDrawableParameter.UpdateEven(depth % 2f != 0f);
            depth++;
        }
    }

    protected override void UpdateAfterChildren()
    {
        if (flowWrapper.DrawHeight >= DrawHeight)
        {
            scrollContainer.AutoSizeAxes = Axes.None;
            scrollContainer.Height = DrawHeight - 45;
        }
        else
        {
            scrollContainer.AutoSizeAxes = Axes.Y;
        }
    }

    public void UpdateParameterValue(VRChatOscMessage message)
    {
        if (listingCache.TryGetValue(message.Address, out var drawableParameter))
        {
            drawableParameter.UpdateValue(message.ParameterValue);
        }
        else
        {
            var newDrawableParameter = new DrawableParameter(message.Address, message.ParameterValue);
            listingCache.Add(message.Address, newDrawableParameter);
            listingFlow.Add(newDrawableParameter);
        }
    }
}
