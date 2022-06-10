// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.Containers.Screens.ModuleCardScreen;
using VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing;
using VRCOSC.Game.Graphics.Containers.Screens.ModuleRun;
using VRCOSC.Game.Modules;

namespace VRCOSC.Game.Graphics.Containers.Screens;

[Cached]
public sealed class ScreenManager : Container
{
    private RunningPopover runningPopover;
    private ModuleEditPopover moduleEditContainer;

    public ScreenManager()
    {
        Anchor = Anchor.Centre;
        Origin = Anchor.Centre;
        RelativeSizeAxes = Axes.Both;
    }

    [Cached]
    private ModuleManager ModuleManager = new();

    [BackgroundDependencyLoader]
    private void load()
    {
        InternalChildren = new Drawable[]
        {
            ModuleManager,
            new ModuleSelection(),
            runningPopover = new RunningPopover(),
            moduleEditContainer = new ModuleEditPopover()
        };
    }

    public void EditModule(Module module)
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
            moduleEditContainer.MoveToY(1, 1000, Easing.InQuint).Finally(_ =>
            {
                ChangeChildDepth(moduleEditContainer, 0);
            });
        });
    }

    public void ShowTerminal()
    {
        Scheduler.Add(() =>
        {
            ChangeChildDepth(runningPopover, -1);
            runningPopover.MoveToY(0, 1000, Easing.OutQuint).Finally(_ => ModuleManager.Start());
        });
    }

    public void HideTerminal()
    {
        Scheduler.Add(() =>
        {
            ModuleManager.Stop();
            runningPopover.MoveToY(1, 1000, Easing.InQuint).Finally(_ =>
            {
                ChangeChildDepth(runningPopover, 0);
                //runningPopover.ClearTerminal();
            });
        });
    }
}
