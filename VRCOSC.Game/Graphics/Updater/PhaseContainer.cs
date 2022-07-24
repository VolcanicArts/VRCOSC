// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace VRCOSC.Game.Graphics.Updater;

public abstract class PhaseContainer : VisibilityContainer
{
    protected virtual float ScaleFrom => 1f;

    protected UpdatePhase updatePhase;

    public UpdatePhase UpdatePhase
    {
        get => updatePhase;
        set
        {
            updatePhase = value;
            OnPhaseChange();
        }
    }

    [BackgroundDependencyLoader]
    private void load()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        AutoSizeAxes = Axes.Both;
    }

    protected abstract void OnPhaseChange();

    protected override void PopIn()
    {
        this.FadeInFromZero(200, Easing.OutQuint);
        this.ScaleTo(ScaleFrom).Then().ScaleTo(1, 200, Easing.OutQuint);
    }

    protected override void PopOut()
    {
        this.FadeOutFromOne(200, Easing.OutQuint);
        this.ScaleTo(1f).Then().ScaleTo(ScaleFrom, 200, Easing.OutQuint);
    }
}
