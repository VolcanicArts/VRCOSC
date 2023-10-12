// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Screens;
using osuTK;
using osuTK.Graphics;
using VRCOSC.Game.Graphics;

namespace VRCOSC.Game.Screens.Loading;

public partial class LoadingScreen : Screen
{
    [Resolved]
    private VRCOSCGame game { get; set; } = null!;

    private SpriteText loadingAction = null!;
    private SliderBar<float> loadingBar = null!;

    [BackgroundDependencyLoader]
    private void load()
    {
        AddInternal(new Box
        {
            Colour = Colours.Dark,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both
        });

        AddInternal(new SpriteText
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.BottomCentre,
            Font = FrameworkFont.Regular.With(size: 30),
            Colour = Color4.White,
            Text = "Welcome to VRCOSC"
        });

        AddInternal(new FillFlowContainer
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.TopCentre,
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Vertical,
            Spacing = new Vector2(0, 5),
            Children = new Drawable[]
            {
                loadingAction = new SpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = FrameworkFont.Regular.With(size: 20),
                    Colour = Color4.White,
                    Text = "Easter egg, what's that?"
                },
                loadingBar = new LoadingScreenSliderBar
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Size = new Vector2(300, 15),
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

    protected override void LoadComplete()
    {
        loadingAction.Current.Value = game.LoadingAction.Value;
        loadingBar.Current.Value = game.LoadingProgress.Value;

        game.LoadingAction.ValueChanged += e => Scheduler.Add(() => loadingAction.Current.Value = e.NewValue, false);
        game.LoadingProgress.ValueChanged += e => Scheduler.Add(() => loadingBar.Current.Value = e.NewValue, false);
    }

    public override bool OnExiting(ScreenExitEvent e)
    {
        this.FadeOutFromOne(1000, Easing.OutQuint);
        return false;
    }
}
