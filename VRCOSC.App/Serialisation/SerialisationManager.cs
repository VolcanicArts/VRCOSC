// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VRCOSC.App.Serialisation;

/// <summary>
/// Manages multiple <see cref="ISerialiser"/>s and handles migration between <see cref="ISerialiser"/> versions
/// </summary>
public class SerialisationManager
{
    private int latestSerialiserVersion;

    private readonly Dictionary<int, ISerialiser> serialisers = new();

    /// <summary>
    /// Registers an <see cref="ISerialiser"/> with a specific version. Lower versions that get deserialised will be migrated to the highest serialiser version
    /// </summary>
    /// <param name="version">The version of the serialiser</param>
    /// <param name="serialiser">The <see cref="ISerialiser"/></param>
    public void RegisterSerialiser(int version, ISerialiser serialiser)
    {
        latestSerialiserVersion = Math.Max(version, latestSerialiserVersion);
        serialisers.Add(version, serialiser);
    }

    /// <summary>
    /// Deserialises by attempting to find the correct <see cref="ISerialiser"/> for the file version
    /// </summary>
    /// <param name="serialiseOnFail">If the deserialisation fails, serialise immediately. Useful for first time setup</param>
    /// <param name="filePathOverride">Allows for an override for deserialising for importing from a different location</param>
    public DeserialisationResult Deserialise(bool serialiseOnFail = true, string filePathOverride = "")
    {
        if (string.IsNullOrEmpty(filePathOverride))
        {
            if (!serialisers.Values.Any(serialiser => serialiser.DoesFileExist()) && serialiseOnFail)
            {
                Serialise();
                return DeserialisationResult.Success;
            }
        }
        else
        {
            if (!File.Exists(filePathOverride)) return DeserialisationResult.MissingFile;
        }

        foreach (var (version, serialiser) in serialisers.OrderBy(pair => pair.Key))
        {
            if (!serialiser.TryGetVersion(out var foundVersion)) continue;
            if (version != foundVersion) continue;

            return deserialise(serialiser, filePathOverride);
        }

        // Since we've got to this point that means the file is corrupt
        return DeserialisationResult.CorruptFile;
    }

    private DeserialisationResult deserialise(ISerialiser serialiser, string filePathOverride)
    {
        var deserialisationResult = serialiser.Deserialise(filePathOverride);
        if (deserialisationResult != DeserialisationResult.Success) return deserialisationResult;

        // serialise immediately if we've deserialised from a file path override, or we're not on the latest serialiser
        if (!string.IsNullOrEmpty(filePathOverride) || serialisers[latestSerialiserVersion] != serialiser) Serialise();

        return DeserialisationResult.Success;
    }

    public SerialisationResult Serialise() => serialisers[latestSerialiserVersion].Serialise();
}

public enum DeserialisationResult
{
    Success,
    MissingFile,
    CorruptFile,
    GenericError
}

public enum SerialisationResult
{
    Success,
    GenericError
}