// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VRCOSC.App.Modules.Attributes;
using VRCOSC.App.Modules.Attributes.Parameters;
using VRCOSC.App.Modules.Attributes.Settings;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.Pages.Modules;
using VRCOSC.App.Pages.Modules.Settings;
using VRCOSC.App.Parameters;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Modules;

public abstract class Module
{
    public string PackageId { get; set; } = null!;

    internal string SerialisedName => $"{PackageId}.{GetType().Name.ToLowerInvariant()}";

    public Observable<bool> Enabled { get; } = new(false);
    public Observable<ModuleState> State { get; } = new(ModuleState.Stopped);

    public string Title => GetType().GetCustomAttribute<ModuleTitleAttribute>()?.Title ?? "PLACEHOLDER";
    public string ShortDescription => GetType().GetCustomAttribute<ModuleDescriptionAttribute>()?.ShortDescription ?? string.Empty;
    public ModuleType Type => GetType().GetCustomAttribute<ModuleTypeAttribute>()?.Type ?? ModuleType.Generic;

    // Cached pre-computed lookups
    private readonly Dictionary<string, Enum> parameterNameEnum = new();
    private readonly Dictionary<string, Regex> parameterNameRegex = new();

    private readonly Dictionary<Enum, ModuleParameter> parameters = new();
    public List<ModuleParameter> Parameters => parameters.Select(pair => pair.Value).ToList();

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
        Application.Current.MainWindow!.Closed += MainWindowOnClosed;

        State.Subscribe(newState => Log(newState.ToString()));
        Enabled.Subscribe(isEnabled => Log(isEnabled.ToString()));
    }

    private static Regex parameterToRegex(string parameterName)
    {
        var pattern = parameterName.Replace("/", @"\/").Replace("*", @"(\S*)");
        pattern += "$";
        return new Regex(pattern);
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

        parameterNameEnum.Clear();
        parameters.ForEach(pair => parameterNameEnum.Add(pair.Value.Name.Value, pair.Key));

        parameterNameRegex.Clear();
        parameters.ForEach(pair => parameterNameRegex.Add(pair.Value.Name.Value, parameterToRegex(pair.Value.Name.Value)));

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
    /// Registers a parameter with a lookup to allow the user to customise the parameter name
    /// </summary>
    /// <typeparam name="T">The value type of this <see cref="ModuleParameter"/></typeparam>
    /// <param name="lookup">The lookup of this parameter, used as a reference when calling <see cref="SendParameter(Enum,object)"/></param>
    /// <param name="defaultName">The default name of the parameter</param>
    /// <param name="title">The title of the parameter</param>
    /// <param name="description">A short description of the parameter</param>
    /// <param name="mode">Whether the parameter can read to or write from VRChat</param>
    /// <param name="legacy">Whether the parameter is legacy and should no longer be used in favour of the other parameters</param>
    protected void RegisterParameter<T>(Enum lookup, string defaultName, ParameterMode mode, string title, string description, bool legacy = false) where T : struct
    {
        parameters.Add(lookup, new ModuleParameter(new ModuleParameterMetadata(title, description, mode, typeof(T), legacy), defaultName));
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

    protected void CreateTextBox(Enum lookup, string title, string description, string defaultValue, bool emptyIsValid = true)
    {
        validateSettingsLookup(lookup);
        settings.Add(lookup.ToLookup(), new StringModuleSetting(new ModuleSettingMetadata(title, description, typeof(StringSettingPage)), emptyIsValid, defaultValue));
    }

    private void validateSettingsLookup(Enum lookup)
    {
        if (!settings.ContainsKey(lookup.ToLookup())) return;

        //PushException(new InvalidOperationException("Cannot add multiple of the same key for settings"));
    }

    /// <summary>
    /// Retrieves the container of the setting using the provided lookup and type param for custom <see cref="ModuleSetting"/>s. This allows for creating more complex UI callback behaviour.
    /// This is best used inside of <see cref="OnPostLoad"/>
    /// </summary>
    /// <typeparam name="T">The custom <see cref="ModuleSetting"/> type</typeparam>
    /// <param name="lookup">The lookup of the setting</param>
    /// <returns>The container if successful, otherwise pushes an exception and returns default</returns>
    protected T? GetSetting<T>(Enum lookup) where T : ModuleSetting => GetSetting<T>(lookup.ToLookup());

    internal T? GetSetting<T>(string lookup) where T : ModuleSetting
    {
        if (settings.TryGetValue(lookup, out var setting)) return (T)setting;

        return default;
    }

    internal ModuleParameter? GetParameter(string lookup)
    {
        return parameters.SingleOrDefault(pair => pair.Key.ToLookup() == lookup).Value;
    }

    /// <summary>
    /// Retrieves a <see cref="ModuleSetting"/>'s value as a shorthand for <see cref="ModuleAttribute.GetValue{TValueType}"/>
    /// </summary>
    /// <param name="lookup">The lookup of the setting</param>
    /// <typeparam name="T">The value type of the setting</typeparam>
    /// <returns>The value if successful, otherwise pushes an exception and returns default</returns>
    protected T? GetSettingValue<T>(Enum lookup)
    {
        if (!settings.ContainsKey(lookup.ToLookup())) return default;

        return settings[lookup.ToLookup()].GetValue<T>(out var value) ? value : default;
    }

    internal virtual void OnParameterReceived(VRChatOscMessage message)
    {
        var receivedParameter = new ReceivedParameter(message.ParameterName, message.ParameterValue);

        try
        {
            OnAnyParameterReceived(receivedParameter);
        }
        catch (Exception e)
        {
            //PushException(e);
        }

        var parameterName = parameters.Values.FirstOrDefault(moduleParameter => parameterNameRegex[moduleParameter.Name.Value].IsMatch(receivedParameter.Name))?.Name.Value;
        if (parameterName is null) return;

        if (!parameterNameEnum.TryGetValue(parameterName, out var lookup)) return;

        var parameterData = parameters[lookup];

        if (!parameterData.Metadata.Mode.HasFlagFast(ParameterMode.Read)) return;

        if (!receivedParameter.IsValueType(parameterData.Metadata.Type))
        {
            Log($"Cannot accept input parameter. `{lookup}` expects type `{parameterData.Metadata.Type.ToReadableName()}` but received type `{receivedParameter.Value.GetType().ToReadableName()}`");
            return;
        }

        var registeredParameter = new RegisteredParameter(receivedParameter, lookup, parameterData);

        try
        {
            InternalOnRegisteredParameterReceived(registeredParameter);
        }
        catch (Exception e)
        {
            //PushException(e);
        }
    }

    protected internal abstract void InternalOnRegisteredParameterReceived(RegisteredParameter registeredParameter);

    protected virtual void OnAnyParameterReceived(ReceivedParameter receivedParameter)
    {
    }

    #endregion

    #region UI

    private ModuleSettingsWindow? moduleSettingsWindow;

    private void MainWindowOnClosed(object? sender, EventArgs e)
    {
        moduleSettingsWindow?.Close();
    }

    public ICommand UISettingsButton => new RelayCommand(_ => OnSettingsButtonClick());

    private void OnSettingsButtonClick()
    {
        if (moduleSettingsWindow is null)
        {
            moduleSettingsWindow = new ModuleSettingsWindow(this);

            moduleSettingsWindow.Closed += (_, _) =>
            {
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow is null) return;

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
