// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using VRCOSC.App.Modules;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.Pages.Modules;
using VRCOSC.App.Pages.Modules.Parameters;
using VRCOSC.App.Pages.Modules.Settings;
using VRCOSC.App.SDK.Modules.Attributes;
using VRCOSC.App.SDK.Modules.Attributes.Parameters;
using VRCOSC.App.SDK.Modules.Attributes.Settings;
using VRCOSC.App.SDK.Modules.Attributes.Types;
using VRCOSC.App.SDK.OVR;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Settings;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.Modules;

public abstract class Module : INotifyPropertyChanged
{
    public string PackageId { get; set; } = null!;

    internal string SerialisedName => $"{PackageId}.{GetType().Name.ToLowerInvariant()}";

    public Observable<bool> Enabled { get; } = new();
    internal Observable<ModuleState> State { get; } = new(ModuleState.Stopped);

    protected OVRClient OVRClient => AppManager.GetInstance().OVRClient;

    public string Title => GetType().GetCustomAttribute<ModuleTitleAttribute>()?.Title ?? "PLACEHOLDER";
    public string ShortDescription => GetType().GetCustomAttribute<ModuleDescriptionAttribute>()?.ShortDescription ?? string.Empty;
    public ModuleType Type => GetType().GetCustomAttribute<ModuleTypeAttribute>()?.Type ?? ModuleType.Generic;
    public Brush Colour => Type.ToColour();

    // Cached pre-computed lookups
    private readonly Dictionary<string, Enum> parameterNameEnum = new();
    private readonly Dictionary<string, Regex> parameterNameRegex = new();

    internal readonly Dictionary<Enum, ModuleParameter> Parameters = new();
    public List<ModuleParameter> UIParameters => Parameters.Select(pair => pair.Value).ToList();

    internal readonly Dictionary<string, ModuleSetting> Settings = new();
    public List<ModuleSetting> UISettings => Settings.Select(pair => pair.Value).ToList();

    internal readonly Dictionary<string, List<string>> Groups = new();
    internal readonly Dictionary<ModulePersistentAttribute, PropertyInfo> PersistentProperties = new();

    private readonly List<Repeater> updateTasks = new();
    internal readonly List<MethodInfo> ChatBoxUpdateMethods = new();

    private SerialisationManager moduleSerialisationManager = null!;
    private SerialisationManager persistenceSerialisationManager = null!;

