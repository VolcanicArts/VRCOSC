// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Net.Sockets;
using CoreOSC;
using CoreOSC.IO;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Modules;

public abstract class Module
{
    private const string osc_ip_address = "127.0.0.1";
    private const int osc_port = 9000;

    public virtual string Title => string.Empty;
    public virtual string Description => string.Empty;
    public virtual string Author => string.Empty;
    public virtual double DeltaUpdate => double.PositiveInfinity;
    public virtual Colour4 Colour => Colour4.Black;
    public virtual ModuleType Type => ModuleType.General;

    public readonly ModuleDataManager DataManager;
    private readonly UdpClient oscClient;

    protected TerminalLogger Terminal { get; private set; } = null!;

    protected Module(Storage storage)
    {
        DataManager = new ModuleDataManager(storage, GetType().Name);
        oscClient = new UdpClient(osc_ip_address, osc_port);
    }

    #region Module Functions

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

    #endregion

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
        var setting = new EnumModuleSetting()
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
        oscClient.SendMessageAsync(message);
    }

    #endregion
}
