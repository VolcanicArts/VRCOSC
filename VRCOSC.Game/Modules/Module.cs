// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using CoreOSC;
using CoreOSC.IO;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Modules;

public abstract class Module
{
    internal ModuleDataManager DataManager { get; set; } = null!;
    internal UdpClient OscClient { get; set; } = null!;

    protected TerminalLogger Terminal { get; private set; } = null!;

    public virtual string Title => string.Empty;
    public virtual string Description => string.Empty;
    public virtual string Author => string.Empty;
    public virtual double DeltaUpdate => double.PositiveInfinity;
    public virtual Colour4 Colour => Colour4.Black;
    public virtual ModuleType Type => ModuleType.General;
    public virtual bool Experimental => false;

    // These are *only* definitions. All reads and writes for persistent data occur inside the DataManager
    protected virtual IReadOnlyDictionary<Enum, (string, string, object)> Settings => new Dictionary<Enum, (string, string, object)>();
    protected virtual IReadOnlyDictionary<Enum, (string, string, string)> OutputParameters => new Dictionary<Enum, (string, string, string)>();
    protected virtual IReadOnlyCollection<Enum> InputParameters => new List<Enum>();

    public bool IsRequestingInput => InputParameters.Count != 0;

    public bool HasSettings => DataManager.Settings.Count != 0;
    public bool HasParameters => DataManager.Parameters.Count != 0;

    public bool HasAttributes => HasSettings || HasParameters;

    internal void CreateAttributes()
    {
        Settings.ForEach(pair =>
        {
            var key = pair.Key;
            var displayName = pair.Value.Item1;
            var description = pair.Value.Item2;
            var value = pair.Value.Item3;

            createSetting(key, displayName, description, value);
        });

        OutputParameters.ForEach(pair =>
        {
            var key = pair.Key;
            var displayName = pair.Value.Item1;
            var description = pair.Value.Item2;
            var address = pair.Value.Item3;

            createParameter(key, displayName, description, address);
        });
    }

    internal void Start()
    {
        Terminal = new TerminalLogger(GetType().Name);
        Terminal.Log("Starting");
        OnStart();
    }

    protected virtual void OnStart() { }

    internal void Update()
    {
        OnUpdate();
    }

    protected virtual void OnUpdate() { }

    internal void Stop()
    {
        Terminal.Log("Stopping");
        OnStop();
    }

    protected virtual void OnStop() { }

    internal void OnOSCMessage(OscMessage message)
    {
        var address = message.Address.Value;
        var value = message.Arguments.First();

        var id = -1;

        for (var i = 0; i < InputParameters.Count; i++)
        {
            var inputParameterAddress = $"/avatar/parameters/{InputParameters.ElementAt(i)}";
            if (address.Equals(inputParameterAddress)) id = i;
        }

        if (id == -1) return;

        if (value is OscTrue) value = true;
        if (value is OscFalse) value = false;

        var key = InputParameters.ElementAt(id);

        OnParameterReceived(key, value);

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
    }

    protected virtual void OnParameterReceived(Enum key, object value) { }

    protected virtual void OnBoolParameterReceived(Enum key, bool value) { }

    protected virtual void OnIntParameterReceived(Enum key, int value) { }

    protected virtual void OnFloatParameterReceived(Enum key, float value) { }

    private void createSetting(Enum key, string displayName, string description, object defaultValue)
    {
        switch (defaultValue)
        {
            case string stringValue:
                createSetting(key, displayName, description, stringValue);
                break;

            case bool boolValue:
                createSetting(key, displayName, description, boolValue);
                break;

            case int intValue:
                createSetting(key, displayName, description, intValue);
                break;

            case Enum enumValue:
                createSetting(key, displayName, description, enumValue);
                break;

            case long longValue:
                createSetting(key, displayName, description, (int)longValue);
                break;

            case float floatValue:
                createSetting(key, displayName, description, (int)floatValue);
                break;

            case double doubleValue:
                createSetting(key, displayName, description, (int)doubleValue);
                break;
        }
    }

    private void createSetting(Enum key, string displayName, string description, string defaultValue)
    {
        var setting = new StringModuleSetting
        {
            DisplayName = displayName,
            Description = description,
            Value = defaultValue
        };
        DataManager.SetSetting(key, setting);
    }

    private void createSetting(Enum key, string displayName, string description, int defaultValue)
    {
        var setting = new IntModuleSetting
        {
            DisplayName = displayName,
            Description = description,
            Value = defaultValue
        };
        DataManager.SetSetting(key, setting);
    }

    private void createSetting(Enum key, string displayName, string description, bool defaultValue)
    {
        var setting = new BoolModuleSetting
        {
            DisplayName = displayName,
            Description = description,
            Value = defaultValue
        };
        DataManager.SetSetting(key, setting);
    }

    private void createSetting<T>(Enum key, string displayName, string description, T defaultValue) where T : Enum
    {
        var setting = new EnumModuleSetting
        {
            DisplayName = displayName,
            Description = description,
            Value = defaultValue
        };
        DataManager.SetSetting(key, setting);
    }

    protected T GetSettingAs<T>(Enum key)
    {
        return DataManager.GetSettingAs<T>(key.ToString().ToLower());
    }

    private void createParameter(Enum key, string displayName, string description, string defaultAddress)
    {
        var parameter = new ModuleParameter
        {
            DisplayName = displayName,
            Description = description,
            Value = defaultAddress
        };
        DataManager.SetParameter(key, parameter);
    }

    protected void SendParameter(Enum key, bool value)
    {
        SendParameter(key, value ? OscTrue.True : OscFalse.False);
    }

    protected void SendParameter(Enum key, object value)
    {
        var address = new Address(DataManager.GetParameter(key));
        var message = new OscMessage(address, new[] { value });
        OscClient.SendMessageAsync(message);
    }

    protected int[] ToDigitArray(int num, int totalWidth)
    {
        var numStr = num.ToString().PadLeft(totalWidth, '0');
        return numStr.Select(digit => int.Parse(digit.ToString())).ToArray();
    }

    protected float MapBetween(float value, float sMin, float sMax, float dMin, float dMax)
    {
        return value / (sMax - sMin) * (dMax - dMin) + dMin;
    }
}