    protected virtual bool ShouldUsePersistence => true;

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
                    moduleSettings.Add(Settings[moduleSettingLookup]);
                    settingsInGroup.Add(moduleSettingLookup);
                });
                groupsFormatted.Add(pair.Key, moduleSettings);
            });

            var miscModuleSettings = new List<ModuleSetting>();
            Settings.Where(pair => !settingsInGroup.Contains(pair.Key)).ForEach(pair => miscModuleSettings.Add(pair.Value));
            if (miscModuleSettings.Any()) groupsFormatted.Add("Miscellaneous", miscModuleSettings);

            return groupsFormatted;
        }
    }

    protected Module()
    {
        State.Subscribe(newState => Log(newState.ToString()));
    }

    private static Regex parameterToRegex(string parameterName)
    {
        var pattern = "^"; // start of string
        pattern += parameterName.Replace("/", @"\/").Replace("*", @"(\S*)");
        pattern += "$"; // end of string

        return new Regex(pattern);
    }

    public void InjectDependencies(SerialisationManager moduleSerialisationManager, SerialisationManager persistenceSerialisationManager)
    {
        this.moduleSerialisationManager = moduleSerialisationManager;
        this.persistenceSerialisationManager = persistenceSerialisationManager;
    }

    public void Load()
    {
        OnPreLoad();

        Settings.Values.ForEach(moduleSetting => moduleSetting.Load());
        Parameters.Values.ForEach(moduleParameter => moduleParameter.Load());

        moduleSerialisationManager.Deserialise();
        cachePersistentProperties();

        Enabled.Subscribe(_ => moduleSerialisationManager.Serialise());
        Settings.Values.ForEach(moduleSetting => moduleSetting.RequestSerialisation = () => moduleSerialisationManager.Serialise());
        Parameters.Values.ForEach(moduleParameter => moduleParameter.RequestSerialisation = () => moduleSerialisationManager.Serialise());

        OnPostLoad();
    }

    #region Persistence

    internal bool TryGetPersistentProperty(string key, [NotNullWhen(true)] out PropertyInfo? property)
    {
        property = PersistentProperties.SingleOrDefault(property => property.Key.SerialisedName == key).Value;
        return property is not null;
    }

    private void cachePersistentProperties()
    {
        try
        {
            GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ForEach(info =>
            {
                var isDefined = info.IsDefined(typeof(ModulePersistentAttribute));
                if (!isDefined) return;

                if (!info.CanRead || !info.CanWrite) throw new InvalidOperationException($"Property '{info.Name}' must be declared with get/set to have persistence");

                PersistentProperties.Add(info.GetCustomAttribute<ModulePersistentAttribute>()!, info);
            });
        }
        catch (Exception e)
        {
            //PushException(e);
        }
    }

    private void loadPersistentProperties()
    {
        if (!PersistentProperties.Any() || !ShouldUsePersistence) return;

        persistenceSerialisationManager.Deserialise();
    }

    private void savePersistentProperties()
    {
        if (!PersistentProperties.Any() || !ShouldUsePersistence) return;

        persistenceSerialisationManager.Serialise();
    }

    #endregion

    public async Task Start()
    {
        State.Value = ModuleState.Starting;

        parameterNameEnum.Clear();
        Parameters.ForEach(pair => parameterNameEnum.Add(pair.Value.Name.Value, pair.Key));

        parameterNameRegex.Clear();
        Parameters.ForEach(pair => parameterNameRegex.Add(pair.Value.Name.Value, parameterToRegex(pair.Value.Name.Value)));

        loadPersistentProperties();

        var startResult = await OnModuleStart();

        if (!startResult)
        {
            await Stop();
            return;
        }

        State.Value = ModuleState.Started;

        initialiseUpdateAttributes(GetType());
    }

    public async Task Stop()
    {
        State.Value = ModuleState.Stopping;

        foreach (var updateTask in updateTasks) await updateTask.StopAsync();
        updateTasks.Clear();
        await OnModuleStop();

        savePersistentProperties();

        State.Value = ModuleState.Stopped;
    }

    private void updateMethod(MethodBase method)
    {
        try
        {
            method.Invoke(this, null);
        }
        catch (Exception e)
        {
            //PushException(new Exception($"{Title} ({className}) experienced an exception calling method {method.Name}", e));
        }
    }

    private void initialiseUpdateAttributes(Type? type)
    {
        if (type is null) return;

        initialiseUpdateAttributes(type.BaseType);

        type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .ForEach(method =>
            {
                var updateAttribute = method.GetCustomAttribute<ModuleUpdateAttribute>();
                if (updateAttribute is null) return;

                switch (updateAttribute.Mode)
                {
                    case ModuleUpdateMode.Custom:
                        var updateTask = new Repeater(() => updateMethod(method));
                        updateTask.Start(TimeSpan.FromMilliseconds(updateAttribute.DeltaMilliseconds));
                        updateTasks.Add(updateTask);
                        if (updateAttribute.UpdateImmediately) updateMethod(method);
                        break;

                    case ModuleUpdateMode.ChatBox:
                        ChatBoxUpdateMethods.Add(method);
                        break;
                }
            });
    }

    #region Update

    // TODO: Limit this and the list to ChatBoxModule

    internal void ChatBoxUpdate()
    {
        ChatBoxUpdateMethods.ForEach(UpdateMethod);
    }

    protected void UpdateMethod(MethodBase method)
    {
        try
        {
            method.Invoke(this, null);
        }
        catch (Exception e)
        {
            Logger.Error(e, nameof(Module));
            //PushException(new Exception($"{className} experienced an exception calling method {method.Name}", e));
        }
    }

    #endregion

    #region SDK

    protected virtual void OnPreLoad()
    {
    }

    protected virtual void OnPostLoad()
    {
    }

    protected virtual Task<bool> OnModuleStart() => Task.FromResult(true);

    protected virtual Task OnModuleStop() => Task.CompletedTask;

    /// <summary>
    /// Logs to the terminal when the module is running
    /// </summary>
    /// <param name="message">The message to log to the terminal</param>
    protected void Log(string message)
    {
        Logger.Log($"[{Title}]: {message}", "terminal");
    }

    /// <summary>
    /// Logs to a module debug file when enabled in the settings
    /// </summary>
    /// <param name="message">The message to log to the file</param>
    protected void LogDebug(string message)
    {
        if (!SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.ModuleLogDebug)) return;

        Logger.Log($"[{Title}]: {message}", "module-debug");
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
        Parameters.Add(lookup, new ModuleParameter(new ModuleParameterMetadata(title, description, mode, typeof(T), legacy), defaultName));
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

    /// <summary>
    /// Allows you to create custom module settings to be listed in the module
    /// </summary>
    protected void CreateCustom(Enum lookup, ModuleSetting moduleSetting)
    {
        validateSettingsLookup(lookup);
        Settings.Add(lookup.ToLookup(), moduleSetting);
    }

    protected void CreateToggle(Enum lookup, string title, string description, bool defaultValue)
    {
        validateSettingsLookup(lookup);
        Settings.Add(lookup.ToLookup(), new BoolModuleSetting(new ModuleSettingMetadata(title, description, typeof(ToggleSettingPage)), defaultValue));
    }

    protected void CreateTextBox(Enum lookup, string title, string description, string defaultValue)
    {
        validateSettingsLookup(lookup);
        Settings.Add(lookup.ToLookup(), new StringModuleSetting(new ModuleSettingMetadata(title, description, typeof(TextBoxSettingPage)), defaultValue));
    }

    protected void CreateTextBox(Enum lookup, string title, string description, int defaultValue)
    {
        validateSettingsLookup(lookup);
        Settings.Add(lookup.ToLookup(), new IntModuleSetting(new ModuleSettingMetadata(title, description, typeof(TextBoxSettingPage)), defaultValue));
    }

    protected void CreateSlider(Enum lookup, string title, string description, int defaultValue, int minValue, int maxValue, int tickFrequency = 1)
    {
        validateSettingsLookup(lookup);
        Settings.Add(lookup.ToLookup(), new SliderModuleSetting(new ModuleSettingMetadata(title, description, typeof(SliderSettingPage)), defaultValue, minValue, maxValue, tickFrequency));
    }

    protected void CreateSlider(Enum lookup, string title, string description, float defaultValue, float minValue, float maxValue, float tickFrequency = 0.1f)
    {
        validateSettingsLookup(lookup);
        Settings.Add(lookup.ToLookup(), new SliderModuleSetting(new ModuleSettingMetadata(title, description, typeof(SliderSettingPage)), defaultValue, minValue, maxValue, tickFrequency));
    }

    protected void CreateDropdown<T>(Enum lookup, string title, string description, T defaultValue) where T : Enum
    {
        validateSettingsLookup(lookup);
        Settings.Add(lookup.ToLookup(), new EnumModuleSetting(new ModuleSettingMetadata(title, description, typeof(DropdownSettingPage)), Convert.ToInt32(defaultValue), typeof(T)));
    }

    protected void CreateTextBoxList(Enum lookup, string title, string description, IEnumerable<string> defaultValues, bool rowNumberVisible = false)
    {
        validateSettingsLookup(lookup);
        Settings.Add(lookup.ToLookup(), new StringListModuleSetting(new ModuleSettingMetadata(title, description, typeof(ListTextBoxSettingPage)), defaultValues, rowNumberVisible));
    }

    protected void CreateKeyValuePairList(Enum lookup, string title, string description, List<MutableKeyValuePair> defaultValues, string keyTitle, string valueTitle, bool rowNumberVisible = false)
    {
        validateSettingsLookup(lookup);
        Settings.Add(lookup.ToLookup(), new MutableKeyValuePairListModuleSetting(new MutableKeyValuePairSettingMetadata(title, description, typeof(MutableKeyValuePairSettingPage), keyTitle, valueTitle), defaultValues, rowNumberVisible));
    }

    private void validateSettingsLookup(Enum lookup)
    {
        if (!Settings.ContainsKey(lookup.ToLookup())) return;

        //PushException(new InvalidOperationException("Cannot add multiple of the same key for settings"));
    }

    /// <summary>
    /// Maps a value <paramref name="source"/> from a source range to a destination range
    /// </summary>
    protected static float Map(float source, float sMin, float sMax, float dMin, float dMax) => dMin + (dMax - dMin) * ((source - sMin) / (sMax - sMin));

    /// <summary>
    /// Allows you to send any parameter name and value.
    /// If you want the user to be able to customise the parameter, register a parameter and use <see cref="SendParameter(Enum,object)"/>
    /// </summary>
    /// <param name="name">The name of the parameter</param>
    /// <param name="value">The value to set the parameter to</param>
    protected void SendParameter(string name, object value)
    {
        AppManager.GetInstance().VRChatOscClient.SendValue($"{VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}{name}", value);
    }

    /// <summary>
    /// Allows you to send a customisable parameter using its lookup and a value
    /// </summary>
    /// <param name="lookup">The lookup of the parameter</param>
    /// <param name="value">The value to set the parameter to</param>
    protected void SendParameter(Enum lookup, object value)
    {
        if (!Parameters.TryGetValue(lookup, out var moduleParameter))
        {
            //PushException(new InvalidOperationException($"Lookup `{lookup}` has not been registered. Please register it using `RegisterParameter<T>(Enum,object)`"));
            return;
        }

        AppManager.GetInstance().VRChatOscClient.SendValue($"{VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}{moduleParameter.Name.Value}", value);
    }

    /// <summary>
    /// Retrieves the container of the setting using the provided lookup. This allows for creating more complex UI callback behaviour.
    /// This is best used inside of <see cref="OnPostLoad"/>
    /// </summary>
    /// <param name="lookup">The lookup of the setting</param>
    /// <returns>The container if successful, otherwise pushes an exception and returns default</returns>
    protected ModuleSetting? GetSetting(Enum lookup) => GetSetting<ModuleSetting>(lookup);

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
        if (Settings.TryGetValue(lookup, out var setting)) return (T)setting;

        return default;
    }

    internal ModuleParameter? GetParameter(string lookup)
    {
        return Parameters.SingleOrDefault(pair => pair.Key.ToLookup() == lookup).Value;
    }

    /// <summary>
    /// Retrieves a <see cref="ModuleSetting"/>'s value as a shorthand for <see cref="ModuleAttribute.GetValue{TValueType}"/>
    /// </summary>
    /// <param name="lookup">The lookup of the setting</param>
    /// <typeparam name="T">The value type of the setting</typeparam>
    /// <returns>The value if successful, otherwise pushes an exception and returns default</returns>
    protected T? GetSettingValue<T>(Enum lookup)
    {
        if (!Settings.ContainsKey(lookup.ToLookup())) return default;

        return Settings[lookup.ToLookup()].GetValue<T>(out var value) ? value : default;
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

        var parameterName = Parameters.Values.FirstOrDefault(moduleParameter => parameterNameRegex[moduleParameter.Name.Value].IsMatch(receivedParameter.Name))?.Name.Value;
        if (parameterName is null) return;

        if (!parameterNameEnum.TryGetValue(parameterName, out var lookup)) return;

        var parameterData = Parameters[lookup];

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
    private ModuleParametersWindow? moduleParametersWindow;

    public ICommand UISettingsButton => new RelayCommand(_ => OnSettingsButtonClick());

    public bool UISettingsButtonEnabled => Settings.Any();

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

    public ICommand UIParametersButton => new RelayCommand(_ => OnParametersButtonClicked());

    public bool UIParametersButtonEnabled => Parameters.Any();

    private void OnParametersButtonClicked()
    {
        if (moduleParametersWindow is null)
        {
            moduleParametersWindow = new ModuleParametersWindow(this);

            moduleParametersWindow.Closed += (_, _) =>
            {
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow is null) return;

                mainWindow.WindowState = WindowState.Normal;
                mainWindow.Focus();

                moduleParametersWindow = null;
            };

            moduleParametersWindow.Show();
        }
        else
        {
            moduleParametersWindow.Focus();
        }
    }

    public ICommand UIResetParameters => new RelayCommand(_ => OnResetParametersButtonClicked());

    private void OnResetParametersButtonClicked()
    {
        Parameters.Values.ForEach(parameter => parameter.SetDefault());
    }

    private double parameterScrollViewerHeight = double.NaN;

    public double ParameterScrollViewerHeight
    {
        get => parameterScrollViewerHeight;
        set
        {
            parameterScrollViewerHeight = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}
