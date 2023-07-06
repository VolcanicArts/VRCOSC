// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using osu.Framework.Bindables;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Threading;
using VRCOSC.Game.App;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Managers;
using VRCOSC.Game.Modules.Attributes;
using VRCOSC.Game.Modules.Avatar;
using VRCOSC.Game.Modules.Serialisation.V1;
using VRCOSC.Game.OpenVR;
using VRCOSC.Game.OSC.VRChat;
using VRCOSC.Game.Serialisation;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming

namespace VRCOSC.Game.Modules;

public abstract class Module : IComparable<Module>
{
    private GameHost Host = null!;
    private AppManager AppManager = null!;
    private Scheduler Scheduler = null!;
    private TerminalLogger Terminal = null!;
    private ModuleDebugLogger moduleDebugLogger = null!;
    private Storage Storage = null!;

    protected Player Player => AppManager.VRChat.Player;
    protected OVRClient OVRClient => AppManager.OVRClient;
    protected VRChatOscClient OscClient => AppManager.OSCClient;
    protected ChatBoxManager ChatBoxManager => AppManager.ChatBoxManager;
    protected Bindable<ModuleState> State = new(ModuleState.Stopped);
    protected AvatarConfig? AvatarConfig => AppManager.VRChat.AvatarConfig;

    internal readonly BindableBool Enabled = new();
    internal readonly Dictionary<string, ModuleAttribute> Settings = new();
    internal readonly Dictionary<Enum, ModuleParameter> Parameters = new();

    // Cached pre-computed lookups
    internal readonly Dictionary<string, Enum> ParameterNameEnum = new();
    internal readonly Dictionary<string, Regex> ParameterNameRegex = new();

    public virtual string Title => string.Empty;
    public virtual string Description => string.Empty;
    public virtual string Author => string.Empty;
    public virtual string Prefab => string.Empty;
    public virtual ModuleType Type => ModuleType.General;
    public virtual IEnumerable<string> Info => new List<string>();
    protected virtual TimeSpan DeltaUpdate => TimeSpan.MaxValue;
    protected virtual bool ShouldUpdateImmediately => true;

    private bool IsEnabled => Enabled.Value;
    private bool ShouldUpdate => DeltaUpdate != TimeSpan.MaxValue;
    internal string Name => GetType().Name;
    internal string SerialisedName => Name.ToLowerInvariant();

    protected bool IsStarting => State.Value == ModuleState.Starting;
    protected bool HasStarted => State.Value == ModuleState.Started;
    protected bool IsStopping => State.Value == ModuleState.Stopping;
    protected bool HasStopped => State.Value == ModuleState.Stopped;

    private readonly SerialisationManager saveStateSerialisationManager = new();
    private readonly SerialisationManager moduleSerialisationManager = new();
    private NotificationContainer notifications = null!;

    public void InjectDependencies(GameHost host, AppManager appManager, Scheduler scheduler, Storage storage, NotificationContainer notifications)
    {
        Host = host;
        AppManager = appManager;
        Scheduler = scheduler;
        this.notifications = notifications;
        Storage = storage;

        moduleSerialisationManager.RegisterSerialiser(1, new ModuleSerialiser(storage, notifications, this));
    }

    public void Load()
    {
        Terminal = new TerminalLogger(Title);
        moduleDebugLogger = new ModuleDebugLogger(Title);

        CreateAttributes();
        Settings.Values.ForEach(setting => setting.Setup());
        Parameters.Values.ForEach(parameter => parameter.Setup());

        State.ValueChanged += _ => Log(State.Value.ToString());

        Deserialise();
    }

    #region Serialisation

    public void Serialise()
    {
        moduleSerialisationManager.Serialise();
    }

    public void Deserialise()
    {
        moduleSerialisationManager.Deserialise();
    }

    #endregion

    #region Save State

    protected void RegisterSaveStateSerialiser<T>(int version) where T : ISaveStateSerialiser
    {
        var serialiser = (ISaveStateSerialiser)Activator.CreateInstance(typeof(T), Storage, notifications, this)!;
        saveStateSerialisationManager.RegisterSerialiser(version, serialiser);
    }

    protected void SaveState()
    {
        saveStateSerialisationManager.Serialise();
    }

    protected void LoadState()
    {
        saveStateSerialisationManager.Deserialise();
    }

    #endregion

    #region Attributes

    protected virtual void CreateAttributes() { }

    protected void CreateSetting(Enum lookup, ModuleAttribute attribute) => Settings.Add(lookup.ToLookup(), attribute);

