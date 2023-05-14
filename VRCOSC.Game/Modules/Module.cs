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
using VRCOSC.Game.Managers;
using VRCOSC.Game.Modules.Avatar;
using VRCOSC.Game.OpenVR;
using VRCOSC.Game.OSC.VRChat;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming

namespace VRCOSC.Game.Modules;

public abstract class Module : IComparable<Module>
{
    private GameHost Host = null!;
    private GameManager GameManager = null!;
    protected IVRCOSCSecrets Secrets { get; private set; } = null!;
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
    internal readonly Dictionary<Enum, ParameterAttribute> Parameters = new();
    internal readonly Dictionary<string, Enum> ParametersLookup = new();

    public virtual string Title => string.Empty;
    public virtual string Description => string.Empty;
    public virtual string Author => string.Empty;
    public virtual string Prefab => string.Empty;
    public virtual ModuleType Type => ModuleType.General;
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

    internal bool HasSettings => Settings.Any();
    internal bool HasParameters => Parameters.Any();

    public void InjectDependencies(GameHost host, GameManager gameManager, IVRCOSCSecrets secrets, Scheduler scheduler)
    {
        Host = host;
        GameManager = gameManager;
        Secrets = secrets;
        Scheduler = scheduler;
    }

    public void Load()
    {
        Terminal = new TerminalLogger(Title);

        CreateAttributes();

        Parameters.ForEach(pair => ParametersLookup.Add(pair.Key.ToLookup(), pair.Key));
        State.ValueChanged += _ => Log(State.Value.ToString());
    }

    #region Attributes

    protected virtual void CreateAttributes() { }

    protected void CreateSetting(Enum lookup, string displayName, string description, bool defaultValue, Func<bool>? dependsOn = null)
        => addSingleSetting(lookup, displayName, description, defaultValue, dependsOn);

    protected void CreateSetting(Enum lookup, string displayName, string description, int defaultValue, Func<bool>? dependsOn = null)
        => addSingleSetting(lookup, displayName, description, defaultValue, dependsOn);

    protected void CreateSetting(Enum lookup, string displayName, string description, string defaultValue, Func<bool>? dependsOn = null)
        => addSingleSetting(lookup, displayName, description, defaultValue, dependsOn);

    protected void CreateSetting(Enum lookup, string displayName, string description, Enum defaultValue, Func<bool>? dependsOn = null)
        => addSingleSetting(lookup, displayName, description, defaultValue, dependsOn);

    protected void CreateSetting(Enum lookup, string displayName, string description, int defaultValue, int minValue, int maxValue, Func<bool>? dependsOn = null)
        => addRangedSetting(lookup, displayName, description, defaultValue, minValue, maxValue, dependsOn);

    protected void CreateSetting(Enum lookup, string displayName, string description, float defaultValue, float minValue, float maxValue, Func<bool>? dependsOn = null)
        => addRangedSetting(lookup, displayName, description, defaultValue, minValue, maxValue, dependsOn);

    protected void CreateSetting(Enum lookup, string displayName, string description, string defaultValue, string buttonText, Action buttonAction, Func<bool>? dependsOn = null)
        => addTextAndButtonSetting(lookup, displayName, description, defaultValue, buttonText, buttonAction, dependsOn);

    protected void CreateParameter<T>(Enum lookup, ParameterMode mode, string parameterName, string displayName, string description)
        => Parameters.Add(lookup, new ParameterAttribute(mode, new ModuleAttributeMetadata(displayName, description), parameterName, typeof(T), null));

    private void addSingleSetting(Enum lookup, string displayName, string description, object defaultValue, Func<bool>? dependsOn)
        => Settings.Add(lookup.ToLookup(), new ModuleAttribute(new ModuleAttributeMetadata(displayName, description), defaultValue, dependsOn));

    private void addRangedSetting<T>(Enum lookup, string displayName, string description, T defaultValue, T minValue, T maxValue, Func<bool>? dependsOn) where T : struct
        => Settings.Add(lookup.ToLookup(), new ModuleAttributeWithBounds(new ModuleAttributeMetadata(displayName, description), defaultValue, minValue, maxValue, dependsOn));

    private void addTextAndButtonSetting(Enum lookup, string displayName, string description, string defaultValue, string buttonText, Action buttonAction, Func<bool>? dependsOn)
        => Settings.Add(lookup.ToLookup(), new ModuleAttributeWithButton(new ModuleAttributeMetadata(displayName, description), defaultValue, buttonText, buttonAction, dependsOn));

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

    public bool DoesParameterExist(string lookup, [NotNullWhen(returnValue: true)] out ModuleAttribute? key)
    {
        foreach (var (lookupToCheck, _) in Parameters)
        {
            if (lookupToCheck.ToLookup() != lookup) continue;

            key = Parameters[lookupToCheck];
            return true;
        }

        key = null;
        return false;
    }

    #endregion

    #region Events

    internal void Start()
    {
        State.Value = ModuleState.Starting;

        OnModuleStart();

        if (ShouldUpdate) Scheduler.AddDelayed(OnModuleUpdate, DeltaUpdate.TotalMilliseconds, true);

        State.Value = ModuleState.Started;

        if (ShouldUpdateImmediately) OnModuleUpdate();
    }

    internal void Update() => OnFixedUpdate();

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

    protected T GetSetting<T>(Enum lookup)
    {
        object? value = Settings[lookup.ToLookup()].Attribute.Value;

        if (value is not T valueCast)
            throw new InvalidCastException($"Setting with lookup '{lookup}' is not of type '{nameof(T)}'");

        return valueCast;
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

        Enum lookup;

        try
        {
            lookup = Parameters.Single(pair => pair.Value.Name == data.ParameterName).Key;
        }
        catch (InvalidOperationException)
        {
            return;
        }

        var parameterData = Parameters[lookup];

        if (!parameterData.Mode.HasFlagFast(ParameterMode.Read)) return;

        if (data.ParameterValue.GetType() != parameterData.ExpectedType)
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
