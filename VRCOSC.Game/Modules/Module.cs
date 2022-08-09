// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Logging;
using osu.Framework.Platform;
using VRCOSC.Game.Modules.Util;
using VRCOSC.Game.Util;

// ReSharper disable InconsistentNaming

namespace VRCOSC.Game.Modules;

public abstract class Module
{
    private Storage Storage = null!;
    private OscClient OscClient = null!;
    private TimedTask? updateTask;
    protected TerminalLogger Terminal = null!;
    protected Player Player = null!;
    protected ModuleState ModuleState = ModuleState.Stopped;

    public readonly BindableBool Enabled = new();

    public readonly Dictionary<string, ModuleAttributeData> Settings = new();
    public readonly Dictionary<string, ModuleAttributeData> OutputParameters = new();

    private readonly Dictionary<Enum, InputParameterData> InputParameters = new();

    public virtual string Title => string.Empty;
    public virtual string Description => string.Empty;
    public virtual string Author => string.Empty;
    public virtual ColourInfo Colour => Colour4.Black;
    public virtual ModuleType ModuleType => ModuleType.General;
    public virtual string Prefab => string.Empty;
    protected virtual double DeltaUpdate => double.PositiveInfinity;

    public void Initialise(Storage storage, OscClient oscClient)
    {
        Storage = storage;
        OscClient = oscClient;
        Terminal = new TerminalLogger(GetType().Name);

        CreateAttributes();
        performLoad();
    }

    #region Properties

    public bool HasSettings => Settings.Count != 0;
    public bool HasOutputParameters => OutputParameters.Count != 0;
    public bool HasAttributes => HasSettings || HasOutputParameters;

    private string FileName => $"{GetType().Name}.ini";

    #endregion

    #region Attributes

    protected virtual void CreateAttributes() { }

    protected void CreateSetting(Enum lookup, string displayName, string description, bool defaultValue)
    {
        addSetting(lookup.ToString().ToLower(), displayName, description, defaultValue);
    }

    protected void CreateSetting(Enum lookup, string displayName, string description, int defaultValue)
    {
        addSetting(lookup.ToString().ToLower(), displayName, description, defaultValue);
    }

    protected void CreateSetting(Enum lookup, string displayName, string description, int defaultValue, int minValue, int maxValue)
    {
        addRangedSetting(lookup.ToString().ToLower(), displayName, description, defaultValue, minValue, maxValue);
    }

    protected void CreateSetting(Enum lookup, string displayName, string description, float defaultValue, float minValue, float maxValue)
    {
        addRangedSetting(lookup.ToString().ToLower(), displayName, description, defaultValue, minValue, maxValue);
    }

    protected void CreateSetting(Enum lookup, string displayName, string description, string defaultValue)
    {
        addSetting(lookup.ToString().ToLower(), displayName, description, defaultValue);
    }

    protected void CreateSetting<T>(Enum lookup, string displayName, string description, T defaultValue) where T : Enum
    {
        addSetting(lookup.ToString().ToLower(), displayName, description, defaultValue);
    }

    private void addSetting(string lookup, string displayName, string description, object defaultValue)
    {
        Settings.Add(lookup, new ModuleAttributeData(displayName, description, defaultValue));
    }

    private void addRangedSetting<T>(string lookup, string displayName, string description, T defaultValue, T minValue, T maxValue) where T : struct
    {
        Settings.Add(lookup, new ModuleAttributeDataWithBounds(displayName, description, defaultValue, minValue, maxValue));
    }

    protected void CreateOutputParameter(Enum lookup, string displayName, string description, string defaultAddress)
    {
        var lookupString = lookup.ToString().ToLower();
        OutputParameters.Add(lookupString, new ModuleAttributeData(displayName, description, defaultAddress));
    }

    protected void RegisterGenericInputParameter<T>(Enum lookup) where T : struct
    {
        InputParameters.Add(lookup, new InputParameterData(typeof(T)));
    }

    protected void RegisterButtonInput(Enum lookup)
    {
        InputParameters.Add(lookup, new InputParameterData(typeof(bool), ActionMenu.Button));
    }

