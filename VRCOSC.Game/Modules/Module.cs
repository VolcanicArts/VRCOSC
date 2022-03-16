// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using System.Net.Sockets;
using CoreOSC;
using CoreOSC.IO;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using VRCOSC.Game.Util;

#pragma warning disable CS8618

// ReSharper disable InconsistentNaming

namespace VRCOSC.Game.Modules;

public abstract class Module
{
    private const string storage_directory = "modules";
    private const string osc_ip_address = "127.0.0.1";
    private const int osc_port = 9000;

    public virtual string Title => string.Empty;
    public virtual string Description => string.Empty;
    public virtual string Author => string.Empty;
    public virtual Colour4 Colour => Colour4.Black;
    public virtual ModuleType Type => ModuleType.General;
    public virtual double DeltaUpdate => double.PositiveInfinity;

    public ModuleMetadata Metadata { get; } = new();
    public ModuleData Data { get; } = new();
    public Bindable<bool> Enabled { get; } = new(true);

    protected TerminalLogger Terminal { get; private set; }

    private readonly UdpClient OscClient;
    private readonly Storage Storage;

    protected Module(Storage storage)
    {
        OscClient = new UdpClient(osc_ip_address, osc_port);
        Storage = storage;
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

    protected void CreateSetting(Enum key, string displayName, string description, object defaultValue)
    {
        var moduleSettingMetadata = new ModuleAttributeMetadata
        {
            DisplayName = displayName,
            Description = description
        };

        Data.Settings.Add(key.ToString().ToLower(), defaultValue);
        Metadata.Settings.Add(key.ToString().ToLower(), moduleSettingMetadata);
    }

    protected void CreateParameter(Enum key, string displayName, string description, string defaultAddress)
    {
        var moduleOscParameterMetadata = new ModuleAttributeMetadata
        {
            DisplayName = displayName,
            Description = description,
        };

        Data.Parameters.Add(key.ToString().ToLower(), defaultAddress);
        Metadata.Parameters.Add(key.ToString().ToLower(), moduleOscParameterMetadata);
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
        var moduleStorage = Storage.GetStorageForDirectory(storage_directory);

        moduleStorage.Delete(fileName);

        using var fileStream = moduleStorage.GetStream(fileName, FileAccess.ReadWrite);
        using var streamWriter = new StreamWriter(fileStream);

        var serialisedData = JsonConvert.SerializeObject(Data);
        streamWriter.WriteLine(serialisedData);
    }

    internal void LoadData()
    {
        var fileName = $"{GetType().Name}.conf";
        var moduleStorage = Storage.GetStorageForDirectory(storage_directory);

        using (var fileStream = moduleStorage.GetStream(fileName, FileAccess.ReadWrite))
        {
            using (var streamReader = new StreamReader(fileStream))
            {
                var deserializedData = JsonConvert.DeserializeObject<ModuleData>(streamReader.ReadToEnd());

                if (deserializedData != null)
                {
                    deserializedData.Settings.ForEach(pair =>
                    {
                        if (Data.Settings.ContainsKey(pair.Key))
                            Data.Settings[pair.Key] = pair.Value;
                    });
                    deserializedData.Parameters.ForEach(pair =>
                    {
                        if (Data.Parameters.ContainsKey(pair.Key))
                            Data.Parameters[pair.Key] = pair.Value;
                    });
                    Enabled.Value = deserializedData.Enabled;
                }
            }
        }

        Enabled.BindValueChanged(e =>
        {
            Data.Enabled = e.NewValue;
            saveData();
        }, true);
    }

    protected T GetSettingAs<T>(Enum key)
    {
        return (T)Data.Settings[key.ToString().ToLower()];
    }

    protected void SendParameter(Enum key, bool value)
    {
        SendParameter(key, value ? OscTrue.True : OscFalse.False);
    }

    protected void SendParameter(Enum key, object value)
    {
        var address = new Address(Data.Parameters[key.ToString().ToLower()]);
        var message = new OscMessage(address, new[] { value });
        OscClient.SendMessageAsync(message);
    }
}
