// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Serialisation;

/// <summary>
/// Allows for serialising TReference into TSerialisable and deserialising TSerialisable to pass to <see cref="ExecuteAfterDeserialisation"/>
/// </summary>
/// <typeparam name="TReference"></typeparam>
/// <typeparam name="TSerialisable"></typeparam>
public abstract class Serialiser<TReference, TSerialisable> : ISerialiser where TSerialisable : class
{
    private readonly object serialisationLock = new();

    protected readonly TReference Reference;

    protected virtual string Directory => string.Empty;
    protected virtual string FileName => string.Empty;
    protected virtual Formatting Format => Formatting.Indented;

    private readonly Storage baseStorage;

    protected Serialiser(Storage storage, TReference reference)
    {
        baseStorage = storage;
        Reference = reference;
    }

    public string FullPath => baseStorage.GetStorageForDirectory(Directory).GetFullPath(FileName);
    public bool DoesFileExist() => baseStorage.GetStorageForDirectory(Directory).Exists(FileName);

    public bool TryGetVersion([NotNullWhen(true)] out int? version, string filePathOverride = "")
    {
        if (string.IsNullOrEmpty(filePathOverride) && !DoesFileExist())
        {
            version = null;
            return false;
        }

        var filePath = string.IsNullOrEmpty(filePathOverride) ? FullPath : filePathOverride;

        try
        {
            lock (serialisationLock)
            {
                var data = performDeserialisation<SerialisableVersion>(filePath);

                if (data is null)
                {
                    version = null;
                    return false;
                }

                version = data.Version;
                return true;
            }
        }
        catch
        {
            version = null;
            return false;
        }
    }

    public DeserialisationResult Deserialise(string filePathOverride = "")
    {
        var filePath = string.IsNullOrEmpty(filePathOverride) ? FullPath : filePathOverride;

        try
        {
            lock (serialisationLock)
            {
                var data = performDeserialisation<TSerialisable>(filePath);
                if (data is null) return DeserialisationResult.CorruptFile;

                if (ExecuteAfterDeserialisation(data)) Serialise();

                return DeserialisationResult.Success;
            }
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"{GetType().Name} experienced an issue");
            return DeserialisationResult.GenericError;
        }
    }

    public SerialisationResult Serialise()
    {
        try
        {
            lock (serialisationLock)
            {
                var data = (TSerialisable)Activator.CreateInstance(typeof(TSerialisable), Reference)!;
                var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data, Format));
                using var stream = baseStorage.GetStorageForDirectory(Directory).CreateFileSafely(FileName);
                stream.Write(bytes);
            }

            return SerialisationResult.Success;
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"{GetType().Name} experienced an issue");
            return SerialisationResult.GenericError;
        }
    }

    private T? performDeserialisation<T>(string filePath) where T : class
    {
        try
        {
            var bytes = File.ReadAllBytes(filePath);

            if (bytes is [0xFF, 0xFE, ..])
            {
                bytes = bytes[2..];
                Logger.Log("Found BOM. Deserialising as UTF16");
                return JsonConvert.DeserializeObject<T>(Encoding.Unicode.GetString(bytes));
            }

            var utf16Str = Encoding.Unicode.GetString(bytes);
            var utf8Str = Encoding.UTF8.GetString(bytes);

            if (utf16Str.StartsWith('{'))
            {
                Logger.Log($"Deserialising {filePath} as UTF16", LoggingTarget.Information);
                return JsonConvert.DeserializeObject<T>(utf16Str);
            }

            if (utf8Str.StartsWith('{'))
            {
                Logger.Log($"Deserialising {filePath} as UTF8", LoggingTarget.Information);
                return JsonConvert.DeserializeObject<T>(utf8Str);
            }

            Logger.Log($"'{filePath}' was unable to be deserialised");
            return null;
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"'{filePath}' was unable to be deserialised");
            return null;
        }
    }

    /// <summary>
    /// Executed after the deserialisation is complete
    /// </summary>
    /// <param name="data">The data that has been deserialised</param>
    /// <returns>True if the data should be immediately reserialised</returns>
    protected abstract bool ExecuteAfterDeserialisation(TSerialisable data);

    /// <summary>
    /// Attempts to convert the <paramref name="value"/> to the <paramref name="targetType"/>
    /// </summary>
    /// <remarks>This has some special logic to handle different types automatically</remarks>
    protected bool TryConvertToTargetType(object? value, Type targetType, out object? outValue)
    {
        try
        {
            switch (value)
            {
                case null:
                    outValue = null;
                    return true;

                case JToken token:
                    outValue = token.ToObject(targetType)!;
                    return true;

                case string strValue when targetType == typeof(Guid):
                    outValue = Guid.Parse(strValue);
                    return true;

                case var subValue when targetType.IsAssignableTo(typeof(Enum)):
                    outValue = Enum.ToObject(targetType, subValue);
                    return true;

                case long utcTicks when targetType == typeof(DateTimeOffset):
                    var utcDateTime = new DateTime(utcTicks, DateTimeKind.Utc);
                    var localDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, TimeZoneInfo.Local);
                    outValue = new DateTimeOffset(localDateTime, TimeZoneInfo.Local.GetUtcOffset(localDateTime));
                    return true;

                default:
                    outValue = Convert.ChangeType(value, targetType);
                    return true;
            }
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"'{FullPath}' was unable to convert {value!.GetType().GetFriendlyName()} to {targetType.GetFriendlyName()}");
            throw;
        }
    }
}