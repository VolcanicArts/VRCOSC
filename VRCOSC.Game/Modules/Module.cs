// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using VRCOSC.OpenVR;
using VRCOSC.OSC.VRChat;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable InconsistentNaming

namespace VRCOSC.Game.Modules;

public abstract partial class Module : Component, IComparable<Module>
{
    [Resolved]
    private GameHost Host { get; set; } = null!;

    [Resolved]
    private GameManager GameManager { get; set; } = null!;

    private Storage Storage = null!;
    private TerminalLogger Terminal = null!;

    protected Player Player => GameManager.Player;
    protected OVRClient OVRClient => GameManager.OVRClient;
    protected Bindable<ModuleState> State = new(ModuleState.Stopped);

    public readonly BindableBool Enabled = new();
    public readonly Dictionary<string, ModuleAttribute> Settings = new();
    public readonly Dictionary<Enum, ParameterMetadata> Parameters = new();
    private readonly Dictionary<string, Enum> parametersReversed = new();

    public virtual string Title => string.Empty;
    public virtual string Description => string.Empty;
    public virtual string Author => string.Empty;
    public virtual string Prefab => string.Empty;
    public virtual ModuleType Type => ModuleType.General;
    protected virtual TimeSpan DeltaUpdate => TimeSpan.MaxValue;
    protected virtual int ChatBoxPriority => 0;

    private bool IsEnabled => Enabled.Value;
    private bool ShouldUpdate => DeltaUpdate != TimeSpan.MaxValue;
    private string FileName => @$"{GetType().Name}.ini";

    protected bool IsStarting => State.Value == ModuleState.Starting;
    protected bool HasStarted => State.Value == ModuleState.Started;
    protected bool IsStopping => State.Value == ModuleState.Stopping;
    protected bool HasStopped => State.Value == ModuleState.Stopped;

    public bool HasSettings => Settings.Any();
    public bool HasParameters => Parameters.Any();

    [BackgroundDependencyLoader]
    public void load(Storage storage)
    {
        Storage = storage.GetStorageForDirectory("modules");
        Terminal = new TerminalLogger(Title);

        CreateAttributes();

        Parameters.ForEach(pair => parametersReversed.Add(pair.Value.Name, pair.Key));

        performLoad();
    }

