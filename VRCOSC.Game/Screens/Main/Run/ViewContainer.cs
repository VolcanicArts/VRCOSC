// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Screens.Main.Run;

public partial class ViewContainer : Container
{
    [Resolved]
    private AppManager appManager { get; set; } = null!;

    private GridContainer parameterView = null!;
    private ParameterList outgoingParameterList = null!;
    private ParameterList incomingParameterList = null!;

    private readonly Bindable<bool> viewState = new(true);

    [BackgroundDependencyLoader]
    private void load()
    {
        Children = new Drawable[]
        {
            new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Colours.GRAY7
            },
            new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(10),
                Child = new GridContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 10),
                        new Dimension(GridSizeMode.Absolute, 40),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                Name = "Views",
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    parameterView = new GridContainer
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        RelativeSizeAxes = Axes.Both,
                                        ColumnDimensions = new[]
                                        {
                                            new Dimension(),
                                            new Dimension(GridSizeMode.Absolute, 10),
                                            new Dimension(),
                                        },
                                        RowDimensions = new[]
                                        {
                                            new Dimension()
                                        },
                                        Content = new[]
                                        {
                                            new Drawable?[]
                                            {
                                                outgoingParameterList = new ParameterList("Outgoing")
                                                {
                                                    RelativeSizeAxes = Axes.Both
                                                },
                                                null,
                                                incomingParameterList = new ParameterList("Incoming")
                                                {
                                                    RelativeSizeAxes = Axes.Both
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        null,
                        new Drawable[]
                        {
                            new ViewSwitcher
                            {
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre,
                                RelativeSizeAxes = Axes.Y,
                                Width = 300,
                                Masking = true,
                                CornerRadius = 5,
                                State = viewState.GetBoundCopy()
                            }
                        }
                    }
                }
            }
        };

        viewState.BindValueChanged(onStateChange, true);

        appManager.VRChatOscClient.OnParameterSent += message => Schedule(() => outgoingParameterList.UpdateParameterValue(message));
        appManager.VRChatOscClient.OnParameterReceived += message => Schedule(() => incomingParameterList.UpdateParameterValue(message));
    }

    private void onStateChange(ValueChangedEvent<bool> e)
    {
        parameterView.Alpha = e.NewValue ? 0 : 1;
    }
}
