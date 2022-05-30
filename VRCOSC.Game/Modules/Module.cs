// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using CoreOSC;
using CoreOSC.IO;
using osu.Framework.Graphics;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Modules;

public abstract class Module
{
    public virtual string Title => string.Empty;
    public virtual string Description => string.Empty;
    public virtual string Author => string.Empty;
    public virtual double DeltaUpdate => double.PositiveInfinity;
    public virtual Colour4 Colour => Colour4.Black;
    public virtual ModuleType Type => ModuleType.General;
    public virtual IReadOnlyCollection<Enum> InputParameters => new List<Enum>();

    internal ModuleDataManager DataManager { get; set; }
    internal UdpClient OscClient { get; set; }

    public bool IsRequestingInput => InputParameters.Count != 0;

    protected TerminalLogger Terminal { get; private set; } = null!;
    public bool Running { get; set; }

    #region Module Functions

    public virtual void CreateAttributes() { }

    internal void Start()
    {
        Terminal = new TerminalLogger(GetType().Name);
        Terminal.Log("Starting");
        OnStart();
        Running = true;
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
        Running = false;
        OnStop();
    }

    protected virtual void OnStop() { }

    #endregion

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
    }

    protected virtual void OnParameterReceived(Enum key, object value) { }

    #region Module Settings

    protected void CreateSetting(Enum key, string displayName, string description, string defaultValue)
    {
        var setting = new StringModuleSetting
        {
            DisplayName = displayName,
            Description = description,
            Value = defaultValue
        };
        DataManager.SetSetting(key, setting);
    }

    protected void CreateSetting(Enum key, string displayName, string description, int defaultValue)
    {
        var setting = new IntModuleSetting
        {
            DisplayName = displayName,
            Description = description,
            Value = defaultValue
        };
        DataManager.SetSetting(key, setting);
    }

    protected void CreateSetting(Enum key, string displayName, string description, bool defaultValue)
    {
        var setting = new BoolModuleSetting
        {
            DisplayName = displayName,
            Description = description,
            Value = defaultValue
        };
        DataManager.SetSetting(key, setting);
    }

    protected void CreateSetting<T>(Enum key, string displayName, string description, T defaultValue) where T : Enum
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

    #endregion

    #region Module Parameters

    protected void CreateParameter(Enum key, string displayName, string description, string defaultAddress)
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

    #endregion

    #region Module Utilities

    protected int[] ToDigitArray(int num, int totalWidth)
    {
        var numStr = num.ToString().PadLeft(totalWidth, '0');
        return numStr.Select(digit => int.Parse(digit.ToString())).ToArray();
    }

    protected float MapBetween(float value, float sMin, float sMax, float dMin, float dMax)
    {
        return value / (sMax - sMin) * (dMax - dMin) + dMin;
    }

    #endregion
}
