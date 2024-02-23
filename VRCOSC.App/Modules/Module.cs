// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VRCOSC.App.Modules.Attributes.Settings;
using VRCOSC.App.Pages.Modules;
using VRCOSC.App.Pages.Modules.Settings;
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

    private readonly Dictionary<string, ModuleSetting> settings = new();
    public List<ModuleSetting> Settings => settings.Select(pair => pair.Value).ToList();

    internal readonly Dictionary<string, List<string>> Groups = new();

    public Dictionary<string, List<ModuleSetting>> GroupsFormatted
    {
        get
        {
            var settingsInGroup = new List<string>();

            var groupsFormatted = new Dictionary<string, List<ModuleSetting>>();

            Groups.ForEach(pair =>
            {
                var moduleSettings = new List<ModuleSetting>();
                pair.Value.ForEach(moduleSettingLookup =>
                {
                    moduleSettings.Add(settings[moduleSettingLookup]);
                    settingsInGroup.Add(moduleSettingLookup);
                });
                groupsFormatted.Add(pair.Key, moduleSettings);
            });

            var miscModuleSettings = new List<ModuleSetting>();
            settings.Where(pair => !settingsInGroup.Contains(pair.Key)).ForEach(pair => miscModuleSettings.Add(pair.Value));
            groupsFormatted.Add("Miscellaneous", miscModuleSettings);

            return groupsFormatted;
        }
    }

    public Module()
    {
        State.Subscribe(newState => Log(newState.ToString()));
        Enabled.Subscribe(isEnabled => Log(isEnabled.ToString()));
    }

    public void Load()
    {
        OnPreLoad();

        settings.Values.ForEach(moduleSetting => moduleSetting.Load());

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

    /// <summary>
    /// Specifies a list of settings to group together in the UI
    /// </summary>
    /// <param name="title">The title of the group</param>
    /// <param name="lookups">The settings lookups to put in this group</param>
    protected void CreateGroup(string title, params Enum[] lookups)
    {
        Groups.Add(title, lookups.Select(lookup => lookup.ToLookup()).ToList());
    }

    protected void CreateToggle(Enum lookup, string title, string description, bool defaultValue)
    {
        validateSettingsLookup(lookup);
        settings.Add(lookup.ToLookup(), new BoolModuleSetting(new ModuleSettingMetadata(title, description, typeof(BoolSettingPage)), defaultValue));
    }

    private void validateSettingsLookup(Enum lookup)
    {
        if (!settings.ContainsKey(lookup.ToLookup())) return;

        //PushException(new InvalidOperationException("Cannot add multiple of the same key for settings"));
    }

    #endregion

    #region UI

    private ModuleSettingsWindow? moduleSettingsWindow;

    public ICommand UISettingsButton => new RelayCommand(_ => OnSettingsButtonClick());

    private void OnSettingsButtonClick()
    {
        if (moduleSettingsWindow is null)
        {
            moduleSettingsWindow = new ModuleSettingsWindow(this);

            moduleSettingsWindow.Closed += (_, _) =>
            {
                var mainWindow = Application.Current.MainWindow!;
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.Focus();

                moduleSettingsWindow = null;
            };

            moduleSettingsWindow.Show();
        }
        else
        {
            moduleSettingsWindow.Focus();
        }
    }

    #endregion
}