    protected void CreateSetting(Enum lookup, string displayName, string description, bool defaultValue, Func<bool>? dependsOn = null)
        => Settings.Add(lookup.ToLookup(), new ModuleBoolAttribute
        {
            Name = displayName,
            Description = description,
            Default = defaultValue,
            DependsOn = dependsOn
        });

    protected void CreateSetting(Enum lookup, string displayName, string description, int defaultValue, Func<bool>? dependsOn = null)
        => Settings.Add(lookup.ToLookup(), new ModuleIntAttribute
        {
            Name = displayName,
            Description = description,
            Default = defaultValue,
            DependsOn = dependsOn
        });

    protected void CreateSetting(Enum lookup, string displayName, string description, float defaultValue, Func<bool>? dependsOn = null)
        => Settings.Add(lookup.ToLookup(), new ModuleFloatAttribute
        {
            Name = displayName,
            Description = description,
            Default = defaultValue,
            DependsOn = dependsOn
        });

    protected void CreateSetting(Enum lookup, string displayName, string description, string defaultValue, Func<bool>? dependsOn = null)
        => Settings.Add(lookup.ToLookup(), new ModuleStringAttribute
        {
            Name = displayName,
            Description = description,
            Default = defaultValue,
            DependsOn = dependsOn
        });

    protected void CreateSetting(Enum lookup, string displayName, string description, string defaultValue, string buttonText, Action buttonCallback, Func<bool>? dependsOn = null)
        => Settings.Add(lookup.ToLookup(), new ModuleStringWithButtonAttribute
        {
            Name = displayName,
            Description = description,
            Default = defaultValue,
            DependsOn = dependsOn,
            ButtonText = buttonText,
            ButtonCallback = buttonCallback
        });

    protected void CreateSetting<T>(Enum lookup, string displayName, string description, T defaultValue, Func<bool>? dependsOn = null) where T : Enum
        => Settings.Add(lookup.ToLookup(), new ModuleEnumAttribute<T>
        {
            Name = displayName,
            Description = description,
            Default = defaultValue,
            DependsOn = dependsOn
        });

    protected void CreateSetting(Enum lookup, string displayName, string description, IEnumerable<string> defaultValue, Func<bool>? dependsOn = null)
        => Settings.Add(lookup.ToLookup(), new ModuleStringListAttribute
        {
            Name = displayName,
            Description = description,
            Default = defaultValue.Select(v => new Bindable<string>(v)).ToList(),
            DependsOn = dependsOn
        });

    protected void CreateSetting(Enum lookup, string displayName, string description, List<MutableKeyValuePair> defaultValue, string keyPlaceholder, string valuePlaceholder, Func<bool>? dependsOn = null)
        => Settings.Add(lookup.ToLookup(), new MutableKeyValuePairListAttribute
        {
            Name = displayName,
            Description = description,
            Default = defaultValue,
            DependsOn = dependsOn,
            KeyPlaceholder = keyPlaceholder,
            ValuePlaceholder = valuePlaceholder
        });

    protected void CreateSetting(Enum lookup, string displayName, string description, int defaultValue, int minValue, int maxValue, Func<bool>? dependsOn = null)
        => Settings.Add(lookup.ToLookup(), new ModuleIntRangeAttribute
        {
            Name = displayName,
            Description = description,
            Default = defaultValue,
            DependsOn = dependsOn,
            Min = minValue,
            Max = maxValue
        });

    protected void CreateSetting(Enum lookup, string displayName, string description, float defaultValue, float minValue, float maxValue, Func<bool>? dependsOn = null)
        => Settings.Add(lookup.ToLookup(), new ModuleFloatRangeAttribute
        {
            Name = displayName,
            Description = description,
            Default = defaultValue,
            DependsOn = dependsOn,
            Min = minValue,
            Max = maxValue
        });

    protected void CreateParameter<T>(Enum lookup, ParameterMode mode, string parameterName, string displayName, string description)
        => Parameters.Add(lookup, new ModuleParameter
        {
            Name = displayName,
            Description = description,
            Default = parameterName,
            DependsOn = null,
            Mode = mode,
            ExpectedType = typeof(T)
        });

    internal bool DoesSettingExist(string lookup, [NotNullWhen(true)] out ModuleAttribute? attribute)
    {
        if (Settings.TryGetValue(lookup, out var setting))
        {
            attribute = setting;
            return true;
        }

        attribute = null;
        return false;
    }

    internal bool DoesParameterExist(string lookup, [NotNullWhen(true)] out ModuleAttribute? attribute)
    {
        foreach (var (lookupToCheck, _) in Parameters)
        {
            if (lookupToCheck.ToLookup() != lookup) continue;

            attribute = Parameters[lookupToCheck];
            return true;
        }

        attribute = null;
        return false;
    }

