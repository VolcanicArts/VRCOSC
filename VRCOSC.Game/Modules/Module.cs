// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using CoreOSC;
using CoreOSC.IO;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using VRCOSC.Game.Util;

// ReSharper disable InconsistentNaming

namespace VRCOSC.Game.Modules;

public abstract class Module
{
    private Storage Storage = null!;
    private UdpClient OscClient = null!;

    protected TerminalLogger Terminal = null!;

    public Action<string, object>? OnParameterSent;
    public Action<string, object>? OnParameterReceived;

    public BindableBool Enabled = new();

    public readonly Dictionary<string, ModuleAttributeData> Settings = new();
    public readonly Dictionary<string, ModuleAttributeData> OutputParameters = new();

    public readonly Dictionary<Enum, Type> InputParameters = new();

    public void Initialise(Storage storage, UdpClient oscClient)
    {
        Storage = storage;
        OscClient = oscClient;
        Terminal = new TerminalLogger(GetType().Name);

        Enabled.BindValueChanged((_) => PerformSave());
    }

    #region Properties

    public bool IsRequestingInput => InputParameters.Count != 0;

    public bool HasSettings => Settings.Count != 0;
    public bool HasOutputParameters => OutputParameters.Count != 0;
    public bool HasAttributes => HasSettings || HasOutputParameters;

    private string FileName => $"{GetType().Name}.ini";

    #endregion

    #region Metadata

    /// <summary>
    /// The title of the module
    /// </summary>
    public virtual string Title => string.Empty;

    /// <summary>
    /// The description of the module.
    /// This is what's displayed on the GUI, so it should be short
    /// </summary>
    public virtual string Description => string.Empty;

    /// <summary>
    /// The author of the module. Usually your GitHub username
    /// </summary>
    public virtual string Author => string.Empty;

    /// <summary>
    /// The tags of the module. Used for searching along side the title
    /// </summary>
    public virtual string[] Tags => Array.Empty<string>();

    /// <summary>
    /// The time in milliseconds between each update call
    /// </summary>
    public virtual double DeltaUpdate => double.MaxValue;

    /// <summary>
    /// If the module has been untested or unverified
    /// </summary>
    public virtual bool Experimental => false;

    /// <summary>
    /// The colour of the module.
    /// Used in the GUI for displaying
    /// </summary>
    public virtual Colour4 Colour => Colour4.Black;

    /// <summary>
    /// The type of the module.
    /// Used for grouping in the GUI
    /// </summary>
    public virtual ModuleType Type => ModuleType.General;

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

    protected void RegisterInputParameter(Enum lookup, Type expectedType)
    {
        InputParameters.Add(lookup, expectedType);
    }

    #endregion

    #region Events

    public virtual void Start() { }

    public virtual void Update() { }

    public virtual void Stop() { }

    #endregion

    #region Settings

    public T GetSetting<T>(Enum lookup) => GetSetting<T>(lookup.ToString().ToLower());

    public T GetSetting<T>(string lookup) => (T)Settings[lookup].Value;

    public T GetSettingDefault<T>(Enum lookup) => GetSettingDefault<T>(lookup.ToString().ToLower());

    public T GetSettingDefault<T>(string lookup) => (T)Settings[lookup].DefaultValue;

    #endregion

    #region Parameters

    public string GetOutputParameter(Enum lookup) => GetOutputParameter(lookup.ToString().ToLower());

    public string GetOutputParameter(string lookup) => (string)OutputParameters[lookup].Value;

    public string GetOutputParameterDefault(Enum lookup) => GetOutputParameterDefault(lookup.ToString().ToLower());

    public string GetOutputParameterDefault(string lookup) => (string)OutputParameters[lookup].DefaultValue;

    #endregion

    #region IncomingParameters

    public void OnOSCMessage(OscMessage message)
    {
        if (!message.Arguments.Any()) return;

        var addressEndpoint = message.Address.Value.Split('/').Last();
        var value = message.Arguments.First();

        Enum? key = InputParameters.Keys.ToList().Find(e => e.ToString().Equals(addressEndpoint));
        if (key == null) return;

        var type = InputParameters[key];

        if (value is OscTrue) value = true;
        if (value is OscFalse) value = false;

        if (value.GetType() != type)
            throw new ArgumentException($"{key} expects type {type} but received type {value.GetType()}");

        notifyParameterReceived(key, value);
    }

    private void notifyParameterReceived(Enum key, object value)
    {
        switch (value)
        {
            case bool boolValue:
                OnBoolParameterReceived(key, boolValue);
                break;

            case int intValue:
                OnIntParameterReceived(key, intValue);
                break;

            case float floatValue:
                OnFloatParameterReceived(key, floatValue);
                break;
        }

        OnParameterReceived?.Invoke($"/avatar/parameters/{key}", value);
    }

    protected virtual void OnBoolParameterReceived(Enum key, bool value) { }

    protected virtual void OnIntParameterReceived(Enum key, int value) { }

    protected virtual void OnFloatParameterReceived(Enum key, float value) { }

    #endregion

    #region OutgoingParameters

    protected void SendParameter(Enum key, int value) => sendParameter(key, value);

    protected void SendParameter(Enum key, float value) => sendParameter(key, value);

    protected void SendParameter(Enum key, bool value) => sendParameter(key, value ? OscTrue.True : OscFalse.False);

    private void sendParameter(Enum key, object value) => sendParameter(key.ToString().ToLower(), value);

    private void sendParameter(string key, object value)
    {
        var addressString = OutputParameters[key].Value.ToString()!;

        var address = new Address(addressString);
        var message = new OscMessage(address, new[] { value });
        OscClient.SendMessageAsync(message);
        OnParameterSent?.Invoke(addressString, value);
    }

    #endregion

    #region Loading

    public void PerformLoad()
    {
        PerformSave();
    }

    #endregion

    #region Saving

    public void PerformSave()
    {
        using var stream = Storage.CreateFileSafely(FileName);
        using var w = new StreamWriter(stream);

        w.WriteLine(@"{0}={1}", "enabled", Enabled.Value.ToString());

        foreach (var (lookup, moduleAttributeData) in Settings)
        {
            var type = moduleAttributeData.Value.GetType().Name.ToLower();
            var value = moduleAttributeData.Value;

            w.WriteLine(@"{0}:{1}={2}", type, lookup, value);
        }
    }

    #endregion
}
