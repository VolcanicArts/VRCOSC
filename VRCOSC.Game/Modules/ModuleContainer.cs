// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using CoreOSC;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;

namespace VRCOSC.Game.Modules;

public class ModuleContainer : Container
{
    public readonly Module Module;

    public ModuleContainer(Module module)
    {
        Module = module;
    }

    public void Start()
    {
        if (!Module.Enabled.Value) return;

        Logger.Log($"[{Module.Title}]: Starting", "terminal");
        Module.Start();
        Logger.Log($"[{Module.Title}]: Started", "terminal");

        if (double.IsPositiveInfinity(Module.DeltaUpdate)) return;

        Scheduler.Add(Module.Update);
        Scheduler.AddDelayed(Module.Update, Module.DeltaUpdate, true);
    }

    public void Stop()
    {
        if (!Module.Enabled.Value) return;

        Scheduler.CancelDelayedTasks();

        Logger.Log($"[{Module.Title}]: Stopping", "terminal");
        Module.Stop();
        Logger.Log($"[{Module.Title}]: Stopped", "terminal");
    }

    public void OnOSCMessage(OscMessage message)
    {
        if (!Module.Enabled.Value || !Module.IsRequestingInput) return;

        Module.OnOSCMessage(message);
    }
}
