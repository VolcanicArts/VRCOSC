// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using VRCOSC.Actions;
using VRCOSC.Graphics;

namespace VRCOSC.Screens.Loading;

public partial class LoadingScreen : VisibilityContainer
{
    public Bindable<string> Title { get; } = new("How are you seeing this");
    public Bindable<string> Description { get; } = new("This shouldn't be possible");
    public Bindable<string> Action { get; } = new("We do love easter eggs");

    protected override bool OnMouseDown(MouseDownEvent e) => State.Value == Visibility.Visible;
    protected override bool OnClick(ClickEvent e) => State.Value == Visibility.Visible;
    protected override bool OnHover(HoverEvent e) => State.Value == Visibility.Visible;
    protected override bool OnScroll(ScrollEvent e) => State.Value == Visibility.Visible;

    public BindableNumber<float> Progress { get; } = new();

    private LoadingScreenSliderBar rootProgress = null!;

    private ProgressAction? currentAction;

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;
        AlwaysPresent = true;

        AddInternal(new Box
        {
            Anchor = Anchor.TopCentre,
            Origin = Anchor.TopCentre,
            RelativeSizeAxes = Axes.Both,
            Colour = Colours.BLACK.Opacity(0.5f)
        });

        AddInternal(new FillFlowContainer
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(0, 5),
            Children = new Drawable[]
            {
                new SpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = FrameworkFont.Regular.With(size: 45),
                    Colour = Colours.WHITE0,
                    Current = Title.GetBoundCopy()
                },
                new SpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = FrameworkFont.Regular.With(size: 25),
                    Colour = Colours.WHITE2,
                    Current = Description.GetBoundCopy()
                },
                rootProgress = new LoadingScreenSliderBar
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Size = new Vector2(600, 30),
                    Current = new BindableNumber<float>
                    {
                        Value = 0,
                        MinValue = 0,
                        MaxValue = 1
                    }
                }
            }
        });
    }

    protected override void Update()
    {
        if (currentAction is null) return;

        rootProgress.TextCurrent.Value = currentAction.Title;
        rootProgress.Current.Value = currentAction.GetProgress();

        if (currentAction.IsComplete) SetAction(null);
    }

    protected override void PopIn()
    {
        this.FadeInFromZero(500, Easing.OutQuint);
    }

    protected override void PopOut()
    {
        this.FadeOutFromOne(500, Easing.OutQuint);
    }

    public void SetAction(ProgressAction? action)
    {
        currentAction = action;

        if (currentAction is null)
        {
            Scheduler.Add(Hide, false);
        }
        else
        {
            Scheduler.Add(Show, false);
        }
    }
}
