// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osuTK;
using VRCOSC.Game.Graphics.UI.Button;
using VRCOSC.Game.Graphics.Updater;

namespace VRCOSC.Game.Graphics.UpdaterV2;

public class FinishedContainer : PhaseContainer
{
    protected override float ScaleFrom => 0.9f;

    private SpriteText spriteText = null!;
    private TextButton button = null!;

    public Action? SuccessCallback;
    public Action? FailCallback;

    [BackgroundDependencyLoader]
    private void load()
    {
        Child = new FillFlowContainer
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            AutoSizeAxes = Axes.Both,
            Padding = new MarginPadding(5),
            Spacing = new Vector2(0, 5),
            Direction = FillDirection.Vertical,
            Children = new Drawable[]
            {
                spriteText = new SpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Font = FrameworkFont.Regular.With(size: 30)
                },
                new Container
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    AutoSizeAxes = Axes.Both,
                    Child = button = new TextButton
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(200, 40),
                        Masking = true,
                        CornerRadius = 5
                    }
                }
            }
        };
    }

    protected override void UpdateUsingPhase()
    {
        switch (UpdatePhase)
        {
            case UpdatePhase.Success:
                spriteText.Text = "Update Complete!";
                button.Text = "Click To Restart";
                button.BackgroundColour = VRCOSCColour.Green;
                button.Action = SuccessCallback;
                break;

            case UpdatePhase.Fail:
                spriteText.Text = "Update Failed!";
                button.Text = "Click To Reinstall";
                button.BackgroundColour = VRCOSCColour.Red;
                button.Action = FailCallback;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(updatePhase), updatePhase, $"Cannot use this update phases inside {nameof(FinishedContainer)}");
        }
    }
}
