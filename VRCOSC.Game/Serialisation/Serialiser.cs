// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using osu.Framework.Logging;
using osu.Framework.Platform;
using VRCOSC.Game.Graphics.Notifications;

namespace VRCOSC.Game.Serialisation;

public abstract class Serialiser<TReference, TSerialisable> : ISerialiser where TSerialisable : class
{
    private readonly object serialisationLock = new();
    private readonly NotificationContainer notification;
    private readonly TReference reference;
    private Storage storage;

    protected virtual string Directory => string.Empty;
    protected virtual string FileName => throw new NotImplementedException($"{typeof(Serialiser<TReference, TSerialisable>)} requires a file name");

    protected Serialiser(Storage storage, NotificationContainer notification, TReference reference)
    {
        this.storage = storage;
        this.notification = notification;
        this.reference = reference;
    }

    public void Initialise()
    {
        if (!string.IsNullOrEmpty(Directory)) storage = storage.GetStorageForDirectory(Directory);
    }

    public bool DoesFileExist() => storage.Exists(FileName);

    public bool TryGetVersion([NotNullWhen(true)] out int? version)
    {
        if (!DoesFileExist())
        {
            version = null;
            return false;
        }

        try
        {
            var data = performDeserialisation<SerialisableVersion>(storage.GetFullPath(FileName));

            if (data is null)
            {
                version = null;
                return false;
            }

            version = data.Version;
            return true;
        }
        catch
        {
            version = null;
            return false;
        }
    }

    public bool Deserialise(string filePathOverride = "")
    {
        var filePath = string.IsNullOrEmpty(filePathOverride) ? storage.GetFullPath(FileName) : filePathOverride;

        Logger.Log($"Performing load for file {FileName}");

        try
        {
            lock (serialisationLock)
            {
                var data = performDeserialisation<TSerialisable>(filePath);
                if (data is null) return false;

                ExecuteAfterDeserialisation(reference, data);
                return true;
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Load failed for file {FileName}");
            notification.Notify(new ExceptionNotification($"Could not load file {FileName}. Report on the Discord server"));
            return false;
        }
    }

    public bool Serialise()
    {
        Logger.Log($"Performing save for file {FileName}");

        try
        {
            lock (serialisationLock)
            {
                performSerialisation();
            }

            return true;
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Save failed for file {FileName}");
            notification.Notify(new ExceptionNotification($"Could not save file {FileName}. Report on the Discord server"));
            return false;
        }
    }

    private T? performDeserialisation<T>(string filePath)
    {
        try
        {
            return JsonConvert.DeserializeObject<T>(Encoding.Unicode.GetString(File.ReadAllBytes(filePath)));
        }
        catch // migration from UTF-8
        {
            Logger.Log("UTF-8 possibly detected. Attempting conversion from UTF-8 to Unicode");
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath));
        }
    }

    private void performSerialisation()
    {
        var data = GetSerialisableData(reference);

        var bytes = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(data, Formatting.Indented));
        using var stream = storage.CreateFileSafely(FileName);
        stream.Write(bytes);
    }

    protected abstract TSerialisable GetSerialisableData(TReference reference);
    protected abstract void ExecuteAfterDeserialisation(TReference reference, TSerialisable data);
}
