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
    protected virtual Dictionary<Enum, (string, string, object)> Settings => new();
    protected virtual Dictionary<Enum, (string, string, string)> OutputParameters => new();
    protected virtual List<Enum> InputParameters => new();

    private readonly Dictionary<string, object> defaultSettings = new();
    private readonly Dictionary<string, string> defaultOutputParameters = new();

    public Action<string, object> OnParameterSent;
    public Action<string, object> OnParameterReceived;

    public bool IsRequestingInput => InputParameters.Count != 0;

    public bool HasSettings => DataManager.Settings.Count != 0;
    public bool HasParameters => DataManager.Parameters.Count != 0;

    public bool HasAttributes => HasSettings || HasParameters;

    public T GetDefaultSetting<T>(string key)
    {
        return (T)defaultSettings[key];
    }

    public string GetDefaultOutputParameter(string key)
    {
        return defaultOutputParameters[key];
    }

    internal void CreateAttributes()
    {
        Settings.ForEach(pair =>
        {
            var key = pair.Key;
            var displayName = pair.Value.Item1;
            var description = pair.Value.Item2;
            var value = pair.Value.Item3;

            DataManager.CreateSetting(key, displayName, description, value);
            defaultSettings.Add(key.ToString().ToLower(), value);
        });

        OutputParameters.ForEach(pair =>
        {
            var key = pair.Key;
            var displayName = pair.Value.Item1;
            var description = pair.Value.Item2;
            var address = pair.Value.Item3;

            DataManager.CreateParameter(key, displayName, description, address);
            defaultOutputParameters.Add(key.ToString().ToLower(), address);
        });
    }

    internal void Start()
    {
        Terminal = new TerminalLogger(GetType().Name);
        Terminal.Log("Starting");
        OnStart();
        Terminal.Log("Started");
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
        Terminal.Log("Stopped");
    }

    protected virtual void OnStop() { }

    internal void OnOSCMessage(OscMessage message)
    {
        var address = message.Address.Value;

        if (!message.Arguments.Any()) return;

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

        OnParameterReceived?.Invoke(address, value);

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

    protected virtual void OnBoolParameterReceived(Enum key, bool value) { }

    protected virtual void OnIntParameterReceived(Enum key, int value) { }

    protected virtual void OnFloatParameterReceived(Enum key, float value) { }

    protected T GetSettingAs<T>(Enum key)
    {
        return DataManager.GetSettingAs<T>(key);
    }

    protected void SendParameter(Enum key, int value) => sendParameter(key, value);

    protected void SendParameter(Enum key, float value) => sendParameter(key, value);

    protected void SendParameter(Enum key, bool value) => sendParameter(key, value ? OscTrue.True : OscFalse.False);

    private void sendParameter(Enum key, object value)
    {
        var address = new Address(DataManager.GetParameter(key));
        var message = new OscMessage(address, new[] { value });
        OscClient.SendMessageAsync(message);
        OnParameterSent?.Invoke(DataManager.GetParameter(key), value);
    }

    protected static int[] ToDigitArray(int num, int totalWidth)
    {
        var numStr = num.ToString().PadLeft(totalWidth, '0');
        return numStr.Select(digit => int.Parse(digit.ToString())).ToArray();
    }

    protected static float MapBetween(float value, float sMin, float sMax, float dMin, float dMax)
    {
        return value / (sMax - sMin) * (dMax - dMin) + dMin;
    }
}
