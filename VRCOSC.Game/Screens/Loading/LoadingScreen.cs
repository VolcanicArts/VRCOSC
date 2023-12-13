﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
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
using VRCOSC.Game.Actions;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Screens.Loading;

public partial class LoadingScreen : VisibilityContainer
{
    public Bindable<string> Title { get; } = new("How are you seeing this");
    public Bindable<string> Description { get; } = new("This shouldn't be possible");
    public Bindable<string> Action { get; } = new("We do love easter eggs");

    protected override bool OnMouseDown(MouseDownEvent e) => true;
    protected override bool OnClick(ClickEvent e) => true;
    protected override bool OnHover(HoverEvent e) => true;
    protected override bool OnScroll(ScrollEvent e) => true;

    public BindableNumber<float> Progress { get; } = new();

    public Bindable<ProgressAction?> CurrentAction = new();

    private SpriteText title = null!;
    private LoadingScreenSliderBar rootProgress = null!;
    private LoadingScreenSliderBar childProgress = null!;

    public LoadingScreen()
    {
        Show();
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        RelativeSizeAxes = Axes.Both;

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
                title = new SpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = FrameworkFont.Regular.With(size: 45),
                    Colour = Colours.WHITE0,
                    Text = "Sit Tight!"
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
                },
                childProgress = new LoadingScreenSliderBar
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Size = new Vector2(600, 30),
                    Current = new BindableNumber<float>
                    {
                        Value = 0,
                        MinValue = 0,
                        MaxValue = 1
                    },
                    Alpha = 0
                }
            }
        });

        CurrentAction.BindValueChanged(onCurrentActionChanged);
    }

    private void onCurrentActionChanged(ValueChangedEvent<ProgressAction?> e)
    {
        if (e.NewValue is null)
        {
            Hide();
            return;
        }

        childProgress.Alpha = e.NewValue is CompositeProgressAction ? 1f : 0f;

        Show();
    }

    protected override void Update()
    {
        if (CurrentAction.Value is null) return;

        rootProgress.TextCurrent.Value = CurrentAction.Value.Title;
        rootProgress.Current.Value = CurrentAction.Value.GetProgress();

        if (CurrentAction.Value is CompositeProgressAction compositeProgressAction)
        {
            childProgress.TextCurrent.Value = compositeProgressAction.SubTitle;
            childProgress.Current.Value = compositeProgressAction.GetSubProgress();
        }

        if (CurrentAction.Value.IsComplete) CurrentAction.Value = null;
    }

    protected override void PopIn()
    {
        this.FadeInFromZero(500, Easing.OutQuint);
    }

    protected override void PopOut()
    {
        this.FadeOutFromOne(500, Easing.OutQuint);
    }
}
