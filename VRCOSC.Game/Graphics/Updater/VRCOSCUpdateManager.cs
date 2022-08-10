// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Platform;

namespace VRCOSC.Game.Graphics.Updater;

public abstract class VRCOSCUpdateManager : VisibilityContainer
{
    private LoadingContainer loadingContainer = null!;
    private FinishedContainer finishedContainer = null!;

    public bool Updating => State.Value == Visibility.Visible;

    [BackgroundDependencyLoader]
    private void load(GameHost host)
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;

        Children = new Drawable[]
        {
            new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = Colour4.Black.Opacity(0.75f)
            },
            loadingContainer = new LoadingContainer(),
            finishedContainer = new FinishedContainer
            {
                SuccessCallback = RequestRestart,
                FailCallback = () => host.OpenUrlExternally("https://github.com/VolcanicArts/VRCOSC/releases/latest")
            }
        };
    }

    // Shorthand for dealing with 0-100
    protected void UpdateProgress(int percentage) => UpdateProgress(percentage / 100f);

    public void UpdateProgress(float percentage) => Schedule(() => loadingContainer.ProgressBar.Current.Value = percentage);

    public void SetPhase(UpdatePhase phase) => Schedule(() =>
    {
        UpdateProgress(0);

        switch (phase)
        {
            case UpdatePhase.Check:
            case UpdatePhase.Download:
            case UpdatePhase.Install:
                finishedContainer.Hide();
                loadingContainer.Show();
                loadingContainer.UpdatePhase = phase;
                break;

            case UpdatePhase.Success:
            case UpdatePhase.Fail:
                loadingContainer.Hide();
                finishedContainer.Show();
                finishedContainer.UpdatePhase = phase;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(phase), phase, $"Cannot use this update phase inside {nameof(LoadingContainer)}");
        }
    });

    protected override void PopIn() => Schedule(() =>
    {
        this.FadeInFromZero(250, Easing.OutQuint);
    });

    protected override void PopOut() => Schedule(() =>
    {
        this.FadeOutFromOne(250, Easing.InQuint);
    });

    protected abstract void RequestRestart();
    public abstract Task CheckForUpdate(bool useDelta = true);

    protected override bool OnMouseDown(MouseDownEvent e) => State.Value == Visibility.Visible;
    protected override bool OnHover(HoverEvent e) => State.Value == Visibility.Visible;
}
