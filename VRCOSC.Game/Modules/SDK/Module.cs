// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Reflection;
using System.Threading.Tasks;

namespace VRCOSC.Game.Modules.SDK;

public class Module
{
    internal string Title => GetType().GetCustomAttribute<ModuleTitleAttribute>()?.Title ?? "PLACEHOLDER";
    internal string ShortDescription => GetType().GetCustomAttribute<ModuleDescriptionAttribute>()?.ShortDescription ?? string.Empty;
    internal ModuleType Type => GetType().GetCustomAttribute<ModuleTypeAttribute>()?.Type ?? ModuleType.Generic;

    internal ModuleState State { get; private set; } = ModuleState.Stopped;

    internal Task Start()
    {
        State = ModuleState.Starting;

        var startTask = OnModuleStart();
        startTask.GetAwaiter().OnCompleted(() => State = ModuleState.Started);
        return startTask;
    }

    internal Task Stop()
    {
        State = ModuleState.Stopping;

        var stopTask = OnModuleStop();
        stopTask.GetAwaiter().OnCompleted(() => State = ModuleState.Stopped);
        return stopTask;
    }

    #region Events

    protected virtual Task OnModuleStart() => Task.CompletedTask;
    protected virtual Task OnModuleStop() => Task.CompletedTask;

    #endregion
}