    protected void RegisterRadialInput(Enum lookup)
    {
        InputParameters.Add(lookup, new InputParameterData(typeof(float), ActionMenu.Radial));
    }

    #endregion

    #region Events

    internal void start()
    {
        if (!Enabled.Value) return;

        Terminal.Log("Starting");
        Player = new Player(OscClient);

        OnStart();

        if (!double.IsPositiveInfinity(DeltaUpdate))
        {
            updateTask = new TimedTask(OnUpdate, DeltaUpdate, true);
            updateTask.Start();
        }

        OscClient.OnParameterReceived += onParameterReceived;

        Terminal.Log("Started");
        ModuleState = ModuleState.Started;
    }

    protected virtual void OnStart() { }

    protected virtual void OnUpdate() { }

    internal async Task stop()
    {
        if (!Enabled.Value) return;

        Terminal.Log("Stopping");

        OscClient.OnParameterReceived -= onParameterReceived;

        if (updateTask is not null) await updateTask.Stop();

        OnStop();

        Player.ResetAll();

        Terminal.Log("Stopped");
        ModuleState = ModuleState.Stopped;
    }

    protected virtual void OnStop() { }

    protected virtual void OnAvatarChange() { }

    protected virtual void OnPlayerStateUpdate(VRChatInputParameter key) { }

    #endregion

    #region Settings

    protected T GetSetting<T>(Enum lookup) => getSetting<T>(lookup.ToString().ToLower());

    private T getSetting<T>(string lookup)
    {
        var value = Settings[lookup].Attribute.Value;

        if (value is not T valueCast)
            throw new InvalidCastException($"Setting with lookup '{lookup}' is not of type '{nameof(T)}'");

        return valueCast;
    }

    #endregion

    #region IncomingParameters

    private void onParameterReceived(string address, object value)
    {
        var parameterName = address.Split('/').Last();

        updatePlayerState(parameterName, value);

        if (parameterName.Equals("change"))
        {
            OnAvatarChange();
            return;
        }

        Enum? key = InputParameters.Keys.ToList().Find(e => e.ToString().Equals(parameterName));
        if (key == null) return;

        var inputParameterData = InputParameters[key];

        if (value.GetType() != inputParameterData.Type)
        {
            Terminal.Log($@"Cannot accept input parameter. `{key}` expects type `{inputParameterData.Type}` but received type `{value.GetType()}`");
            return;
        }

        notifyParameterReceived(key, value, inputParameterData);
    }

    private void notifyParameterReceived(Enum key, object value, InputParameterData data)
    {
        switch (value)
        {
            case bool boolValue:
                Terminal.Log($"Received bool of key `{key}`");
                OnBoolParameterReceived(key, boolValue);
                if (data.ActionMenu == ActionMenu.Button && boolValue) OnButtonPressed(key);
                break;

            case int intValue:
                Terminal.Log($"Received int of key `{key}`");
                OnIntParameterReceived(key, intValue);
                break;

            case float floatValue:
                Terminal.Log($"Received float of key `{key}`");
                OnFloatParameterReceived(key, floatValue);

                if (data.ActionMenu == ActionMenu.Radial)
                {
                    var radialData = (RadialInputParameterData)data;
                    OnRadialPuppetChange(key, new VRChatRadialPuppet(floatValue, radialData.PreviousValue));
                    radialData.PreviousValue = floatValue;
                }

                break;
        }
    }

