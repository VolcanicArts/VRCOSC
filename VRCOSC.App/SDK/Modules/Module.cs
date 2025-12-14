// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VRCOSC.App.ChatBox;
using VRCOSC.App.ChatBox.Clips;
using VRCOSC.App.ChatBox.Clips.Variables;
using VRCOSC.App.ChatBox.Clips.Variables.Instances;
using VRCOSC.App.Modules;
using VRCOSC.App.Nodes;
using VRCOSC.App.OpenVR;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.SDK.Handlers;
using VRCOSC.App.SDK.Modules.Attributes.Settings;
using VRCOSC.App.SDK.Modules.Attributes.Types;
using VRCOSC.App.SDK.Nodes;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.SDK.Parameters.Queryable;
using VRCOSC.App.SDK.VRChat;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Settings;
using VRCOSC.App.SteamVR;
using VRCOSC.App.UI.Core;
using VRCOSC.App.UI.Views.Modules.Settings;
using VRCOSC.App.Utils;

// ReSharper disable UnusedMember.Global
// ReSharper disable ArrangeMethodOrOperatorBody
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedParameter.Global
// ReSharper disable VirtualMemberNeverOverridden.Global

namespace VRCOSC.App.SDK.Modules;

public abstract class Module
{
    /// <summary>
    /// The ID of this <see cref="Module"/>
    /// </summary>
    internal string ID => GetType().Name.ToLowerInvariant();

    /// <summary>
    /// The ID of the package this <see cref="Module"/> belongs to
    /// </summary>
    internal string PackageID { get; set; } = null!;

    /// <summary>
    /// The package ID this <see cref="Module"/> is from + the <see cref="Module"/>'s ID
    /// </summary>
    internal string FullID => $"{PackageID}.{ID}";

    /// <summary>
    /// Whether this <see cref="Module"/> is currently enabled on the module listing page
    /// </summary>
    public Observable<bool> Enabled { get; } = new();

    /// <summary>
    /// The current state of this <see cref="Module"/>
    /// </summary>
    internal Observable<ModuleState> State { get; } = new(ModuleState.Stopped);

    private readonly Dictionary<Enum, ModuleParameter> readParameters = new();
    private readonly Dictionary<Enum, Regex> parameterNameRegex = new();

    internal readonly Dictionary<Enum, ModuleParameter> Parameters = new();
    internal readonly Dictionary<string, ModuleSetting> Settings = new();

    internal readonly List<SettingsGroup> Groups = new();
    internal readonly Dictionary<ModulePersistentAttribute, PropertyInfo> PersistentProperties = new();

    private readonly List<Repeater> updateTasks = new();
    private readonly List<MethodInfo> chatBoxUpdateMethods = new();

    private readonly object registeredParameterWaitListLock = new();
    private readonly object anyParameterWaitListLock = new();
    private readonly List<RegisteredWaitingParameter> registeredParameterWaitList = [];
    private readonly List<AnyWaitingParameter> anyParameterWaitList = [];

    private SerialisationManager moduleSerialisationManager = null!;
    private SerialisationManager persistenceSerialisationManager = null!;

    internal Type? SettingsWindowType { get; private set; }
    internal Type? RuntimeViewType { get; private set; }

    private bool isLoaded;

    public string Title => GetType().GetCustomAttribute<ModuleTitleAttribute>()?.Title ?? "PLACEHOLDER TITLE";
    public string TitleWithPackage => PackageID == "local" ? $"(Local) {Title}" : Title;
    public string ShortDescription => GetType().GetCustomAttribute<ModuleDescriptionAttribute>()?.ShortDescription ?? "PLACEHOLDER DESCRIPTION";
    public ModuleType Type => GetType().GetCustomAttribute<ModuleTypeAttribute>()?.Type ?? ModuleType.Generic;
    public Brush Colour => Type.ToColour();
    public Uri InfoUrl => GetType().GetCustomAttribute<ModuleInfoAttribute>()!.Url;

    public bool IsRemote => PackageID != "local";
    public bool HasSettings => Settings.Count != 0;
    public bool HasParameters => Parameters.Count != 0;
    public bool HasInfo => GetType().GetCustomAttribute<ModuleInfoAttribute>() is not null;
    public bool HasPrefabs => GetType().GetCustomAttributes<ModulePrefabAttribute>().Any();
    public IEnumerable<ModulePrefabAttribute> Prefabs => GetType().GetCustomAttributes<ModulePrefabAttribute>();

    protected Module()
    {
        State.Subscribe(newState => Log(newState.ToString()));
    }

    #region Management

    internal void InjectDependencies(SerialisationManager moduleSerialisationManager, SerialisationManager persistenceSerialisationManager)
    {
        this.moduleSerialisationManager = moduleSerialisationManager;
        this.persistenceSerialisationManager = persistenceSerialisationManager;
    }

    internal void Load(string filePathOverride = "")
    {
        isLoaded = false;

        Settings.Clear();
        Parameters.Clear();
        Groups.Clear();

        setSettingsWindow();

        OnPreLoad();

        moduleSerialisationManager.Deserialise(string.IsNullOrEmpty(filePathOverride), filePathOverride);

        cachePersistentProperties();

        Enabled.Subscribe(_ => moduleSerialisationManager.Serialise());

        foreach (var (_, moduleSetting) in Settings)
        {
            moduleSetting.OnSettingChange += Serialise;
        }

        isLoaded = true;

        OnPostLoad();
    }

