// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using osu.Framework.Logging;
using osu.Framework.Platform;
using VRCOSC.Game.Graphics.Notifications;

namespace VRCOSC.Game.Serialisation;

public abstract class Serialiser<TReference, TReturn> : ISerialiser<TReturn> where TReturn : class
{
    private readonly object serialisationLock = new();
    private readonly Storage storage;
    private readonly NotificationContainer notification;
    private readonly TReference reference;

    protected virtual string FileName => throw new NotImplementedException($"{typeof(Serialiser<TReference, TReturn>)} requires a file name");

    protected Serialiser(Storage storage, NotificationContainer notification, TReference reference)
    {
        this.storage = storage;
        this.notification = notification;
        this.reference = reference;
    }

    public TReturn? Deserialise()
    {
        Logger.Log($"Performing load for file {FileName}");

        if (!storage.Exists(FileName))
        {
            Logger.Log($"File {FileName} does not exist. Creating...");
            Serialise();
        }

        try
        {
            lock (serialisationLock)
            {
                return performDeserialisation();
            }
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Load failed for file {FileName}");
            notification.Notify(new ExceptionNotification($"Could not load file {FileName}. Report on the Discord server"));
            return null;
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

    private TReturn? performDeserialisation()
    {
        try
        {
            return JsonConvert.DeserializeObject<TReturn>(Encoding.Unicode.GetString(File.ReadAllBytes(storage.GetFullPath(FileName))));
        }
        catch // migration from UTF-8
        {
            Logger.Log("UTF-8 possibly detected. Attempting conversion from UTF-8 to Unicode");
            return JsonConvert.DeserializeObject<TReturn>(File.ReadAllText(storage.GetFullPath(FileName)));
        }
    }

    private void performSerialisation()
    {
        var data = GetSerialisableData(reference);

        var bytes = Encoding.Unicode.GetBytes(JsonConvert.SerializeObject(data, Formatting.Indented));
        using var stream = storage.CreateFileSafely(FileName);
        stream.Write(bytes);
    }

    protected abstract object GetSerialisableData(TReference reference);
}