    protected override void LoadComplete()
    {
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

    protected void CreateSetting(Enum lookup, string displayName, string description, IEnumerable<string> defaultValues, bool canBeEmpty, Func<bool>? dependsOn = null)
        => addEnumerableSetting(lookup, displayName, description, defaultValues, canBeEmpty, dependsOn);

    protected void CreateSetting(Enum lookup, string displayName, string description, string defaultValue, string buttonText, Action buttonAction, Func<bool>? dependsOn = null)
        => addTextAndButtonSetting(lookup, displayName, description, defaultValue, buttonText, buttonAction, dependsOn);

    protected void CreateParameter<T>(Enum lookup, ParameterMode mode, string parameterName, string description)
        => Parameters.Add(lookup, new ParameterMetadata(mode, parameterName, description, typeof(T)));

    private void addSingleSetting(Enum lookup, string displayName, string description, object defaultValue, Func<bool>? dependsOn)
        => Settings.Add(lookup.ToLookup(), new ModuleAttributeSingle(new ModuleAttributeMetadata(displayName, description), defaultValue, dependsOn));

    private void addEnumerableSetting<T>(Enum lookup, string displayName, string description, IEnumerable<T> defaultValues, bool canBeEmpty, Func<bool>? dependsOn)
        => Settings.Add(lookup.ToLookup(), new ModuleAttributeList(new ModuleAttributeMetadata(displayName, description), defaultValues.Cast<object>(), typeof(T), canBeEmpty, dependsOn));

    private void addRangedSetting<T>(Enum lookup, string displayName, string description, T defaultValue, T minValue, T maxValue, Func<bool>? dependsOn) where T : struct
        => Settings.Add(lookup.ToLookup(), new ModuleAttributeSingleWithBounds(new ModuleAttributeMetadata(displayName, description), defaultValue, minValue, maxValue, dependsOn));

    private void addTextAndButtonSetting(Enum lookup, string displayName, string description, string defaultValue, string buttonText, Action buttonAction, Func<bool>? dependsOn)
        => Settings.Add(lookup.ToLookup(), new ModuleAttributeSingleWithButton(new ModuleAttributeMetadata(displayName, description), defaultValue, buttonText, buttonAction, dependsOn));

    #endregion

    #region Events

    internal void start()
    {
        if (!IsEnabled) return;

        State.Value = ModuleState.Starting;

        OnModuleStart();

        if (ShouldUpdate)
        {
            Scheduler.Add(OnModuleUpdate);
            Scheduler.AddDelayed(OnModuleUpdate, DeltaUpdate.TotalMilliseconds, true);
        }

        GameManager.OscClient.OnParameterReceived += onParameterReceived;

        State.Value = ModuleState.Started;
    }

    internal void stop()
    {
        if (!IsEnabled) return;

        State.Value = ModuleState.Stopping;

        GameManager.OscClient.OnParameterReceived -= onParameterReceived;

        Scheduler.CancelDelayedTasks();

        OnModuleStop();

        State.Value = ModuleState.Stopped;
    }

    protected virtual void OnModuleStart() { }
    protected virtual void OnModuleUpdate() { }
    protected virtual void OnModuleStop() { }
    protected virtual void OnAvatarChange() { }

    #endregion

    #region Settings

    protected T GetSetting<T>(Enum lookup)
    {
        var setting = Settings[lookup.ToLookup()];

        object? value;

        switch (setting)
        {
            case ModuleAttributeSingle settingSingle:
                value = settingSingle.Attribute.Value;
                break;

            case ModuleAttributeList settingList when settingList.Type == typeof(string):
                value = settingList.GetValueList<string>();
                break;

            case ModuleAttributeList settingList when settingList.Type == typeof(int):
                value = settingList.GetValueList<int>();
                break;

            default:
                value = null;
                break;
        }

        if (value is not T valueCast)
            throw new InvalidCastException($"Setting with lookup '{lookup}' is not of type '{nameof(T)}'");

        return valueCast;
    }

    #endregion

    #region Parameters

    protected void SendParameter<T>(Enum lookup, T value) where T : struct
    {
        if (HasStopped) return;

        if (!Parameters.ContainsKey(lookup)) throw new InvalidOperationException($"Parameter {lookup} has not been defined");

        var data = Parameters[lookup];
        if (!data.Mode.HasFlagFast(ParameterMode.Write)) throw new InvalidOperationException("Cannot send a value to a read-only parameter");

        GameManager.OscClient.SendValue(data.FormattedAddress, value);
    }

    private void onParameterReceived(VRChatOscData data)
    {
        if (data.IsAvatarChangeEvent)
        {
            OnAvatarChange();
            return;
        }

        if (!data.IsAvatarParameter || !parametersReversed.ContainsKey(data.ParameterName)) return;

        var lookup = parametersReversed[data.ParameterName];
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

    protected DateTimeOffset SetChatBoxText(string? text, TimeSpan displayLength) => GameManager.ChatBoxInterface.SetText(text, ChatBoxPriority, displayLength);

    protected void SetChatBoxTyping(bool typing) => GameManager.ChatBoxInterface.SetTyping(typing);

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
        Health = 0,
        OpenVR = 1,
        Integrations = 2,
        General = 3,
    }

    public int CompareTo(Module? other)
    {
        if (other is null) return 1;
        if (Type > other.Type) return 1;
        if (Type < other.Type) return -1;

        return Type == other.Type ? string.CompareOrdinal(Title, other.Title) : 0;
    }
}
