// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK;
using VRCOSC.Game.Graphics.Containers.UI.Static;

namespace VRCOSC.Game.Graphics.Updater;

public class VRCOSCUpdateManager : Container
{
    private Container popover;
    private ProgressBar progressBar;
    private TextButton restartButton;
    private Container progressBarContainer;
    private TextFlowContainer titleText;

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
            Size = new Vector2(400, 100),
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
                        titleText = new TextFlowContainer
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
                                FillAspectRatio = 8,
                                Masking = true,
                                CornerRadius = 10,
                                Text = "Click To Restart",
                                FontSize = 25,
                                Action = RequestRestart
                            }
                        },
                        progressBarContainer = new Container
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            RelativeSizeAxes = Axes.X,
                            Height = 25,
                            Child = progressBar = new ProgressBar
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Masking = true,
                                CornerRadius = 10,
                                BorderThickness = 2
                            }
                        }
                    }
                }
            }
        };
    }

    protected override void LoadComplete()
    {
        progressBarContainer.Hide();
        restartButton.Hide();
    }

    // Shorthand for dealing with 0-100
    public void UpdateProgress(int percentage)
    {
        UpdateProgress(percentage / 100f);
    }

    public void UpdateProgress(float percentage)
    {
        Scheduler.Add(() => updateProgress(percentage));
    }

    private void updateProgress(float percentage)
    {
        progressBar.Progress.Value = percentage;
    }

    public void SetPhase(UpdatePhase phase)
    {
        Scheduler.Add(() => setPhase(phase));
    }

    private void setPhase(UpdatePhase phase)
    {
        switch (phase)
        {
            case UpdatePhase.Download:
                enterDownloadPhase();
                break;

            case UpdatePhase.Install:
                enterInstallPhase();
                break;

            case UpdatePhase.Success:
                enterSuccessPhase();
                break;

            case UpdatePhase.Fail:
                enterFailPhase();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(phase), phase, null);
        }
    }

    public override void Show()
    {
        shown = true;
        popover.MoveToY(0, 500d, Easing.OutQuart);
    }

    private void enterDownloadPhase()
    {
        progressBarContainer.Show();
        restartButton.Hide();

        updateProgress(0f);

        titleText.Clear();
        titleText.AddText("Update Available", t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 30);
            t.Shadow = true;
        });

        progressBar.Text.Value = "Downloading";
    }

    private void enterInstallPhase()
    {
        progressBarContainer.Show();
        restartButton.Hide();

        updateProgress(0f);

        titleText.Clear();
        titleText.AddText("Update Available", t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 30);
            t.Shadow = true;
        });

        progressBar.Text.Value = "Installing";
    }

    private void enterSuccessPhase()
    {
        progressBarContainer.Hide();
        restartButton.Show();

        titleText.Clear();
        titleText.AddText("Update Complete!", t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 30);
            t.Shadow = true;
        });
    }

    private void enterFailPhase()
    {
        progressBarContainer.Hide();
        restartButton.Hide();

        titleText.Clear();
        titleText.AddText("Update Failed!", t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 30);
            t.Shadow = true;
        });
        titleText.AddParagraph("Please re-install VRCOSC from https://github.com/VolcanicArts/VRCOSC", t =>
        {
            t.Font = FrameworkFont.Regular.With(size: 20);
            t.Colour = Colour4.White.Darken(0.15f);
            t.Shadow = true;
        });
    }

    protected virtual void RequestRestart() { }

    public virtual async void CheckForUpdate(bool useDelta = true) { }

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
