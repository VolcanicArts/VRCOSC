// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;
using VRCOSC.Game.Graphics.Containers.UI.Static;

namespace VRCOSC.Game.Graphics.Updater;

public class VRCOSCUpdateManager : Container
{
    private Container popover;
    private UpdateBar updateBar;

    private SpriteText updateText;
    private TextButton restartButton;
    private Container updateBarContainer;
    private TextFlowContainer mainText;

    private bool shown;

    public VRCOSCUpdateManager()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Child = popover = new Container
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Size = new Vector2(400, 250),
            Masking = true,
            EdgeEffect = VRCOSCEdgeEffects.DispersedShadow,
            RelativePositionAxes = Axes.Both,
            Position = new Vector2(0, 1),
            CornerRadius = 10,
            BorderThickness = 2,
            Children = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Colour = VRCOSCColour.Gray6
                },
                new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(5),
                    Children = new Drawable[]
                    {
                        mainText = new TextFlowContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            TextAnchor = Anchor.TopCentre,
                            RelativeSizeAxes = Axes.Both
                        },
                        new Container
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            RelativeSizeAxes = Axes.X,
                            Height = 40,
                            FillMode = FillMode.Fit,
                            FillAspectRatio = 3,
                            Padding = new MarginPadding(5),
                            Child = restartButton = new TextButton
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                BackgroundColour = VRCOSCColour.Green,
                                FillMode = FillMode.Fit,
                                FillAspectRatio = 5,
                                Masking = true,
                                CornerRadius = 10,
                                Text = "Restart!",
                                FontSize = 25,
                                Action = RequestRestart
                            }
                        },
                        updateBarContainer = new Container
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            RelativeSizeAxes = Axes.X,
                            Height = 25,
                            Children = new Drawable[]
                            {
                                updateBar = new UpdateBar
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    Masking = true,
                                    CornerRadius = 5,
                                    BorderThickness = 2
                                },
                                updateText = new SpriteText
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.BottomCentre
                                }
                            }
                        }
                    }
                }
            }
        };

        mainText.AddText("Update Available!", t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 30);
            t.Shadow = true;
        });
        mainText.AddParagraph("Downloading and installing the latest version of VRCOSC...", t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 20);
            t.Colour = Colour4.White.Darken(0.15f);
            t.Shadow = true;
        });
    }

    protected override void LoadComplete()
    {
        restartButton.Hide();
    }

    public override void Show()
    {
        Scheduler.Add(() =>
        {
            shown = true;
            popover.MoveToY(0, 500d, Easing.OutQuart);
        });
    }

    public void UpdateProgress(float percentage)
    {
        Scheduler.Add(() => updateBar.Progress.Value = percentage);
    }

    public void UpdateText(string text)
    {
        Scheduler.Add(() => updateText.Text = text);
    }

    public void ResetProgress()
    {
        Scheduler.Add(() => updateBar.Progress.Value = 0f);
    }

    public void CompleteUpdate(bool success)
    {
        Scheduler.Add(() =>
        {
            updateBarContainer.Hide();
            mainText.Clear();

            if (success)
            {
                restartButton.Show();
                mainText.AddText("Update Complete!", t =>
                {
                    t.Font = FrameworkFont.Regular.With(size: 30);
                    t.Shadow = true;
                });
            }
            else
            {
                mainText.AddText("Update Failed!", t =>
                {
                    t.Font = FrameworkFont.Regular.With(size: 30);
                    t.Shadow = true;
                });
                mainText.AddParagraph("Please re-install VRCOSC from https://github.com/VolcanicArts/VRCOSC", t =>
                {
                    t.Font = FrameworkFont.Regular.With(size: 20);
                    t.Colour = Colour4.White.Darken(0.15f);
                    t.Shadow = true;
                });
            }
        });
    }

    public virtual void RequestRestart() { }

    public virtual async Task CheckForUpdate(bool useDelta) { }

    protected override bool OnClick(ClickEvent e)
    {
        return shown;
    }

    protected override bool OnMouseDown(MouseDownEvent e)
    {
        return shown;
    }

    protected override bool OnHover(HoverEvent e)
    {
        return shown;
    }
}
