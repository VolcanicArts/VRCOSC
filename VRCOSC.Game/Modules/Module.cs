// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Platform;
using osu.Framework.Threading;
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
    private GameManager GameManager = null!;
    private Scheduler Scheduler = null!;

    private TerminalLogger Terminal = null!;

    protected Player Player => GameManager.Player;
    protected OVRClient OVRClient => GameManager.OVRClient;
    protected VRChatOscClient OscClient => GameManager.VRChatOscClient;
    protected ChatBoxManager ChatBoxManager => GameManager.ChatBoxManager;
    protected Bindable<ModuleState> State = new(ModuleState.Stopped);
    protected AvatarConfig? AvatarConfig => GameManager.AvatarConfig;

    internal readonly BindableBool Enabled = new();
    internal readonly Dictionary<string, ModuleAttribute> Settings = new();
    internal readonly Dictionary<Enum, ModuleParameter> Parameters = new();

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
    internal string FileName => @$"{Name}.ini";

    protected bool IsStarting => State.Value == ModuleState.Starting;
    protected bool HasStarted => State.Value == ModuleState.Started;
    protected bool IsStopping => State.Value == ModuleState.Stopping;
    protected bool HasStopped => State.Value == ModuleState.Stopped;

    private SerialisationManager serialisationManager = null!;

    public void InjectDependencies(GameHost host, GameManager gameManager, Scheduler scheduler, Storage storage, NotificationContainer notifications)
    {
        Host = host;
        GameManager = gameManager;
        Scheduler = scheduler;

        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(1, new ModuleSerialiser(storage, notifications, this));
    }

    public void Load()
    {
        Terminal = new TerminalLogger(Title);

        CreateAttributes();
        Settings.Values.ForEach(setting => setting.Setup());
        Parameters.Values.ForEach(parameter => parameter.Setup());

        State.ValueChanged += _ => Log(State.Value.ToString());

        Deserialise();
    }

    #region Serialisation

    public void Serialise()
    {
        serialisationManager.Serialise();
    }

    public void Deserialise()
    {
        serialisationManager.Deserialise();
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

    public bool DoesSettingExist(string lookup, [NotNullWhen(returnValue: true)] out ModuleAttribute? attribute)
    {
        if (Settings.TryGetValue(lookup, out var setting))
        {
            attribute = setting;
            return true;
        }

        attribute = null;
        return false;
    }

    public bool DoesParameterExist(string lookup, [NotNullWhen(returnValue: true)] out ModuleAttribute? attribute)
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

    internal void Start()
    {
        State.Value = ModuleState.Starting;

        OnModuleStart();

        if (ShouldUpdate) Scheduler.AddDelayed(OnModuleUpdate, DeltaUpdate.TotalMilliseconds, true);
        Scheduler.AddDelayed(OnFixedUpdate, TimeSpan.FromSeconds(1f / 60f).TotalMilliseconds, true);

        State.Value = ModuleState.Started;

        if (ShouldUpdateImmediately) OnModuleUpdate();
    }

    internal void Stop()
    {
        State.Value = ModuleState.Stopping;

        OnModuleStop();

        State.Value = ModuleState.Stopped;
    }

    internal void PlayerUpdate() => OnPlayerUpdate();

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

        if (setting.GetType().IsSubclassOf(typeof(ModuleAttributePrimitiveList<>)))
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

        throw new InvalidCastException($"Setting with lookup '{lookup}' is not of type '{nameof(List<T>)}'");
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

    /// <summary>
    /// Sends a parameter value to a specified parameter name (that may have been modified), with an optional parameter name suffix
    /// </summary>
    /// <param name="lookup">The lookup key of the parameter</param>
    /// <param name="value">The value to send</param>
    /// <param name="suffix">The optional suffix to add to the parameter name</param>
    protected void SendParameter<T>(Enum lookup, T value, string suffix = "") where T : struct
    {
        if (HasStopped) return;

        if (!Parameters.ContainsKey(lookup)) throw new InvalidOperationException($"Parameter {lookup} has not been defined");

        var data = Parameters[lookup];
        if (!data.Mode.HasFlagFast(ParameterMode.Write)) throw new InvalidOperationException("Cannot send a value to a read-only parameter");

        OscClient.SendValue(data.FormattedAddress + suffix, value);
    }

    internal void OnParameterReceived(VRChatOscData data)
    {
        if (!IsEnabled) return;
        if (!HasStarted) return;

        if (data.IsAvatarChangeEvent)
        {
            OnAvatarChange();
            return;
        }

        if (!data.IsAvatarParameter) return;

        OnAnyParameterReceived(data);

        Enum lookup;

        try
        {
            lookup = Parameters.Single(pair => pair.Value.ParameterName == data.ParameterName).Key;
        }
        catch (InvalidOperationException)
        {
            return;
        }

        var parameterData = Parameters[lookup];

        if (!parameterData.Mode.HasFlagFast(ParameterMode.Read)) return;

        if (!data.IsValueType(parameterData.ExpectedType))
        {
            Log($@"Cannot accept input parameter. `{lookup}` expects type `{parameterData.ExpectedType}` but received type `{data.ParameterValue.GetType()}`");
            return;
        }

        switch (data.ParameterValue)
        {
            case bool boolValue:
                OnBoolParameterReceived(lookup, boolValue);
                break;

            case int intValue:
                OnIntParameterReceived(lookup, intValue);
                break;

            case float floatValue:
                OnFloatParameterReceived(lookup, floatValue);
                break;
        }
    }

    protected virtual void OnAnyParameterReceived(VRChatOscData data) { }
    protected virtual void OnBoolParameterReceived(Enum key, bool value) { }
    protected virtual void OnIntParameterReceived(Enum key, int value) { }
    protected virtual void OnFloatParameterReceived(Enum key, float value) { }

    #endregion

    #region Extensions

    protected void Log(string message) => Terminal.Log(message);

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
        General
    }

    public int CompareTo(Module? other)
    {
        if (other is null) return 1;
        if (Type > other.Type) return 1;
        if (Type < other.Type) return -1;

        return string.CompareOrdinal(Title, other.Title);
    }
}
