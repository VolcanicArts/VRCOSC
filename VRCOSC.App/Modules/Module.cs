// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Modules;

public class Module
{
    public string PackageId { get; set; } = null!;

    internal string SerialisedName => $"{PackageId}.{GetType().Name.ToLowerInvariant()}";

    public Observable<bool> Enabled { get; } = new(false);
    public Observable<ModuleState> State { get; } = new(ModuleState.Stopped);

    public string Title => GetType().GetCustomAttribute<ModuleTitleAttribute>()?.Title ?? "PLACEHOLDER";
    public string ShortDescription => GetType().GetCustomAttribute<ModuleDescriptionAttribute>()?.ShortDescription ?? string.Empty;
    public ModuleType Type => GetType().GetCustomAttribute<ModuleTypeAttribute>()?.Type ?? ModuleType.Generic;

    public Module()
    {
        State.Subscribe(newState => Log(newState.ToString()));
        Enabled.Subscribe(isEnabled => Log(isEnabled.ToString()));
    }

    public void Load()
    {
        OnPreLoad();

        OnPostLoad();
    }

    public async Task Start()
    {
        State.Value = ModuleState.Starting;

        var startResult = await OnModuleStart();

        if (!startResult)
        {
            await Stop();
            return;
        }

        State.Value = ModuleState.Started;
    }

    public async Task Stop()
    {
        State.Value = ModuleState.Stopping;

        await OnModuleStop();

        State.Value = ModuleState.Stopped;
    }

    #region SDK

    public virtual void OnPreLoad()
    {
    }

    public virtual void OnPostLoad()
    {
    }

    public virtual Task<bool> OnModuleStart() => Task.FromResult(true);

    public virtual Task OnModuleStop() => Task.CompletedTask;

    /// <summary>
    /// Logs to the terminal when the module is running
    /// </summary>
    /// <param name="message">The message to log to the terminal</param>
    protected void Log(string message)
    {
        Logger.Log($"[{Title}]: {message}");
    }

    #endregion

    #region UI

    public ICommand UISettingsButton => new RelayCommand(_ => OnSettingsButtonClick());

    private void OnSettingsButtonClick()
    {
        MessageBox.Show("WOOOOOOOO");
    }

    #endregion
}
