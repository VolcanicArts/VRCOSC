// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace VRCOSC.App.Modules;

public class Module
{
    public string PackageId { get; set; } = null!;

    internal string SerialisedName => $"{PackageId}.{GetType().Name.ToLowerInvariant()}";

    public bool Enabled { get; set; }
    public ModuleState State { get; set; }

    public string Title => GetType().GetCustomAttribute<ModuleTitleAttribute>()?.Title ?? "PLACEHOLDER";
    public string ShortDescription => GetType().GetCustomAttribute<ModuleDescriptionAttribute>()?.ShortDescription ?? string.Empty;
    public ModuleType Type => GetType().GetCustomAttribute<ModuleTypeAttribute>()?.Type ?? ModuleType.Generic;

    public void Load()
    {
        OnPreLoad();

        OnPostLoad();
    }

    public async Task Start()
    {
        State = ModuleState.Starting;

        var startResult = await OnModuleStart();

        if (!startResult)
        {
            await Stop();
            return;
        }

        State = ModuleState.Started;
    }

    public async Task Stop()
    {
        State = ModuleState.Stopping;

        await OnModuleStop();

        State = ModuleState.Stopped;
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

    #endregion

    #region UI

    public ICommand UISettingsButton => new RelayCommand(_ => OnSettingsButtonClick());

    private void OnSettingsButtonClick()
    {
        MessageBox.Show("WOOOOOOOO");
    }

    #endregion
}