    #endregion

    #region Events

    private static Regex parameterToRegex(string parameterName)
    {
        var pattern = parameterName.Replace(@"/", @"\/").Replace(@"*", @"(\S*)");
        return new Regex(pattern);
    }

    internal void Start()
    {
        State.Value = ModuleState.Starting;

        ParameterNameEnum.Clear();
        Parameters.ForEach(pair => ParameterNameEnum.Add(pair.Value.ParameterName, pair.Key));

        ParameterNameRegex.Clear();
        Parameters.ForEach(pair => ParameterNameRegex.Add(pair.Value.ParameterName, parameterToRegex(pair.Value.ParameterName)));

        try
        {
            OnModuleStart();
        }
        catch (Exception e)
        {
            notifications.Notify(new ExceptionNotification($"{Title} experienced an exception. Report on the Discord"));
            Logger.Error(e, $"{Name} experienced an exception");
        }

        State.Value = ModuleState.Started;

        Scheduler.AddDelayed(FixedUpdate, TimeSpan.FromSeconds(1f / 60f).TotalMilliseconds, true);

        if (ShouldUpdate) Scheduler.AddDelayed(Update, DeltaUpdate.TotalMilliseconds, true);
        if (ShouldUpdateImmediately) Update();
    }

    internal void Stop()
    {
        State.Value = ModuleState.Stopping;

        try
        {
            OnModuleStop();
        }
        catch (Exception e)
        {
            notifications.Notify(new ExceptionNotification($"{Title} experienced an exception. Report on the Discord"));
            Logger.Error(e, $"{Name} experienced an exception");
        }

        State.Value = ModuleState.Stopped;
    }

    internal void Update()
    {
        try
        {
            OnModuleUpdate();
        }
        catch (Exception e)
        {
            notifications.Notify(new ExceptionNotification($"{Title} experienced an exception. Report on the Discord"));
            Logger.Error(e, $"{Name} experienced an exception");
        }
    }

    internal void FixedUpdate()
    {
        try
        {
            OnFixedUpdate();
        }
        catch (Exception e)
        {
            notifications.Notify(new ExceptionNotification($"{Title} experienced an exception. Report on the Discord"));
            Logger.Error(e, $"{Name} experienced an exception");
        }
    }

    internal void PlayerUpdate()
    {
        try
        {
            OnPlayerUpdate();
        }
        catch (Exception e)
        {
            notifications.Notify(new ExceptionNotification($"{Title} experienced an exception. Report on the Discord"));
            Logger.Error(e, $"{Name} experienced an exception");
        }
    }

    internal void AvatarChange()
    {
        try
        {
            OnAvatarChange();
        }
        catch (Exception e)
        {
            notifications.Notify(new ExceptionNotification($"{Title} experienced an exception. Report on the Discord"));
            Logger.Error(e, $"{Name} experienced an exception");
        }
    }

    protected virtual void OnModuleStart() { }
    protected virtual void OnModuleUpdate() { }
    protected virtual void OnFixedUpdate() { }
    protected virtual void OnModuleStop() { }
    protected virtual void OnAvatarChange() { }
    protected virtual void OnPlayerUpdate() { }

    #endregion

    #region Settings

    protected List<T> GetSettingList<T>(Enum lookup)
    {
        var setting = Settings[lookup.ToLookup()];

        if (setting.GetType().IsSubclassOf(typeof(ModuleAttributePrimitiveList<T>)))
        {
            if (setting is ModuleAttributeList<Bindable<T>> settingList)
            {
                return settingList.Attribute.Select(bindable => bindable.Value).ToList();
            }
        }
        else
        {
            if (setting is ModuleAttributeList<T> settingList)
            {
                return settingList.Attribute.ToList();
            }
        }

        throw new InvalidCastException($"Setting with lookup '{lookup}' is not of type List<'{typeof(T)}>'");
    }

    protected T GetSetting<T>(Enum lookup)
    {
        var setting = Settings[lookup.ToLookup()];

        if (!setting.IsValueType<T>())
            throw new InvalidCastException($"Setting with lookup '{lookup}' is not of type '{nameof(T)}'");

        return ((ModuleAttribute<T>)setting).Value;
    }

    #endregion

    #region Parameters

    [Obsolete("Use SendParameter<T>(lookup, value) instead")]
    protected void SendParameter<T>(Enum lookup, T value, string suffix) where T : struct
    {
        SendParameter(lookup, value);
    }

