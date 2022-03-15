using System.IO;
using System.Net;
using System.Net.Sockets;
using CoreOSC;
using CoreOSC.IO;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using VRCOSC.Game.Util;

// ReSharper disable MemberCanBePrivate.Global

namespace VRCOSC.Game.Modules;

public abstract class Module
{
    public virtual string Title => string.Empty;
    public virtual string Description => string.Empty;
    public virtual Colour4 Colour => Colour4.Black;

    public ModuleMetadata Metadata = new();
    public ModuleData Data = new();
    public BindableBool Enabled = new(true);

    protected UdpClient OscClient = new(IPAddress.Loopback.ToString(), 9000);
    protected TerminalLogger terminal;

    private Storage ModuleStorage;

    protected Module(Storage storage)
    {
        ModuleStorage = storage.GetStorageForDirectory("modules");
    }

    public virtual void Start()
    {
        terminal = new TerminalLogger(GetType().Name);
        terminal.Log("Starting");
    }

    /// <summary>
    /// Called 5 times per second
    /// </summary>
    public virtual void Update() { }

    public virtual void Stop()
    {
        terminal.Log("Stopping");
    }

    protected void CreateSetting(string key, string displayName, string description, object defaultValue)
    {
        var moduleSettingMetadata = new ModuleAttributeMetadata
        {
            DisplayName = displayName,
            Description = description
        };

        Data.Settings.Add(key, defaultValue);
        Metadata.Settings.Add(key, moduleSettingMetadata);
    }

    protected void CreateParameter(string key, string displayName, string description, string defaultAddress)
    {
        var moduleOscParameterMetadata = new ModuleAttributeMetadata
        {
            DisplayName = displayName,
            Description = description,
        };

        Data.Parameters.Add(key, defaultAddress);
        Metadata.Parameters.Add(key, moduleOscParameterMetadata);
    }

    public void UpdateSetting(string key, object value)
    {
        Data.Settings[key] = value;
        saveData();
    }

    public void UpdateParameter(string key, string address)
    {
        Data.Parameters[key] = address;
        saveData();
    }

    private void saveData()
    {
        var fileName = $"{GetType().Name}.conf";

        ModuleStorage.Delete(fileName);

        using var fileStream = ModuleStorage.GetStream(fileName, FileAccess.ReadWrite);
        using var streamWriter = new StreamWriter(fileStream);

        var serialisedData = JsonConvert.SerializeObject(Data);
        streamWriter.WriteLine(serialisedData);
    }

    protected void LoadData()
    {
        var fileName = $"{GetType().Name}.conf";

        using var fileStream = ModuleStorage.GetStream(fileName, FileAccess.ReadWrite);
        using var streamReader = new StreamReader(fileStream);

        var deserializedData = JsonConvert.DeserializeObject<ModuleData>(streamReader.ReadToEnd());
        if (deserializedData != null) Data = deserializedData;
    }

    protected T GetSettingValue<T>(string key)
    {
        return (T)Data.Settings[key];
    }

    protected void SendParameter(string key, bool value)
    {
        SendParameter(key, value ? OscTrue.True : OscFalse.False);
    }

    protected void SendParameter(string key, object value)
    {
        var address = new Address(Data.Parameters[key]);
        var message = new OscMessage(address, new[] { value });
        OscClient.SendMessageAsync(message);
    }
}
