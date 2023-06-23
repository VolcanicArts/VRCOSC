// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VRCOSC.Game.Serialisation;

public class SerialisationManager
{
    private int latestSerialiserVersion;

    private readonly Dictionary<int, ISerialiser> serialisers = new();

    public void RegisterSerialiser(int version, ISerialiser serialiser)
    {
        serialiser.Initialise();
        latestSerialiserVersion = Math.Max(version, latestSerialiserVersion);
        serialisers.Add(version, serialiser);
    }

    public bool Deserialise(string filePathOverride = "")
    {
        if (string.IsNullOrEmpty(filePathOverride))
        {
            if (!serialisers.Values.Any(serialiser => serialiser.DoesFileExist()))
            {
                Serialise();
                return false;
            }
        }
        else
        {
            if (!File.Exists(filePathOverride)) return false;
        }

        foreach (var (version, serialiser) in serialisers.OrderBy(pair => pair.Key))
        {
            if (!serialiser.TryGetVersion(out var foundVersion)) continue;
            if (version != foundVersion) continue;

            return deserialise(serialiser, filePathOverride);
        }

        // If a version 0 exists then use this to deserialise as it's for legacy migration
        if (serialisers.TryGetValue(0, out var legacySerialiser))
        {
            return deserialise(legacySerialiser, filePathOverride);
        }

        // Since we've got to this point that means the file is corrupt
        // As a last resort, attempt to deserialise with the latest serialiser. This also triggers the error notification
        return deserialise(serialisers[latestSerialiserVersion], filePathOverride);
    }

    private bool deserialise(ISerialiser serialiser, string filePathOverride)
    {
        if (!serialiser.Deserialise(filePathOverride)) return false;

        if (string.IsNullOrEmpty(filePathOverride) && serialisers[latestSerialiserVersion] == serialiser) return true;

        Serialise();
        return true;
    }

    public bool Serialise() => serialisers[latestSerialiserVersion].Serialise();
}
