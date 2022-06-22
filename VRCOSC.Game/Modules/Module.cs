// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreOSC;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
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

    public readonly BindableBool Enabled = new();

    public readonly Dictionary<string, ModuleAttributeData> Settings = new();
    public readonly Dictionary<string, ModuleAttributeData> OutputParameters = new();

    public readonly Dictionary<Enum, InputParameterData> InputParameters = new();

    public virtual string Title => string.Empty;
    public virtual string Description => string.Empty;
    public virtual string Author => string.Empty;
    public virtual IEnumerable<string> Tags => Array.Empty<string>();
    public virtual double DeltaUpdate => double.PositiveInfinity;
    public virtual Colour4 Colour => Colour4.Black;
    public virtual ModuleType ModuleType => ModuleType.General;
    public virtual string Prefab => string.Empty;

    public void Initialise(Storage storage, OscClient oscClient)
    {
        Storage = storage;
        OscClient = oscClient;
        Terminal = new TerminalLogger(GetType().Name);

        OscClient.OnParameterReceived += onParameterReceived;
    }

    #region Properties

    public bool IsRequestingInput => InputParameters.Count != 0;

    public bool HasSettings => Settings.Count != 0;
    public bool HasOutputParameters => OutputParameters.Count != 0;
    public bool HasAttributes => HasSettings || HasOutputParameters;

    private string FileName => $"{GetType().Name}.ini";

    #endregion

    #region Attributes

    public virtual void CreateAttributes() { }

    protected void CreateSetting(Enum lookup, string displayName, string description, object defaultValue)
    {
        var lookupString = lookup.ToString().ToLower();
        Settings.Add(lookupString, new ModuleAttributeData(displayName, description, defaultValue));
    }

    protected void CreateOutputParameter(Enum lookup, string displayName, string description, string defaultAddress)
    {
        var lookupString = lookup.ToString().ToLower();
        OutputParameters.Add(lookupString, new ModuleAttributeData(displayName, description, defaultAddress));
    }

    protected void RegisterInputParameter<T>(Enum lookup, ActionMenu actionMenu = ActionMenu.None)
    {
        InputParameters.Add(lookup, new InputParameterData(typeof(T), actionMenu));
    }

    #endregion

    #region Events

    internal void start()
    {
        if (!Enabled.Value) return;

        Terminal.Log("Starting");

        OnStart();

        if (!double.IsPositiveInfinity(DeltaUpdate))
        {
            updateTask?.Dispose();
            updateTask = new TimedTask(OnUpdate, DeltaUpdate);
            updateTask.Start();
        }

        Terminal.Log("Started");
    }

    protected virtual void OnStart() { }

    protected virtual void OnUpdate() { }

    internal void stop()
    {
        if (!Enabled.Value) return;

        Terminal.Log("Stopping");

        updateTask?.Stop();
        OnStop();

        Terminal.Log("Stopped");
    }

    protected virtual void OnStop() { }

    protected virtual void OnAvatarChange() { }

    #endregion

    #region Settings

    public T GetSetting<T>(Enum lookup) => GetSetting<T>(lookup.ToString().ToLower());

    public T GetSetting<T>(string lookup) => (T)Settings[lookup].Attribute.Value;

    #endregion

    #region IncomingParameters

    private void onParameterReceived(string address, object value)
    {
        if (!Enabled.Value || !IsRequestingInput) return;

        var addressEndpoint = address.Split('/').Last();

        if (addressEndpoint.Equals("change"))
        {
            OnAvatarChange();
            return;
        }

        Enum? key = InputParameters.Keys.ToList().Find(e => e.ToString().Equals(addressEndpoint));
        if (key == null) return;

        var inputParameterData = InputParameters[key];

        if (value is OscTrue) value = true;
        if (value is OscFalse) value = false;

        if (value.GetType() != inputParameterData.Type)
        {
            Terminal.Log($@"Cannot accept input parameter. `{key}` expects type `{inputParameterData.Type}` but received type `{value.GetType()}`");
            return;
        }

        notifyParameterReceived(key, value, inputParameterData.ActionMenu);
    }

    private void notifyParameterReceived(Enum key, object value, ActionMenu actionMenu)
    {
        switch (value)
        {
            case bool boolValue:
                if (actionMenu == ActionMenu.Button && !boolValue) return;

                Terminal.Log($"Received bool of key `{key}`");
                OnBoolParameterReceived(key, boolValue);
                break;

            case int intValue:
                Terminal.Log($"Received int of key `{key}`");
                OnIntParameterReceived(key, intValue);
                break;

            case float floatValue:
                Terminal.Log($"Received float of key `{key}`");
                OnFloatParameterReceived(key, floatValue);
                break;
        }
    }

    protected virtual void OnBoolParameterReceived(Enum key, bool value) { }

    protected virtual void OnIntParameterReceived(Enum key, int value) { }

    protected virtual void OnFloatParameterReceived(Enum key, float value) { }

    #endregion

    #region OutgoingParameters

    public string GetOutputParameter(Enum lookup) => GetOutputParameter(lookup.ToString().ToLower());

    public string GetOutputParameter(string lookup) => (string)OutputParameters[lookup].Attribute.Value;

    protected void SendParameter(Enum lookup, int value) => OscClient.SendData(GetOutputParameter(lookup), value);

    protected void SendParameter(Enum lookup, float value) => OscClient.SendData(GetOutputParameter(lookup), value);

    protected void SendParameter(Enum lookup, bool value) => OscClient.SendData(GetOutputParameter(lookup), value);

    #endregion

    #region Loading

    public void PerformLoad()
    {
        using (var stream = Storage.GetStream(FileName))
        {
            if (stream != null)
            {
                using (var reader = new StreamReader(stream))
                {
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
        }

        executeAfterLoad();
    }

    private void performInternalSettingsLoad(StreamReader reader)
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

    private void performSettingsLoad(StreamReader reader)
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

            if (!Enum.TryParse(typeStr, true, out TypeCode type)) continue;

            if (Type.GetTypeCode(Settings[lookup].Attribute.Value.GetType()) != type) return;

            switch (type)
            {
                case TypeCode.String:
                    Settings[lookup].Attribute.Value = value;
                    break;

                case TypeCode.Int32:
                    Settings[lookup].Attribute.Value = int.Parse(value);
                    break;

                case TypeCode.Boolean:
                    Settings[lookup].Attribute.Value = bool.Parse(value);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private void performOutputParametersLoad(StreamReader reader)
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

    private void performInternalSettingsSave(StreamWriter writer)
    {
        writer.WriteLine(@"#InternalSettings");
        writer.WriteLine(@"{0}={1}", "enabled", Enabled.Value.ToString());
        writer.WriteLine(@"#End");
    }

    private void performSettingsSave(StreamWriter writer)
    {
        var areAllDefault = Settings.All(pair => pair.Value.Attribute.IsDefault);
        if (areAllDefault) return;

        writer.WriteLine(@"#Settings");

        foreach (var (lookup, moduleAttributeData) in Settings)
        {
            if (moduleAttributeData.Attribute.IsDefault) continue;

            var value = moduleAttributeData.Attribute.Value;
            var type = value.GetType().Name.ToLower();

            writer.WriteLine(@"{0}:{1}={2}", lookup, type, value);
        }

        writer.WriteLine(@"#End");
    }

    private void performOutputParametersSave(StreamWriter writer)
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