    internal async Task ImportConfig(string filePathOverride)
    {
        await ModuleManager.GetInstance().ReloadAllModules(new Dictionary<string, string> { { FullID, filePathOverride } });
    }

    internal void Serialise()
    {
        moduleSerialisationManager.Serialise();
    }

    private void setSettingsWindow()
    {
        var settingsWindowAttribute = GetType().GetCustomAttribute<ModuleSettingsWindowAttribute>();
        if (settingsWindowAttribute is null) return;

        var windowType = settingsWindowAttribute.WindowType;

        if (!windowType.IsAssignableTo(typeof(Window))) throw new Exception("Cannot set settings window that isn't of type Window");
        if (!windowType.IsAssignableTo(typeof(IManagedWindow))) throw new Exception("Cannot set settings window that doesn't extend IManagedWindow");
        if (!windowType.HasConstructorThatAccepts(GetType())) throw new Exception($"Cannot set settings window that doesn't have a constructor that accepts type {GetType().Name}");

        SettingsWindowType = windowType;
    }

    #endregion

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
            PersistentProperties.Clear();

            GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy).ForEach(info =>
            {
                var isDefined = info.IsDefined(typeof(ModulePersistentAttribute));
                if (!isDefined) return;

                if (!info.CanRead || !info.CanWrite) throw new InvalidOperationException($"Property '{info.Name}' must be declared with get/set to have persistence");

                PersistentProperties.Add(info.GetCustomAttribute<ModulePersistentAttribute>()!, info);
            });
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"{FullID} encountered an error while trying to cache the persistent properties");
        }
    }

    private void loadPersistentProperties()
    {
        if (PersistentProperties.Count == 0) return;

        persistenceSerialisationManager.Deserialise();
    }

    private void savePersistentProperties()
    {
        if (PersistentProperties.Count == 0) return;

        persistenceSerialisationManager.Serialise();
    }

    #endregion

    #region Runtime

    internal async Task Start()
    {
        try
        {
            State.Value = ModuleState.Starting;

            readParameters.Clear();
            parameterNameRegex.Clear();

            var validReadParameters = Parameters.Where(parameter => !string.IsNullOrWhiteSpace(parameter.Value.Name.Value) && parameter.Value.Mode.HasFlag(ParameterMode.Read) && parameter.Value.Enabled.Value).ToList();
            readParameters.AddRange(validReadParameters);
            parameterNameRegex.AddRange(validReadParameters.Select(pair => new KeyValuePair<Enum, Regex>(pair.Key, TemplatedVRChatParameter.TemplateAsRegex(pair.Value.Name.Value))));

            loadPersistentProperties();

            var startResult = await OnModuleStart();

            if (!startResult)
            {
                await Stop();
                return;
            }

            initialiseUpdateAttributes(GetType());

            if (GetType().IsAssignableTo(typeof(IVRCClientEventHandler)))
            {
                VRChatLogReader.Register((IVRCClientEventHandler)this);
            }

            State.Value = ModuleState.Started;
        }
        catch (Exception e)
        {
            State.Value = ModuleState.Stopped;
            ExceptionHandler.Handle(e, $"{FullID} experienced a problem while starting");
        }
    }

    internal async Task Stop()
    {
        State.Value = ModuleState.Stopping;

        if (GetType().IsAssignableTo(typeof(IVRCClientEventHandler)))
        {
            VRChatLogReader.Deregister((IVRCClientEventHandler)this);
        }

        foreach (var updateTask in updateTasks) await updateTask.StopAsync();
        updateTasks.Clear();
        await OnModuleStop();

        savePersistentProperties();

        State.Value = ModuleState.Stopped;
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
                        var updateTask = new Repeater($"{nameof(Module)}-{nameof(invokeMethod)}", () => invokeMethod(method));
                        updateTask.Start(TimeSpan.FromMilliseconds(updateAttribute.DeltaMilliseconds), updateAttribute.UpdateImmediately);
                        updateTasks.Add(updateTask);
                        break;

                    case ModuleUpdateMode.ChatBox:
                        chatBoxUpdateMethods.Add(method);
                        break;
                }
            });
    }

    private Task invokeMethod(MethodBase method)
    {
        try
        {
            method.Invoke(this, null);
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"{FullID} experienced an exception calling method {method.Name}");
        }

        return Task.CompletedTask;
    }

    #endregion

    #region SDK

    /// <summary>
    /// Retrieves the player instance that gives you information about the local player, their built-in avatar parameters, and input controls
    /// </summary>
    public Player GetPlayer() => AppManager.GetInstance().VRChatClient.Player;

    public Instance GetInstance() => AppManager.GetInstance().VRChatClient.Instance;

    /// <summary>
    /// Allows you to access the current state of the current OpenVR runtime
    /// </summary>
    public OpenVRManager GetOpenVRManager() => AppManager.GetInstance().OpenVRManager;

    /// <summary>
    /// Allows you to access the current state of SteamVR
    /// </summary>
    public SteamVRManager GetSteamVRManager() => AppManager.GetInstance().SteamVRManager;

    #region Callbacks

    protected virtual void OnPreLoad()
    {
    }

    protected virtual void OnPostLoad()
    {
    }

    protected virtual Task<bool> OnModuleStart() => Task.FromResult(true);

    protected virtual Task OnModuleStop() => Task.CompletedTask;

    protected virtual void OnAnyParameterReceived(VRChatParameter parameter)
    {
    }

    protected virtual void OnRegisteredParameterReceived(RegisteredParameter parameter)
    {
    }

    protected virtual void OnAvatarChange(AvatarConfig? avatarConfig)
    {
    }

    protected virtual void OnPlayerUpdate()
    {
    }

    #endregion

    /// <summary>
    /// Logs to the terminal when the module is running
    /// </summary>
    /// <param name="message">The message to log to the terminal</param>
    public void Log(string message)
    {
        Logger.Log($"[{Title}]: {message}", LoggingTarget.Terminal);
        LogDebug(message);
    }

    /// <summary>
    /// Logs to a module debug file when enabled in the settings
    /// </summary>
    /// <param name="message">The message to log to the file</param>
    public void LogDebug(string message)
    {
        if (!SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.EnableAppDebug)) return;

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
    /// <param name="mode">Whether the parameter can read from or write to VRChat</param>
    /// <param name="legacy">Whether the parameter is legacy and should no longer be used in favour of the other parameters</param>
    protected void RegisterParameter<T>(Enum lookup, string defaultName, ParameterMode mode, string title, string description, bool legacy = false) where T : struct
    {
        if (isLoaded)
            throw new InvalidOperationException($"{FullID} attempted to register a parameter after the module has been loaded");

        if (Parameters.ContainsKey(lookup))
            throw new InvalidOperationException($"{FullID} attempted to add an already existing lookup ({lookup.ToLookup()}) to its parameters");

        if (typeof(T) != typeof(bool) && typeof(T) != typeof(int) && typeof(T) != typeof(float))
            throw new InvalidOperationException($"{FullID} attempted to register a parameter with an invalid type");

        Parameters.Add(lookup, new ModuleParameter(title, description, defaultName, mode, ParameterTypeFactory.CreateFrom<T>(), legacy));
    }

    /// <summary>
    /// Specifies a list of settings to group together in the UI
    /// </summary>
    /// <param name="title">The title of the group</param>
    /// <param name="lookups">The settings lookups to put in this group</param>
    [Obsolete("Use CreateGroup(string, string, Enum[]) instead", false)]
    protected void CreateGroup(string title, params Enum[] lookups) => CreateGroup(title, string.Empty, lookups);

    /// <summary>
    /// Specifies a list of settings to group together in the UI
    /// </summary>
    /// <param name="title">The title of the group</param>
    /// <param name="description">The description of this group</param>
    /// <param name="lookups">The settings lookups to put in this group</param>
    protected void CreateGroup(string title, string description, params Enum[] lookups)
    {
        if (isLoaded)
            throw new InvalidOperationException($"{FullID} attempted to create a group after the module has been loaded");

        if (Groups.Any(group => group.Title == title))
            throw new InvalidOperationException($"{FullID} attempted to create a group '{title}' which already exists");

        if (lookups.Any(newLookup => Groups.Any(group => group.Settings.Contains(newLookup.ToLookup()))))
            throw new InvalidOperationException($"{FullID} attempted to add a setting to a group when it's already in a group");

        Groups.Add(new SettingsGroup(title, description, lookups.Select(lookup => lookup.ToLookup()).ToList()));
    }

    /// <summary>
    /// Allows you to create custom module settings to be listed in the module
    /// </summary>
    protected void CreateCustomSetting(Enum lookup, ModuleSetting moduleSetting)
    {
        addSetting(lookup, moduleSetting);
    }

    protected void CreateToggle(Enum lookup, string title, string description, bool defaultValue)
    {
        addSetting(lookup, new BoolModuleSetting(title, description, typeof(ToggleSettingView), defaultValue));
    }

    protected void CreateTextBox(Enum lookup, string title, string description, string defaultValue)
    {
        addSetting(lookup, new StringModuleSetting(title, description, typeof(TextBoxSettingView), defaultValue));
    }

    protected void CreateTextBox(Enum lookup, string title, string description, int defaultValue)
    {
        addSetting(lookup, new IntModuleSetting(title, description, typeof(TextBoxSettingView), defaultValue));
    }

    protected void CreateTextBox(Enum lookup, string title, string description, float defaultValue)
    {
        addSetting(lookup, new FloatModuleSetting(title, description, typeof(TextBoxSettingView), defaultValue));
    }

    protected void CreatePasswordTextBox(Enum lookup, string title, string description, string defaultValue)
    {
        addSetting(lookup, new StringModuleSetting(title, description, typeof(PasswordTextBoxSettingView), defaultValue));
    }

    protected void CreateSlider(Enum lookup, string title, string description, int defaultValue, int minValue, int maxValue, int tickFrequency = 1)
    {
        addSetting(lookup, new SliderModuleSetting(title, description, typeof(SliderSettingView), defaultValue, minValue, maxValue, tickFrequency));
    }

    protected void CreateSlider(Enum lookup, string title, string description, float defaultValue, float minValue, float maxValue, float tickFrequency = 0.1f)
    {
        addSetting(lookup, new SliderModuleSetting(title, description, typeof(SliderSettingView), defaultValue, minValue, maxValue, tickFrequency));
    }

    protected void CreateDropdown<T>(Enum lookup, string title, string description, T defaultValue) where T : Enum
    {
        addSetting(lookup, new EnumModuleSetting(title, description, typeof(EnumDropdownSettingView), Convert.ToInt32(defaultValue), typeof(T)));
    }

    protected void CreateDropdown(Enum lookup, string title, string description, IEnumerable<object> items, object defaultItem, string titlePath, string valuePath)
    {
        var titleProperty = defaultItem.GetType().GetProperty(titlePath);
        var valueProperty = defaultItem.GetType().GetProperty(valuePath);

        if (titleProperty is null || valueProperty is null)
        {
            throw new InvalidOperationException("You must have a property for both the titlePath and valuePath");
        }

        var titleValue = titleProperty.GetValue(defaultItem);
        var valueValue = valueProperty.GetValue(defaultItem);

        if (titleValue is null || valueValue is null)
        {
            throw new InvalidOperationException("You must have a property for both the titlePath and valuePath");
        }

        if (titleValue.ToString() is null || valueValue.ToString() is null)
        {
            throw new InvalidOperationException("Your titlePath and valuePath properties must be convertible to a string");
        }

        addSetting(lookup, new DropdownListModuleSetting(title, description, typeof(ListItemDropdownSettingView), items, valueValue.ToString()!, titlePath, valuePath));
    }

    protected void CreateDateTime(Enum lookup, string title, string description, DateTimeOffset defaultValue)
    {
        addSetting(lookup, new DateTimeModuleSetting(title, description, typeof(DateTimeSettingView), defaultValue));
    }

    protected void CreateTextBoxList(Enum lookup, string title, string description, IEnumerable<string> defaultValues)
    {
        addSetting(lookup, new StringListModuleSetting(title, description, typeof(ListTextBoxSettingView), defaultValues));
    }

    protected void CreateTextBoxList(Enum lookup, string title, string description, IEnumerable<int> defaultValues)
    {
        addSetting(lookup, new IntListModuleSetting(title, description, typeof(ListTextBoxSettingView), defaultValues));
    }

    protected void CreateTextBoxList(Enum lookup, string title, string description, IEnumerable<float> defaultValues)
    {
        addSetting(lookup, new FloatListModuleSetting(title, description, typeof(ListTextBoxSettingView), defaultValues));
    }

    protected void CreateKeyValuePairList(Enum lookup, string title, string description, IEnumerable<MutableKeyValuePair> defaultValues, string keyTitle, string valueTitle)
    {
        addSetting(lookup, new MutableKeyValuePairListModuleSetting(title, description, typeof(MutableKeyValuePairListSettingView), defaultValues, keyTitle, valueTitle));
    }

    protected void CreateQueryableParameterList(Enum lookup, string title, string description)
    {
        // TODO: Allow for deriving queryable parameters
        addSetting(lookup, new QueryableParameterListModuleSetting<QueryableParameter>(title, description));
    }

    private void addSetting(Enum lookup, ModuleSetting moduleSetting)
    {
        if (isLoaded)
            throw new InvalidOperationException($"{FullID} attempted to create a module setting after the module has been loaded");

        if (!moduleSetting.ViewType.HasConstructorThatAccepts(typeof(Module), typeof(ModuleSetting)))
            throw new InvalidOperationException($"{FullID} attempted to create a module setting who's view doesn't have a constructor of (Module, ModuleSetting)");

        if (Settings.ContainsKey(lookup.ToLookup()))
            throw new InvalidOperationException($"{FullID} attempted to add an already existing lookup ({lookup.ToLookup()}) to its settings");

        moduleSetting.ParentModule = this;
        Settings.Add(lookup.ToLookup(), moduleSetting);
    }

    #region ChatBox

    #region Runtime

    protected void ChangeState(Enum lookup)
    {
        ChangeState(lookup.ToLookup());
    }

    protected void ChangeState(string lookup)
    {
        if (GetState(lookup) is null) throw new InvalidOperationException($"State with lookup {lookup} does not exist");

        ChatBoxManager.GetInstance().ChangeStateTo(FullID, lookup);
    }

    protected void TriggerEvent(Enum lookup)
    {
        TriggerEvent(lookup.ToLookup());
    }

    protected void TriggerEvent(string lookup)
    {
        if (GetEvent(lookup) is null) throw new InvalidOperationException($"Event with lookup {lookup} does not exist");

        ChatBoxManager.GetInstance().TriggerEvent(FullID, lookup);
    }

    protected void SetVariableValue<T>(Enum lookup, T value) where T : notnull
    {
        SetVariableValue(lookup.ToLookup(), value);
    }

    protected void SetVariableValue<T>(string lookup, T value) where T : notnull
    {
        var variable = GetVariable(lookup);
        if (variable is null) throw new InvalidOperationException($"Variable with lookup {lookup} does not exist");

        variable.SetValue(value);
    }

    #endregion

    #region States

    protected ClipStateReference? CreateState(Enum lookup, string displayName, string defaultFormat = "", IEnumerable<ClipVariableReference>? defaultVariables = null, bool defaultShowTyping = false)
    {
        return CreateState(lookup.ToLookup(), displayName, defaultFormat, defaultVariables, defaultShowTyping);
    }

    protected ClipStateReference? CreateState(string lookup, string displayName, string defaultFormat = "", IEnumerable<ClipVariableReference>? defaultVariables = null, bool defaultShowTyping = false)
    {
        if (GetState(lookup) is not null)
        {
            ExceptionHandler.Handle($"[{FullID}]: You cannot add the same lookup ({lookup}) for a state more than once");
            return null;
        }

        var clipStateReference = new ClipStateReference
        {
            ModuleID = FullID,
            StateID = lookup,
            DefaultFormat = defaultFormat,
            DefaultShowTyping = defaultShowTyping,
            DefaultVariables = defaultVariables?.ToList() ?? new List<ClipVariableReference>(),
            DisplayName = { Value = displayName }
        };

        ChatBoxManager.GetInstance().CreateState(clipStateReference);
        return clipStateReference;
    }

    protected void DeleteState(Enum lookup)
    {
        DeleteState(lookup.ToLookup());
    }

    protected void DeleteState(string lookup)
    {
        ChatBoxManager.GetInstance().DeleteState(FullID, lookup);
    }

    protected ClipStateReference? GetState(Enum lookup)
    {
        return GetState(lookup.ToLookup());
    }

    protected ClipStateReference? GetState(string lookup)
    {
        return ChatBoxManager.GetInstance().GetState(FullID, lookup);
    }

    #endregion

    #region Events

    protected ClipEventReference? CreateEvent(Enum lookup, string displayName, string defaultFormat = "", IEnumerable<ClipVariableReference>? defaultVariables = null, bool defaultShowTyping = false, float defaultLength = 5, ClipEventBehaviour defaultBehaviour = ClipEventBehaviour.Override)
    {
        return CreateEvent(lookup.ToLookup(), displayName, defaultFormat, defaultVariables, defaultShowTyping, defaultLength, defaultBehaviour);
    }

    protected ClipEventReference? CreateEvent(string lookup, string displayName, string defaultFormat = "", IEnumerable<ClipVariableReference>? defaultVariables = null, bool defaultShowTyping = false, float defaultLength = 5, ClipEventBehaviour defaultBehaviour = ClipEventBehaviour.Override)
    {
        if (GetEvent(lookup) is not null)
        {
            ExceptionHandler.Handle($"[{FullID}]: You cannot add the same lookup ({lookup}) for an event more than once");
            return null;
        }

        var clipEventReference = new ClipEventReference
        {
            ModuleID = FullID,
            EventID = lookup,
            DefaultFormat = defaultFormat,
            DefaultShowTyping = defaultShowTyping,
            DefaultVariables = defaultVariables?.ToList() ?? new List<ClipVariableReference>(),
            DefaultLength = defaultLength,
            DefaultBehaviour = defaultBehaviour,
            DisplayName = { Value = displayName }
        };

        ChatBoxManager.GetInstance().CreateEvent(clipEventReference);
        return clipEventReference;
    }

    protected void DeleteEvent(Enum lookup)
    {
        DeleteEvent(lookup.ToLookup());
    }

    protected void DeleteEvent(string lookup)
    {
        ChatBoxManager.GetInstance().DeleteEvent(FullID, lookup);
    }

    protected ClipEventReference? GetEvent(Enum lookup)
    {
        return GetEvent(lookup.ToLookup());
    }

    protected ClipEventReference? GetEvent(string lookup)
    {
        return ChatBoxManager.GetInstance().GetEvent(FullID, lookup);
    }

    #endregion

    #region Variables

    /// <summary>
    /// Creates a variable using the specified <paramref name="lookup"/>
    /// </summary>
    /// <param name="lookup">The lookup to retrieve this variable</param>
    /// <param name="displayName">The display name to show the user</param>
    /// <typeparam name="T">The type of this variable's value</typeparam>
    /// <remarks><paramref name="lookup"/> is turned into a string internally, and is only an enum to allow for easier referencing in your code</remarks>
    protected ClipVariableReference? CreateVariable<T>(Enum lookup, string displayName)
    {
        return CreateVariable<T>(lookup.ToLookup(), displayName);
    }

    /// <summary>
    /// Creates a variable using the specified <paramref name="lookup"/>
    /// </summary>
    /// <param name="lookup">The lookup to retrieve this variable</param>
    /// <param name="displayName">The display name to show the user</param>
    /// <typeparam name="T">The type of this variable's value</typeparam>
    protected ClipVariableReference? CreateVariable<T>(string lookup, string displayName)
    {
        Type? clipVariableType = null;

        if (typeof(T) == typeof(bool))
            clipVariableType = typeof(BoolClipVariable);
        else if (typeof(T) == typeof(int))
            clipVariableType = typeof(IntClipVariable);
        else if (typeof(T) == typeof(float))
            clipVariableType = typeof(FloatClipVariable);
        else if (typeof(T) == typeof(string))
            clipVariableType = typeof(StringClipVariable);
        else if (typeof(T) == typeof(DateTimeOffset))
            clipVariableType = typeof(DateTimeClipVariable);
        else if (typeof(T) == typeof(TimeSpan))
            clipVariableType = typeof(TimeSpanClipVariable);

        if (clipVariableType is null)
            throw new InvalidOperationException("No clip variable exists for that type. Request it is added to the SDK or make a custom clip variable");

        return CreateVariable<T>(lookup, displayName, clipVariableType);
    }

    /// <summary>
    /// Creates a variable using the specified <paramref name="lookup"/> and a custom <see cref="ClipVariable"/>
    /// </summary>
    /// <param name="lookup">The lookup to retrieve this variable</param>
    /// <param name="displayName">The display name to show the user</param>
    /// <param name="clipVariableType">The type of <see cref="ClipVariable"/> to create when instancing this variable</param>
    /// <typeparam name="T">The type of this variable's value</typeparam>
    /// <remarks><paramref name="lookup"/> is turned into a string internally, and is only an enum to allow for easier referencing in your code</remarks>
    protected ClipVariableReference? CreateVariable<T>(Enum lookup, string displayName, Type clipVariableType)
    {
        return CreateVariable<T>(lookup.ToLookup(), displayName, clipVariableType);
    }

    /// <summary>
    /// Creates a variable using the specified <paramref name="lookup"/> and a custom <see cref="ClipVariable"/>
    /// </summary>
    /// <param name="lookup">The lookup to retrieve this variable</param>
    /// <param name="displayName">The display name to show the user</param>
    /// <param name="clipVariableType">The type of <see cref="ClipVariable"/> to create when instancing this variable</param>
    /// <typeparam name="T">The type of this variable's value</typeparam>
    protected ClipVariableReference? CreateVariable<T>(string lookup, string displayName, Type clipVariableType)
    {
        if (GetVariable(lookup) is not null)
        {
            ExceptionHandler.Handle($"[{FullID}]: You cannot add the same lookup ({lookup}) for a variable more than once");
            return null;
        }

        var clipVariableReference = new ClipVariableReference
        {
            ModuleID = FullID,
            VariableID = lookup,
            ClipVariableType = clipVariableType,
            ValueType = typeof(T),
            DisplayName = { Value = displayName }
        };

        ChatBoxManager.GetInstance().CreateVariable(clipVariableReference);
        return clipVariableReference;
    }

    /// <summary>
    /// Allows for deleting a variable at runtime.
    /// This is most useful for when you have variables whose existence is reliant on module settings
    /// and you need to delete the variable when the setting disappears
    /// </summary>
    /// <param name="lookup">The lookup to retrieve this variable</param>
    /// <remarks><paramref name="lookup"/> is turned into a string internally, and is only an enum to allow for easier referencing in your code</remarks>
    protected void DeleteVariable(Enum lookup)
    {
        DeleteVariable(lookup.ToLookup());
    }

    /// <summary>
    /// Allows for deleting a variable at runtime.
    /// This is most useful for when you have variables whose existence is reliant on module settings
    /// and you need to delete the variable when the setting disappears
    /// </summary>
    /// <param name="lookup">The lookup to retrieve this variable</param>
    protected void DeleteVariable(string lookup)
    {
        ChatBoxManager.GetInstance().DeleteVariable(FullID, lookup);
    }

    /// <summary>
    /// Retrieves the <see cref="ClipVariableReference"/> using the <paramref name="lookup"/> provided
    /// </summary>
    /// <param name="lookup">The lookup to retrieve this variable</param>
    /// <remarks><paramref name="lookup"/> is turned into a string internally, and is only an enum to allow for easier referencing in your code</remarks>
    protected ClipVariableReference? GetVariable(Enum lookup)
    {
        return GetVariable(lookup.ToLookup());
    }

    /// <summary>
    /// Retrieves the <see cref="ClipVariableReference"/> using the <paramref name="lookup"/> provided
    /// </summary>
    /// <param name="lookup">The lookup to retrieve this variable</param>
    protected ClipVariableReference? GetVariable(string lookup)
    {
        return ChatBoxManager.GetInstance().GetVariable(FullID, lookup);
    }

    #endregion

    #endregion

    /// <summary>
    /// Allows you to set the view that shows up in the `runtime` tab of the run screen
    /// </summary>
    /// <param name="viewType">This should be the <see cref="Type"/> of your view</param>
    protected void SetRuntimeView(Type viewType)
    {
        if (isLoaded)
            throw new InvalidOperationException($"{FullID} attempted to set the runtime view after the module has been loaded");

        if (!viewType.IsAssignableTo(typeof(UserControl)))
            throw new InvalidOperationException($"{FullID} attempted to set a runtime view which does not extend UserControl");

        if (!viewType.HasConstructorThatAccepts(typeof(Module)))
            throw new InvalidOperationException($"{FullID} attempted to set a runtime view which does not have a constructor of ({GetType().Name})");

        RuntimeViewType = viewType;
    }

    /// <summary>
    /// Changes the user's avatar into the ID that's provided
    /// </summary>
    protected void ChangeAvatar(string avatarId)
    {
        AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOSCConstants.ADDRESS_AVATAR_CHANGE}", avatarId);
    }

    /// <summary>
    /// Allows you to send any parameter name and value.
    /// If you want the user to be able to customise the parameter, register a parameter and use <see cref="SendParameter(Enum,object)"/>
    /// </summary>
    /// <param name="name">The name of the parameter</param>
    /// <param name="value">The value to set the parameter to</param>
    protected void SendParameter(string name, object value)
    {
        AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOSCConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}{name}", value);
    }

    /// <summary>
    /// Allows you to send any parameter name and value, but wait for VRChat to acknowledge it
    /// </summary>
    /// <param name="name">The name of the parameter</param>
    /// <param name="value">The value to set the parameter to</param>
    /// <param name="blockEvents">Whether to block <see cref="OnAnyParameterReceived"/> from running until we acknowledge a response. This is helpful to prevent unwanted loopbacks</param>
    /// <param name="timeout">The timeout at which waiting fails. Defaults to 0.5 seconds</param>
    /// <returns>True if the parameter was acknowledged, false if the parameter doesn't exist or VRChat is closed</returns>
    protected async Task<bool> SendParameterAndWait(string name, object value, bool blockEvents = false, TimeSpan timeout = default)
    {
        if (timeout == TimeSpan.Zero) timeout = TimeSpan.FromSeconds(0.5f);

        var taskCompletionSource = new TaskCompletionSource();
        var waitingParameter = new AnyWaitingParameter(name, blockEvents, taskCompletionSource);

        lock (anyParameterWaitListLock)
        {
            anyParameterWaitList.Add(waitingParameter);
        }

        SendParameter(name, value);

        var result = false;

        try
        {
            await taskCompletionSource.Task.WaitAsync(timeout);
            result = true;
        }
        catch (TimeoutException)
        {
        }

        return result;
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
            ExceptionHandler.Handle(new InvalidOperationException($"Parameter `{lookup}` has not been registered. Please register it by calling {nameof(RegisterParameter)} in {nameof(OnPreLoad)}"));
            return;
        }

        if (!moduleParameter.Enabled.Value || string.IsNullOrWhiteSpace(moduleParameter.Name.Value)) return;

        SendParameter(moduleParameter.Name.Value, value);
    }

    /// <summary>
    /// Allows you to send a customisable parameter using its lookup and a value, but wait for VRChat to acknowledge it
    /// </summary>
    /// <param name="lookup">The lookup of the parameter</param>
    /// <param name="value">The value to set the parameter to</param>
    /// <param name="blockEvents">Whether to block <see cref="OnRegisteredParameterReceived"/> from running until we acknowledge a response. This is helpful to prevent unwanted loopbacks</param>
    /// <param name="timeout">The timeout at which waiting fails. Defaults to 0.5 seconds</param>
    /// <returns>True if the parameter was acknowledged, false if the parameter doesn't exist or VRChat is closed</returns>
    protected async Task<bool> SendParameterAndWait(Enum lookup, object value, bool blockEvents = false, TimeSpan timeout = default)
    {
        if (timeout == TimeSpan.Zero) timeout = TimeSpan.FromSeconds(0.5f);

        var taskCompletionSource = new TaskCompletionSource();
        var waitingParameter = new RegisteredWaitingParameter(lookup, blockEvents, taskCompletionSource);

        lock (registeredParameterWaitListLock)
        {
            registeredParameterWaitList.Add(waitingParameter);
        }

        SendParameter(lookup, value);

        var result = false;

        try
        {
            await taskCompletionSource.Task.WaitAsync(timeout);
            result = true;
        }
        catch (TimeoutException)
        {
        }

        return result;
    }

    public async Task TriggerModuleNode(Type nodeType, object[] data)
    {
        if (!nodeType.IsAssignableTo(typeof(ModuleNode<>).MakeGenericType(GetType()))) throw new InvalidOperationException($"{nodeType.Name} is not a {nameof(ModuleNode<>)}");
        if (!nodeType.IsAssignableTo(typeof(IModuleNodeEventHandler))) throw new InvalidOperationException($"{nodeType.Name} is not a {nameof(IModuleNodeEventHandler)}");

        await NodeManager.GetInstance().TriggerModuleNode(nodeType, data);
    }

    /// <summary>
    /// Retrieves the container of the setting using the provided lookup. This allows for creating more complex UI callback behaviour.
    /// This is best used inside of <see cref="OnPostLoad"/>
    /// </summary>
    /// <param name="lookup">The lookup of the setting</param>
    /// <returns>The container if successful, otherwise pushes an exception and returns default</returns>
    public ModuleSetting GetSetting(Enum lookup) => GetSetting<ModuleSetting>(lookup);

    /// <summary>
    /// Retrieves the container of the setting using the provided lookup and type param for custom <see cref="ModuleSetting"/>s. This allows for creating more complex UI callback behaviour.
    /// This is best used inside of <see cref="OnPostLoad"/>
    /// </summary>
    /// <typeparam name="T">The custom <see cref="ModuleSetting"/> type</typeparam>
    /// <param name="lookup">The lookup of the setting</param>
    /// <returns>The container if successful, otherwise pushes an exception and returns default</returns>
    public T GetSetting<T>(Enum lookup) where T : ModuleSetting => GetSetting<T>(lookup.ToLookup());

    internal T GetSetting<T>(string lookup) where T : ModuleSetting
    {
        if (Settings.TryGetValue(lookup, out var setting)) return (T)setting;

        throw new InvalidOperationException($"Setting with lookup '{lookup}' doesn't exist");
    }

    internal ModuleParameter GetParameter(string lookup)
    {
        var moduleParameter = Parameters.SingleOrDefault(pair => pair.Key.ToLookup() == lookup);
        if (Parameters.Any(pair => pair.Key.ToLookup() == lookup)) return moduleParameter.Value;

        throw new InvalidOperationException($"Parameter with lookup '{lookup}' doesn't exist");
    }

    /// <summary>
    /// Retrieves a <see cref="ModuleSetting"/>'s value as a shorthand for <see cref="ModuleSetting.GetValue{TValueType}"/>
    /// </summary>
    /// <param name="lookup">The lookup of the setting</param>
    /// <typeparam name="T">The value type of the setting</typeparam>
    /// <returns>The value if successful, otherwise pushes an exception and returns default</returns>
    public T GetSettingValue<T>(Enum lookup)
    {
        try
        {
            return GetSetting(lookup).GetValue<T>();
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"'{FullID}' experienced a problem when getting value of setting '{lookup.ToLookup()}'");
            return default!;
        }
    }

    public void SetSettingValue<T>(Enum lookup, T value) where T : notnull
    {
        try
        {
            var setting = GetSetting(lookup);
            if (setting is not ValueModuleSetting<T> valueModuleSetting) throw new Exception("Cannot set a setting value for a non-value module setting");

            valueModuleSetting.Attribute.Value = value;
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"'{FullID}' experienced a problem when setting value of setting '{lookup.ToLookup()}'");
        }
    }

    protected Task<string?> FindCurrentAvatar() => AppManager.GetInstance().VRChatOscClient.FindCurrentAvatar(CancellationToken.None);

    /// <summary>
    /// Retrieves a parameter's value using OSCQuery
    /// </summary>
    /// <param name="lookup">The lookup of the registered parameter</param>
    protected Task<VRChatParameter?> FindParameter(Enum lookup) => FindParameter(Parameters[lookup].Name.Value);

    /// <summary>
    /// Retrieves a parameter's value using OSCQuery
    /// </summary>
    /// <param name="parameterName">The name of the parameter</param>
    protected Task<VRChatParameter?> FindParameter(string parameterName) => AppManager.GetInstance().VRChatOscClient.FindParameter(parameterName, CancellationToken.None);

    internal void OnParameterReceived(VRChatParameter parameter)
    {
        List<AnyWaitingParameter> anyWaitingParameters;

        lock (anyParameterWaitListLock)
        {
            anyWaitingParameters = anyParameterWaitList.Where(waitingParameter => waitingParameter.Name == parameter.Name).ToList();
        }

        if (!anyWaitingParameters.Any(waitingParameter => waitingParameter.BlockEvents))
        {
            try
            {
                OnAnyParameterReceived(parameter);
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e, $"Module {FullID} experienced an exception calling {nameof(OnAnyParameterReceived)}");
            }
        }

        lock (anyParameterWaitListLock)
        {
            foreach (var waitingParameter in anyWaitingParameters)
            {
                anyParameterWaitList.Remove(waitingParameter);
                waitingParameter.CompletionSource.SetResult();
            }
        }

        RegisteredParameter? registeredParameter = null;

        foreach (var (lookup, _) in readParameters.Where(pair => pair.Value.ExpectedType == parameter.Type))
        {
            var templateRegex = parameterNameRegex[lookup];
            var checkingRegisteredParameter = new RegisteredParameter(lookup, new TemplatedVRChatParameter(templateRegex, parameter));
            if (!checkingRegisteredParameter.IsMatch()) continue;

            registeredParameter = checkingRegisteredParameter;
            break;
        }

        if (registeredParameter is null) return;

        List<RegisteredWaitingParameter> registeredWaitingParameters;

        lock (registeredParameterWaitListLock)
        {
            registeredWaitingParameters = registeredParameterWaitList.Where(waitingParameter => waitingParameter.Lookup.Equals(registeredParameter.Lookup)).ToList();
        }

        if (!registeredWaitingParameters.Any(waitingParameter => waitingParameter.BlockEvents))
        {
            try
            {
                OnRegisteredParameterReceived(registeredParameter);
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e, $"Module {FullID} experienced an exception calling {nameof(OnRegisteredParameterReceived)}");
            }
        }

        lock (registeredParameterWaitListLock)
        {
            foreach (var waitingParameter in registeredWaitingParameters)
            {
                registeredParameterWaitList.Remove(waitingParameter);
                waitingParameter.CompletionSource.SetResult();
            }
        }
    }

    internal void InvokeAvatarChange(AvatarConfig? avatarConfig)
    {
        try
        {
            OnAvatarChange(avatarConfig);
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"{FullID} has experienced an exception calling {nameof(OnAvatarChange)}");
        }
    }

    internal void InvokePlayerUpdate()
    {
        try
        {
            OnPlayerUpdate();
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"{FullID} has experienced an exception calling {nameof(OnPlayerUpdate)}");
        }
    }

    internal void InvokeChatBoxUpdate()
    {
        foreach (var chatBoxUpdateMethod in chatBoxUpdateMethods)
        {
            invokeMethod(chatBoxUpdateMethod);
        }
    }

    #endregion

    private record RegisteredWaitingParameter(Enum Lookup, bool BlockEvents, TaskCompletionSource CompletionSource);

    private record AnyWaitingParameter(string Name, bool BlockEvents, TaskCompletionSource CompletionSource);

    public record SettingsGroup(string Title, string Description, List<string> Settings);
}