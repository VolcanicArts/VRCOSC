// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using VRCOSC.Game.Graphics.Containers.Screens.ModuleCardScreen;
using VRCOSC.Game.Graphics.Containers.Screens.ModuleEditScreen;
using VRCOSC.Game.Graphics.Containers.Screens.TerminalScreen;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens;

public class ScreenManager : Container
{
    private TerminalContainer terminalContainer;
    private ModuleEditContainer moduleEditContainer;

    [Resolved]
    private ModuleManager ModuleManager { get; set; }

    [BackgroundDependencyLoader]
    private void load()
    {
        InternalChildren = new Drawable[]
        {
            new ModuleCardListingContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.Both,
                RelativePositionAxes = Axes.Both
            },
            terminalContainer = new TerminalContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.Both,
                RelativePositionAxes = Axes.Both,
                Position = new Vector2(0, 1)
            },
            moduleEditContainer = new ModuleEditContainer
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                RelativeSizeAxes = Axes.Both,
                RelativePositionAxes = Axes.Both,
                Position = new Vector2(0, 1)
            }
        };
    }

    public void EditModule(Modules.Module module)
    {
        Scheduler.Add(() =>
        {
            moduleEditContainer.SourceModule.Value = module;
            ChangeChildDepth(moduleEditContainer, -1);
            moduleEditContainer.MoveToY(0, 1000, Easing.OutQuint);
        });
    }

    public void FinishEditingModule()
    {
        Scheduler.Add(() =>
        {
            ChangeChildDepth(moduleEditContainer, 0);
            moduleEditContainer.MoveToY(1, 1000, Easing.InQuint);
        });
    }

    public void ShowTerminal()
    {
        Scheduler.Add(() =>
        {
            ChangeChildDepth(terminalContainer, -1);
            terminalContainer.MoveToY(0, 1000, Easing.OutQuint).Finally((_) =>
            {
                ModuleManager.Start(Scheduler);
            });
        });
    }

    public void HideTerminal()
    {
        Scheduler.Add(() =>
        {
            ModuleManager.Stop(Scheduler);
            ChangeChildDepth(terminalContainer, 0);
            terminalContainer.MoveToY(1, 1000, Easing.InQuint).Finally((_) =>
            {
                terminalContainer.ClearTerminal();
            });
        });
    }
}
