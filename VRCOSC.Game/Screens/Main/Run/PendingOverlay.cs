// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK;
using VRCOSC.Graphics;
using VRCOSC.Graphics.UI;

namespace VRCOSC.Screens.Main.Run;

public partial class PendingOverlay : VisibilityContainer
{
    [Resolved]
    private AppManager appManager { get; set; } = null!;

    protected override bool OnMouseDown(MouseDownEvent e) => true;
    protected override bool OnClick(ClickEvent e) => true;
    protected override bool OnHover(HoverEvent e) => true;
    protected override bool OnScroll(ScrollEvent e) => true;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;

        Child = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Size = new Vector2(750, 150),
            Masking = true,
            CornerRadius = 10,
            BorderColour = Colours.BLACK,
            BorderThickness = 3,
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colours.GRAY4
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(3),
                    Children = new Drawable[]
                    {
                        new TextFlowContainer(t =>
                        {
                            t.Colour = Colours.WHITE0;
                            t.Font = Fonts.REGULAR.With(size: 35);
                        })
                        {
                            RelativeSizeAxes = Axes.Both,
                            TextAnchor = Anchor.TopCentre,
                            Padding = new MarginPadding(5),
                            Text = "Waiting for VRChat or Unity"
                        },
                        new Container
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            RelativeSizeAxes = Axes.X,
                            Height = 50,
                            Padding = new MarginPadding(7),
                            Children = new Drawable[]
                            {
                                new TextButton
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    RelativeSizeAxes = Axes.Y,
                                    Width = 250,
                                    BackgroundColour = Colours.RED0,
                                    TextContent = "Cancel",
                                    TextFont = Fonts.REGULAR.With(size: 27),
                                    CornerRadius = 7,
                                    Action = () => appManager.CancelStartRequest()
                                },
                                new TextButton
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    RelativeSizeAxes = Axes.Y,
                                    Width = 250,
                                    BackgroundColour = Colours.GREEN0,
                                    TextContent = "Start Anyway",
                                    TextFont = Fonts.REGULAR.With(size: 27),
                                    CornerRadius = 7,
                                    Action = () => appManager.ForceStart()
                                }
                            }
                        }
                    }
                }
            }
        };
    }

    protected override void PopIn()
    {
        this.FadeIn(250, Easing.OutCubic);
    }

    protected override void PopOut()
    {
        this.FadeOut(250, Easing.OutCubic);
    }
}