    private void updatePlayerState(string parameterName, object value)
    {
        if (!Enum.TryParse(parameterName, out VRChatInputParameter vrChatInputParameter)) return;

        switch (vrChatInputParameter)
        {
            case VRChatInputParameter.Viseme:
                Player.Viseme = (Viseme)(int)value;
                break;

            case VRChatInputParameter.Voice:
                Player.Voice = (float)value;
                break;

            case VRChatInputParameter.GestureLeft:
                Player.GestureLeft = (Gesture)(int)value;
                break;

            case VRChatInputParameter.GestureRight:
                Player.GestureRight = (Gesture)(int)value;
                break;

            case VRChatInputParameter.GestureLeftWeight:
                Player.GestureLeftWeight = (float)value;
                break;

            case VRChatInputParameter.GestureRightWeight:
                Player.GestureRightWeight = (float)value;
                break;

            case VRChatInputParameter.AngularY:
                Player.AngularY = (float)value;
                break;

            case VRChatInputParameter.VelocityX:
                Player.VelocityX = (float)value;
                break;

            case VRChatInputParameter.VelocityY:
                Player.VelocityY = (float)value;
                break;

            case VRChatInputParameter.VelocityZ:
                Player.VelocityZ = (float)value;
                break;

            case VRChatInputParameter.Upright:
                Player.Upright = (float)value;
                break;

            case VRChatInputParameter.Grounded:
                Player.Grounded = (bool)value;
                break;

            case VRChatInputParameter.Seated:
                Player.Seated = (bool)value;
                break;

            case VRChatInputParameter.AFK:
                Player.AFK = (bool)value;
                break;

            case VRChatInputParameter.TrackingType:
                Player.TrackingType = (TrackingType)(int)value;
                break;

            case VRChatInputParameter.VRMode:
                Player.IsVR = (int)value == 1;
                break;

            case VRChatInputParameter.MuteSelf:
                Player.IsMuted = (bool)value;
                break;

            case VRChatInputParameter.InStation:
                Player.InStation = (bool)value;
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        OnPlayerStateUpdate(vrChatInputParameter);
    }

    protected virtual void OnBoolParameterReceived(Enum key, bool value) { }

    protected virtual void OnIntParameterReceived(Enum key, int value) { }

    protected virtual void OnFloatParameterReceived(Enum key, float value) { }

    protected virtual void OnButtonPressed(Enum key) { }

    protected virtual void OnRadialPuppetChange(Enum key, VRChatRadialPuppet radialPuppet) { }

    #endregion

    #region OutgoingParameters

    private string getOutputParameter(Enum lookup) => getOutputParameter(lookup.ToString().ToLower());

    private string getOutputParameter(string lookup) => (string)OutputParameters[lookup].Attribute.Value;

    protected void SendParameter<T>(Enum lookup, T value) where T : struct
    {
        if (ModuleState == ModuleState.Stopped) return;

        switch (value)
        {
            case bool boolValue:
                OscClient.SendData(getOutputParameter(lookup), boolValue);
                break;

            case int intValue:
                OscClient.SendData(getOutputParameter(lookup), intValue);
                break;

            case float floatValue:
                OscClient.SendData(getOutputParameter(lookup), floatValue);
                break;

            default:
                throw new ArgumentOutOfRangeException($"Cannot send parameter of type {value.GetType().Name}");
        }
    }

    #endregion

    #region Loading

    private void performLoad()
    {
        using (var stream = Storage.GetStream(FileName))
        {
            if (stream is not null)
            {
                using var reader = new StreamReader(stream);

                while (reader.ReadLine() is { } line)
                {
                    switch (line)
                    {
                        case "#InternalSettings":
                            performInternalSettingsLoad(reader);
                            break;

                        case "#Settings":
                            performSettingsLoad(reader);
                            break;

                        case "#OutputParameters":
                            performOutputParametersLoad(reader);
                            break;
                    }
                }
            }
        }

        executeAfterLoad();
    }

    private void performInternalSettingsLoad(TextReader reader)
    {
        while (reader.ReadLine() is { } line)
        {
            if (line.Equals("#End")) break;

            var lineSplit = line.Split(new[] { '=' }, 2);
            var lookup = lineSplit[0];
            var value = lineSplit[1];

            switch (lookup)
            {
                case "enabled":
                    Enabled.Value = bool.Parse(value);
                    break;
            }
        }
    }

    private void performSettingsLoad(TextReader reader)
    {
        while (reader.ReadLine() is { } line)
        {
            if (line.Equals("#End")) break;

            var lineSplitLookupValue = line.Split(new[] { '=' }, 2);
            var lookupType = lineSplitLookupValue[0].Split(new[] { ':' }, 2);
            var value = lineSplitLookupValue[1];

            var lookup = lookupType[0];
            var typeStr = lookupType[1];

            if (!Settings.ContainsKey(lookup)) continue;

            var readableTypeName = Settings[lookup].Attribute.Value.GetType().ToReadableName().ToLowerInvariant();
            if (!readableTypeName.Equals(typeStr)) continue;

            switch (typeStr)
            {
                case "enum":
                    var typeAndValue = value.Split(new[] { '#' }, 2);
                    var enumType = enumNameToType(typeAndValue[0]);
                    if (enumType is not null) Settings[lookup].Attribute.Value = Enum.ToObject(enumType, int.Parse(typeAndValue[1]));
                    break;

                case "string":
                    Settings[lookup].Attribute.Value = value;
                    break;

                case "int":
                    Settings[lookup].Attribute.Value = int.Parse(value);
                    break;

                case "float":
                    Settings[lookup].Attribute.Value = float.Parse(value);
                    break;

                case "bool":
                    Settings[lookup].Attribute.Value = bool.Parse(value);
                    break;

                default:
                    Logger.Log($"Unknown type found in file: {typeStr}");
                    break;
            }
        }
    }

    private void performOutputParametersLoad(TextReader reader)
    {
        while (reader.ReadLine() is { } line)
        {
            if (line.Equals("#End")) break;

            var lineSplitLookupValue = line.Split(new[] { '=' }, 2);
            var lookup = lineSplitLookupValue[0];
            var value = lineSplitLookupValue[1];

            if (!OutputParameters.ContainsKey(lookup)) continue;

            OutputParameters[lookup].Attribute.Value = value;
        }
    }

    private void executeAfterLoad()
    {
        performSave();

        Enabled.BindValueChanged(_ => performSave());
        Settings.Values.ForEach(value => value.Attribute.BindValueChanged(_ => performSave()));
        OutputParameters.Values.ForEach(value => value.Attribute.BindValueChanged(_ => performSave()));
    }

    private static Type? enumNameToType(string enumName) => AppDomain.CurrentDomain.GetAssemblies().Select(assembly => assembly.GetType(enumName)).FirstOrDefault(type => type?.IsEnum ?? false);

    #endregion

    #region Saving

    private void performSave()
    {
        using var stream = Storage.CreateFileSafely(FileName);
        using var writer = new StreamWriter(stream);

        performInternalSettingsSave(writer);
        performSettingsSave(writer);
        performOutputParametersSave(writer);
    }

    private void performInternalSettingsSave(TextWriter writer)
    {
        writer.WriteLine(@"#InternalSettings");
        writer.WriteLine(@"{0}={1}", "enabled", Enabled.Value.ToString());
        writer.WriteLine(@"#End");
    }

    private void performSettingsSave(TextWriter writer)
    {
        var areAllDefault = Settings.All(pair => pair.Value.Attribute.IsDefault);
        if (areAllDefault) return;

        writer.WriteLine(@"#Settings");

        foreach (var (lookup, moduleAttributeData) in Settings)
        {
            if (moduleAttributeData.Attribute.IsDefault) continue;

            var value = moduleAttributeData.Attribute.Value;
            var valueType = value.GetType();
            var readableTypeName = valueType.ToReadableName().ToLowerInvariant();

            if (valueType.IsSubclassOf(typeof(Enum)))
            {
                var enumClass = valueType.FullName;
                writer.WriteLine(@"{0}:{1}={2}#{3}", lookup, readableTypeName, enumClass, (int)value);
            }
            else
            {
                writer.WriteLine(@"{0}:{1}={2}", lookup, readableTypeName, value);
            }
        }

        writer.WriteLine(@"#End");
    }

    private void performOutputParametersSave(TextWriter writer)
    {
        var areAllDefault = OutputParameters.All(pair => pair.Value.Attribute.IsDefault);
        if (areAllDefault) return;

        writer.WriteLine(@"#OutputParameters");

        foreach (var (lookup, moduleAttributeData) in OutputParameters)
        {
            if (moduleAttributeData.Attribute.IsDefault) continue;

            var value = moduleAttributeData.Attribute.Value;
            writer.WriteLine(@"{0}={1}", lookup, value);
        }

        writer.WriteLine(@"#End");
    }

    #endregion
}

public enum ModuleState
{
    Started,
    Stopped
}
