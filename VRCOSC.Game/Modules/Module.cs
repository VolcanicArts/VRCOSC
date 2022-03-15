using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using CoreOSC;
using CoreOSC.IO;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Modules;

public abstract class Module
{
    public virtual string Title => "Unknown";
    public virtual string Description => "Unknown description";
    public virtual Colour4 Colour => Colour4.Gray;
    public Dictionary<string, ModuleSetting> Settings { get; } = new();
    public Dictionary<string, ModuleOscParameter> Parameters { get; } = new();

    protected UdpClient OscClient = new(IPAddress.Loopback.ToString(), 9000);

    protected TerminalLogger terminal;
    public BindableBool Enabled = new(true);

    public virtual void Start()
    {
        terminal = new TerminalLogger(GetType().Name);
        terminal.Log($"Starting {GetType().Name}");
    }

    /// <summary>
    /// Called 5 times per second
    /// </summary>
    public virtual void Update() { }

    public virtual void Stop()
    {
        terminal.Log($"Stopping");
    }

    protected void CreateSetting(string key, string displayName, string description, object defaultValue)
    {
        var moduleSetting = new ModuleSetting
        {
            Key = key,
            DisplayName = displayName,
            Description = description,
            Value = defaultValue,
            Type = defaultValue.GetType()
        };
        Settings.Add(key, moduleSetting);
    }

    protected void CreateParameter(string key, string displayName, string description, string defaultAddress)
    {
        var moduleOscParameter = new ModuleOscParameter
        {
            Key = key,
            DisplayName = displayName,
            Description = description,
            Address = defaultAddress
        };
        Parameters.Add(key, moduleOscParameter);
    }

    protected T GetSettingValue<T>(string key)
    {
        return (T)Settings[key].Value;
    }

    protected string GetParameterAddress(string key)
    {
        return Parameters[key].Address;
    }

    protected void SendParameter(string key, bool value)
    {
        SendParameter(key, value ? OscTrue.True : OscFalse.False);
    }

    protected void SendParameter(string key, object value)
    {
        var address = new Address(Parameters[key].Address);
        var message = new OscMessage(address, new[] { value });
        OscClient.SendMessageAsync(message);
    }
}
