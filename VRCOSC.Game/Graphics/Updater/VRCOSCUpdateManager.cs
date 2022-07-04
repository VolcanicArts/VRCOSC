// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Platform;
using osuTK;
using VRCOSC.Game.Graphics.Containers.UI.Button;

namespace VRCOSC.Game.Graphics.Updater;

public class VRCOSCUpdateManager : Container
{
    private Container popover;
    private ProgressBar progressBar;
    private TextButton button;
    private LoadingSpriteText titleText;

    private bool shown;

    public bool Updating { get; private set; }

    [Resolved]
    private GameHost host { get; set; }

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
                        titleText = new LoadingSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Font = FrameworkFont.Regular.With(size: 30),
                            Shadow = true
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
                            Child = button = new TextButton
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                FillMode = FillMode.Fit,
                                FillAspectRatio = 8,
                                Masking = true,
                                CornerRadius = 10,
                                FontSize = 25
                            }
                        },
                        progressBar = new ProgressBar
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            RelativeSizeAxes = Axes.X,
                            Height = 25,
                            Masking = true,
                            CornerRadius = 10,
                            BorderThickness = 2,
                            Current = new BindableNumber<float>
                            {
                                MinValue = 0f,
                                MaxValue = 1f,
                                Precision = 0.01f
                            }
                        }
                    }
                }
            }
        };
    }

    protected override void LoadComplete()
    {
        progressBar.Hide();
        button.Hide();
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
        progressBar.Current.Value = percentage;
    }

    public void SetPhase(UpdatePhase phase)
    {
        Scheduler.Add(() => setPhase(phase));
    }

    private void setPhase(UpdatePhase phase)
    {
        updateProgress(0f);

        switch (phase)
        {
            case UpdatePhase.Check:
                enterCheckPhase();
                break;

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
        Scheduler.Add(show);
    }

    public override void Hide()
    {
        Scheduler.Add(hide);
    }

    private void show()
    {
        if (shown) return;

        Updating = true;

        shown = true;
        popover.MoveToY(0, 500d, Easing.OutQuart);
    }

    private void hide()
    {
        if (!shown) return;

        Updating = false;

        shown = false;
        popover.MoveToY(1, 500d, Easing.InQuart);
    }

    private void enterCheckPhase()
    {
        progressBar.Show();
        button.Hide();

        titleText.ShouldAnimate.Value = true;
        titleText.CurrentText.Value = "Updating";
        progressBar.Text = "Checking";
    }

    private void enterDownloadPhase()
    {
        progressBar.Show();
        button.Hide();

        titleText.ShouldAnimate.Value = true;
        titleText.CurrentText.Value = "Updating";
        progressBar.Text = "Downloading";
    }

    private void enterInstallPhase()
    {
        progressBar.Show();
        button.Hide();

        titleText.ShouldAnimate.Value = true;
        titleText.CurrentText.Value = "Updating";
        progressBar.Text = "Installing";
    }

    private void enterSuccessPhase()
    {
        progressBar.Hide();
        button.Show();
        button.Text = "Click To Restart";
        button.Action = RequestRestart;
        button.BackgroundColour = VRCOSCColour.Green;

        titleText.ShouldAnimate.Value = false;
        titleText.CurrentText.Value = "Update Complete!";
    }

    private void enterFailPhase()
    {
        progressBar.Hide();
        button.Show();
        button.Text = "Click To Reinstall";
        button.Action = () => host.OpenUrlExternally("https://github.com/VolcanicArts/VRCOSC/releases/latest");
        button.BackgroundColour = VRCOSCColour.Red;

        titleText.ShouldAnimate.Value = false;
        titleText.CurrentText.Value = "Update Failed!";
    }

    protected virtual void RequestRestart() { }

    public virtual async void CheckForUpdate(bool useDelta = true) { }

    protected override bool OnClick(ClickEvent e) => shown;
    protected override bool OnMouseDown(MouseDownEvent e) => shown;
    protected override bool OnHover(HoverEvent e) => shown;
}
