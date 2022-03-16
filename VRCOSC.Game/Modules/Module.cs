// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using CoreOSC;
using CoreOSC.IO;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using VRCOSC.Game.Util;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

namespace VRCOSC.Game.Modules;

public abstract class Module
{
    public virtual string Title => string.Empty;
    public virtual string Description => string.Empty;
    public virtual string Author => string.Empty;
    public virtual Colour4 Colour => Colour4.Black;
    public virtual ModuleType Type => ModuleType.General;

    /// <summary>
    /// The time between updates in milliseconds
    /// </summary>
    public virtual double DeltaUpdate => double.PositiveInfinity;

    public ModuleMetadata Metadata = new();
    public ModuleData Data = new();
    public Bindable<bool> Enabled = new(true);

    protected UdpClient OscClient = new(IPAddress.Loopback.ToString(), 9000);
    protected TerminalLogger? Terminal;

    private readonly Storage ModuleStorage;

    protected Module(Storage storage)
    {
        ModuleStorage = storage.GetStorageForDirectory("modules");
    }

    public virtual void Start()
    {
        Terminal = new TerminalLogger(GetType().Name);
        Terminal.Log("Starting");
    }

    public virtual void Update() { }

    public virtual void Stop()
    {
        Terminal!.Log("Stopping");
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

        ModuleStorage.Delete(fileName);

        using var fileStream = ModuleStorage.GetStream(fileName, FileAccess.ReadWrite);
        using var streamWriter = new StreamWriter(fileStream);

        var serialisedData = JsonConvert.SerializeObject(Data);
        streamWriter.WriteLine(serialisedData);
    }

    protected void LoadData()
    {
        var fileName = $"{GetType().Name}.conf";

        using (var fileStream = ModuleStorage.GetStream(fileName, FileAccess.ReadWrite))
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
        });

        saveData();
    }

    protected T GetSettingValue<T>(string key)
    {
        return (T)Data.Settings[key];
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
