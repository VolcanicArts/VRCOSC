// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using VRCOSC.Game.Graphics.Containers.Screens.ModuleEditing;
using VRCOSC.Game.Graphics.Containers.Screens.ModuleRun;
using VRCOSC.Game.Graphics.Containers.Screens.ModuleSelect;
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
    private ModuleManager moduleManager = new();

    [BackgroundDependencyLoader]
    private void load()
    {
        InternalChildren = new Drawable[]
        {
            moduleManager,
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
                moduleEditContainer.SourceModule.Value = null;
            });
        });
    }

    public void ShowTerminal()
    {
        Scheduler.Add(() =>
        {
            ChangeChildDepth(runningPopover, -1);
            runningPopover.MoveToY(0, 1000, Easing.OutQuint).Finally(_ => moduleManager.Start());
        });
    }

    public void HideTerminal()
    {
        Task.Run(async () =>
        {
            await moduleManager.Stop();
            Scheduler.Add(() =>
            {
                runningPopover.MoveToY(1, 1000, Easing.InQuint).Finally(_ =>
                {
                    ChangeChildDepth(runningPopover, 0);
                    runningPopover.Reset();
                });
            });
        });
    }
}
