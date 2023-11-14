// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics;
using VRCOSC.Game.OSC.VRChat;

namespace VRCOSC.Game.Screens.Main.Run;

public partial class ParameterList : Container
{
    private readonly string title;

    [Resolved]
    private AppManager appManager { get; set; } = null!;

    private FillFlowContainer flowWrapper = null!;
    private BasicScrollContainer scrollContainer = null!;
    private FillFlowContainer<DrawableParameter> listingFlow = null!;
    private Box endCap = null!;

    private readonly Dictionary<string, DrawableParameter> listingCache = new();

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
                                    Direction = FillDirection.Vertical
                                }
                            }
                        }
                    },
                    endCap = new Box
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
    }

    protected override void UpdateAfterChildren()
    {
        endCap.Alpha = listingFlow.Any() ? 1 : 0;

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
            var newDrawableParameter = new DrawableParameter(message.Address, message.ParameterValue, listingFlow.Count % 2 == 1);
            listingCache.Add(message.Address, newDrawableParameter);
            listingFlow.Add(newDrawableParameter);
        }
    }
}
