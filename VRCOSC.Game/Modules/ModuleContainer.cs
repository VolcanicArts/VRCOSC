// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using CoreOSC;
using osu.Framework.Graphics.Containers;

namespace VRCOSC.Game.Modules.Stack;

public class ModuleContainer : Container
{
    public readonly Module Module;

    public ModuleContainer(Module module)
    {
        Module = module;
    }

    public void Start()
    {
        if (!Module.DataManager.Enabled) return;

        Module.Start();

        if (double.IsPositiveInfinity(Module.DeltaUpdate)) return;

        Scheduler.Add(Module.Update);
        Scheduler.AddDelayed(Module.Update, Module.DeltaUpdate, true);
    }

    public void Stop()
    {
        if (!Module.DataManager.Enabled) return;

        Scheduler.CancelDelayedTasks();
        Module.Stop();
    }

    public void OnOSCMessage(OscMessage message)
    {
        if (!Module.Running || !Module.DataManager.Enabled || !Module.IsRequestingInput) return;

        Module.OnOSCMessage(message);
    }
}