    /// <summary>
    /// Sends a <paramref name="value"/> to an address denoted by <paramref name="lookup"/>
    /// </summary>
    /// <param name="lookup">The lookup of the parameter</param>
    /// <param name="value">The value to send</param>
    protected void SendParameter<T>(Enum lookup, T value) where T : struct
    {
        if (HasStopped) return;

        if (!Parameters.ContainsKey(lookup)) throw new InvalidOperationException($"Parameter {lookup.GetType().Name}.{lookup} has not been defined");

        var data = Parameters[lookup];
        if (!data.Mode.HasFlagFast(ParameterMode.Write)) throw new InvalidOperationException($"Parameter {lookup.GetType().Name}.{lookup} is a read-only parameter and therefore can't be sent!");

        Scheduler.Add(() => OscClient.SendValue(data.FormattedAddress, value));
    }

    internal void OnParameterReceived(VRChatOscData data)
    {
        if (!HasStarted) return;

        if (data.IsAvatarChangeEvent)
        {
            AvatarChange();
            return;
        }

        if (!data.IsAvatarParameter) return;

        try
        {
            OnAnyParameterReceived(data);
        }
        catch (Exception e)
        {
            notifications.Notify(new ExceptionNotification($"{Title} experienced an exception. Report on the Discord"));
            Logger.Error(e, $"{Name} experienced an exception");
        }

        var parameterName = Parameters.Values.FirstOrDefault(moduleParameter => ParameterNameRegex[moduleParameter.ParameterName].IsMatch(data.ParameterName))?.ParameterName;
        if (parameterName is null) return;

        var wildcards = new List<string>();

        var match = ParameterNameRegex[parameterName].Match(data.ParameterName);
        if (match.Groups.Count > 1) wildcards.AddRange(match.Groups.Values.Skip(1).Select(group => group.Value));

        if (!ParameterNameEnum.TryGetValue(parameterName, out var lookup)) return;

        var parameterData = Parameters[lookup];

        if (!parameterData.Mode.HasFlagFast(ParameterMode.Read)) return;

        if (!data.IsValueType(parameterData.ExpectedType))
        {
            Log($@"Cannot accept input parameter. `{lookup}` expects type `{parameterData.ExpectedType}` but received type `{data.ParameterValue.GetType()}`");
            return;
        }

        try
        {
            switch (data.ParameterValue)
            {
                case bool boolValue:
                    if (wildcards.Any())
                        OnBoolParameterReceived(lookup, boolValue, wildcards.ToArray());
                    else
                        OnBoolParameterReceived(lookup, boolValue);

                    break;

                case int intValue:
                    if (wildcards.Any())
                        OnIntParameterReceived(lookup, intValue, wildcards.ToArray());
                    else
                        OnIntParameterReceived(lookup, intValue);
                    break;

                case float floatValue:
                    if (wildcards.Any())
                        OnFloatParameterReceived(lookup, floatValue, wildcards.ToArray());
                    else
                        OnFloatParameterReceived(lookup, floatValue);
                    break;
            }
        }
        catch (Exception e)
        {
            notifications.Notify(new ExceptionNotification($"{Title} experienced an exception. Report on the Discord"));
            Logger.Error(e, $"{Name} experienced an exception");
        }
    }

    protected virtual void OnAnyParameterReceived(VRChatOscData data) { }
    protected virtual void OnBoolParameterReceived(Enum key, bool value) { }
    protected virtual void OnIntParameterReceived(Enum key, int value) { }
    protected virtual void OnFloatParameterReceived(Enum key, float value) { }
    protected virtual void OnBoolParameterReceived(Enum key, bool value, string[] wildcards) { }
    protected virtual void OnIntParameterReceived(Enum key, int value, string[] wildcards) { }
    protected virtual void OnFloatParameterReceived(Enum key, float value, string[] wildcards) { }

    #endregion

    #region Extensions

    protected void Log(string message) => Terminal.Log(message);

    /// <summary>
    /// Logs to a special module-debug file when the --module-debug flag is passed on startup
    /// </summary>
    protected void LogDebug(string message) => moduleDebugLogger.Log(message);

    protected void OpenUrlExternally(string Url) => Host.OpenUrlExternally(Url);

    protected static float Map(float source, float sMin, float sMax, float dMin, float dMax) => dMin + (dMax - dMin) * ((source - sMin) / (sMax - sMin));

    #endregion

    public enum ModuleState
    {
        Starting,
        Started,
        Stopping,
        Stopped
    }

    public enum ModuleType
    {
        Health,
        Accessibility,
        OpenVR,
        Integrations,
        General,
        NSFW
    }

    public int CompareTo(Module? other)
    {
        if (other is null) return 1;
        if (Type > other.Type) return 1;
        if (Type < other.Type) return -1;

        return string.CompareOrdinal(Title, other.Title);
    }
}
