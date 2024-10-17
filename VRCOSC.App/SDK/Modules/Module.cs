// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using VRCOSC.App.ChatBox;
using VRCOSC.App.ChatBox.Clips;
using VRCOSC.App.ChatBox.Clips.Variables;
using VRCOSC.App.ChatBox.Clips.Variables.Instances;
using VRCOSC.App.Modules;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.SDK.Modules.Attributes;
using VRCOSC.App.SDK.Modules.Attributes.Settings;
using VRCOSC.App.SDK.Modules.Attributes.Types;
using VRCOSC.App.SDK.OVR;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.SDK.VRChat;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Settings;
using VRCOSC.App.UI.Views.Modules.Settings;
using VRCOSC.App.Utils;

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

    // Cached pre-computed lookups
    private readonly Dictionary<string, Enum> parameterNameEnum = new();
    private readonly Dictionary<string, Regex> parameterNameRegex = new();

    internal readonly Dictionary<Enum, ModuleParameter> Parameters = new();
    internal readonly Dictionary<string, ModuleSetting> Settings = new();

    internal readonly Dictionary<string, List<string>> Groups = new();
    internal readonly Dictionary<ModulePersistentAttribute, PropertyInfo> PersistentProperties = new();

    private readonly List<Repeater> updateTasks = new();
    private readonly List<MethodInfo> chatBoxUpdateMethods = new();

    private SerialisationManager moduleSerialisationManager = null!;
    private SerialisationManager persistenceSerialisationManager = null!;

    internal Type? RuntimeViewType { get; private set; }

    private readonly object loadLock = new();
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
        lock (loadLock)
        {
            isLoaded = false;

            Settings.Clear();
            Parameters.Clear();
            Groups.Clear();

            OnPreLoad();

            Settings.Values.ForEach(moduleSetting => moduleSetting.PreDeserialise());
            moduleSerialisationManager.Deserialise(string.IsNullOrEmpty(filePathOverride), filePathOverride);
            Settings.Values.ForEach(moduleSetting => moduleSetting.PostDeserialise());

            cachePersistentProperties();

            Enabled.Subscribe(_ => moduleSerialisationManager.Serialise());

            isLoaded = true;

            OnPostLoad();
        }
    }

    internal void ImportConfig(string filePathOverride)
    {
        ModuleManager.GetInstance().ReloadAllModules(new Dictionary<string, string> { { FullID, filePathOverride } });
    }

    internal void Serialise()
    {
        moduleSerialisationManager.Serialise();
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

    private static Regex parameterToRegex(string parameterName)
    {
        var pattern = "^"; // start of string
        pattern += @"(?:VF\d+_)?"; // VRCFury prefix
        pattern += $"({parameterName.Replace("/", @"\/").Replace("*", @"(?:\S*)")})";
        pattern += "$"; // end of string

        return new Regex(pattern);
    }

    internal async Task Start()
    {
        try
        {
            State.Value = ModuleState.Starting;

            parameterNameEnum.Clear();
            parameterNameRegex.Clear();

            var validParameters = Parameters.Where(parameter => !string.IsNullOrWhiteSpace(parameter.Value.Name.Value)).ToList();

            validParameters.ForEach(pair =>
            {
                parameterNameEnum.Add(pair.Value.Name.Value, pair.Key);
                parameterNameRegex.Add(pair.Value.Name.Value, parameterToRegex(pair.Value.Name.Value));
            });

            loadPersistentProperties();

            var startResult = await OnModuleStart();

            if (!startResult)
            {
                await Stop();
                return;
            }

            initialiseUpdateAttributes(GetType());

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
                        var updateTask = new Repeater(() => invokeMethod(method));
                        updateTask.Start(TimeSpan.FromMilliseconds(updateAttribute.DeltaMilliseconds), updateAttribute.UpdateImmediately);
                        updateTasks.Add(updateTask);
                        break;

                    case ModuleUpdateMode.ChatBox:
                        chatBoxUpdateMethods.Add(method);
                        break;
                }
            });
    }

    private void invokeMethod(MethodBase method)
    {
        try
        {
            method.Invoke(this, null);
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"{FullID} experienced an exception calling method {method.Name}");
        }
    }

    #endregion

    #region SDK

    /// <summary>
    /// Retrieves the player instance that gives you information about the local player, their built-in avatar parameters, and input controls
    /// </summary>
    protected Player GetPlayer() => AppManager.GetInstance().VRChatClient.Player;

    /// <summary>
    /// Allows you to access the current state of SteamVR (or any OpenVR runtime)
    /// </summary>
    protected OVRClient GetOVRClient() => AppManager.GetInstance().OVRClient;

    #region Callbacks

    protected virtual void OnPreLoad()
    {
    }

    protected virtual void OnPostLoad()
    {
    }

    protected virtual Task<bool> OnModuleStart() => Task.FromResult(true);

    protected virtual Task OnModuleStop() => Task.CompletedTask;

    protected virtual void OnAnyParameterReceived(ReceivedParameter parameter)
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

        Parameters.Add(lookup, new ModuleParameter(title, description, defaultName, mode, typeof(T), legacy));
    }

    /// <summary>
    /// Specifies a list of settings to group together in the UI
    /// </summary>
    /// <param name="title">The title of the group</param>
    /// <param name="lookups">The settings lookups to put in this group</param>
    protected void CreateGroup(string title, params Enum[] lookups)
    {
        if (isLoaded)
            throw new InvalidOperationException($"{FullID} attempted to create a group after the module has been loaded");

        if (Groups.ContainsKey(title))
            throw new InvalidOperationException($"{FullID} attempted to create a group '{title}' which already exists");

        if (lookups.Any(newLookup => Groups.Any(pair => pair.Value.Contains(newLookup.ToLookup()))))
            throw new InvalidOperationException($"{FullID} attempted to add a setting to a group when it's already in a group");

        Groups.Add(title, lookups.Select(lookup => lookup.ToLookup()).ToList());
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
        addSetting(lookup, new EnumModuleSetting(title, description, typeof(DropdownSettingView), Convert.ToInt32(defaultValue), typeof(T)));
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

    protected void SetVariableValue<T>(Enum lookup, T value)
    {
        SetVariableValue(lookup.ToLookup(), value);
    }

    protected void SetVariableValue<T>(string lookup, T value)
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
            ExceptionHandler.Handle(new InvalidOperationException($"Parameter `{lookup}` has not been registered. Please register it by calling RegisterParameter in OnPreLoad"));
            return;
        }

        if (!moduleParameter.Enabled.Value || string.IsNullOrWhiteSpace(moduleParameter.Name.Value)) return;

        SendParameter(moduleParameter.Name.Value, value);
    }

    /// <summary>
    /// Retrieves the container of the setting using the provided lookup. This allows for creating more complex UI callback behaviour.
    /// This is best used inside of <see cref="OnPostLoad"/>
    /// </summary>
    /// <param name="lookup">The lookup of the setting</param>
    /// <returns>The container if successful, otherwise pushes an exception and returns default</returns>
    protected ModuleSetting GetSetting(Enum lookup) => GetSetting<ModuleSetting>(lookup);

    /// <summary>
    /// Retrieves the container of the setting using the provided lookup and type param for custom <see cref="ModuleSetting"/>s. This allows for creating more complex UI callback behaviour.
    /// This is best used inside of <see cref="OnPostLoad"/>
    /// </summary>
    /// <typeparam name="T">The custom <see cref="ModuleSetting"/> type</typeparam>
    /// <param name="lookup">The lookup of the setting</param>
    /// <returns>The container if successful, otherwise pushes an exception and returns default</returns>
    protected T GetSetting<T>(Enum lookup) where T : ModuleSetting => GetSetting<T>(lookup.ToLookup());

    internal T GetSetting<T>(string lookup) where T : ModuleSetting
    {
        lock (loadLock)
        {
            if (Settings.TryGetValue(lookup, out var setting)) return (T)setting;

            throw new InvalidOperationException($"Setting with lookup '{lookup}' doesn't exist");
        }
    }

    internal ModuleParameter GetParameter(string lookup)
    {
        lock (loadLock)
        {
            var moduleParameter = Parameters.SingleOrDefault(pair => pair.Key.ToLookup() == lookup);
            if (Parameters.Any(pair => pair.Key.ToLookup() == lookup)) return moduleParameter.Value;

            throw new InvalidOperationException($"Parameter with lookup '{lookup}' doesn't exist");
        }
    }

    /// <summary>
    /// Retrieves a <see cref="ModuleSetting"/>'s value as a shorthand for <see cref="ModuleAttribute.GetValue{TValueType}"/>
    /// </summary>
    /// <param name="lookup">The lookup of the setting</param>
    /// <typeparam name="T">The value type of the setting</typeparam>
    /// <returns>The value if successful, otherwise pushes an exception and returns default</returns>
    protected T GetSettingValue<T>(Enum lookup)
    {
        lock (loadLock)
        {
            var moduleSetting = GetSetting(lookup);

            if (moduleSetting.GetValue<T>(out var value)) return value;

            throw new InvalidOperationException($"Could not get the value of setting with lookup '{lookup.ToLookup()}'");
        }
    }

    /// <summary>
    /// Retrieves a parameter's value using OSCQuery
    /// </summary>
    /// <param name="lookup">The lookup of the registered parameter</param>
    protected Task<object?> FindParameterValue(Enum lookup)
    {
        var parameterName = Parameters[lookup].Name.Value;
        return FindParameterValue(parameterName);
    }

    /// <summary>
    /// Retrieves a parameter's value using OSCQuery
    /// </summary>
    /// <param name="parameterName">The name of the parameter</param>
    protected Task<object?> FindParameterValue(string parameterName)
    {
        return AppManager.GetInstance().VRChatOscClient.FindParameterValue(parameterName);
    }

    /// <summary>
    /// Retrieves a parameter's type using OSCQuery
    /// </summary>
    /// <param name="lookup">The lookup of the registered parameter</param>
    protected Task<TypeCode?> FindParameterType(Enum lookup)
    {
        var parameterName = Parameters[lookup].Name.Value;
        return FindParameterType(parameterName);
    }

    /// <summary>
    /// Retrieves a parameter's type using OSCQuery
    /// </summary>
    /// <param name="parameterName">The name of the parameter</param>
    protected Task<TypeCode?> FindParameterType(string parameterName)
    {
        return AppManager.GetInstance().VRChatOscClient.FindParameterType(parameterName);
    }

    internal void OnParameterReceived(VRChatOscMessage message)
    {
        lock (loadLock)
        {
            if (message.IsAvatarChangeEvent)
            {
                var avatarConfig = AvatarConfigLoader.LoadConfigFor((string)message.ParameterValue);
                invokeAvatarChange(avatarConfig);
                return;
            }

            var receivedParameter = new ReceivedParameter(message.ParameterName, message.ParameterValue);

            try
            {
                OnAnyParameterReceived(receivedParameter);
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e, $"Module {FullID} experienced an exception calling {nameof(OnAnyParameterReceived)}");
            }

            string? parameterName = null;

            foreach (var parameter in Parameters.Values)
            {
                var match = parameterNameRegex[parameter.Name.Value].Match(receivedParameter.Name);
                if (!match.Success) continue;

                parameterName = match.Groups[1].Captures[0].Value;
            }

            if (parameterName is null) return;

            if (!parameterNameEnum.TryGetValue(parameterName, out var lookup)) return;

            var parameterData = Parameters[lookup];

            if (!parameterData.Mode.HasFlag(ParameterMode.Read)) return;

            if (!receivedParameter.IsValueType(parameterData.ExpectedType))
            {
                Log($"Cannot accept input parameter. `{lookup}` expects type `{parameterData.ExpectedType.ToReadableName()}` but received type `{receivedParameter.Value.GetType().ToReadableName()}`");
                return;
            }

            var registeredParameter = new RegisteredParameter(receivedParameter, lookup, parameterData);

            try
            {
                OnRegisteredParameterReceived(registeredParameter);
            }
            catch (Exception e)
            {
                ExceptionHandler.Handle(e, $"Module {FullID} experienced an exception calling {nameof(OnRegisteredParameterReceived)}");
            }
        }
    }

    private void invokeAvatarChange(AvatarConfig? avatarConfig)
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
        chatBoxUpdateMethods.ForEach(invokeMethod);
    }

    #endregion
}