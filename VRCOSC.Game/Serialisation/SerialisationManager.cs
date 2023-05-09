// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;

namespace VRCOSC.Game.Serialisation;

public class SerialisationManager
{
    private int latestSerialiserVersion;

    private readonly Dictionary<int, ISerialiser> serialisers = new();

    public void RegisterSerialiser(int version, ISerialiser serialiser)
    {
        latestSerialiserVersion = Math.Max(version, latestSerialiserVersion);
        serialisers.Add(version, serialiser);
    }

    public bool Deserialise()
    {
        var doesFileExist = false;

        foreach (var (_, serialiser) in serialisers.OrderBy(pair => pair.Key))
        {
            if (serialiser.DoesFileExist()) doesFileExist = true;
        }

        if (!doesFileExist)
        {
            Serialise();
            return false;
        }

        foreach (var (version, serialiser) in serialisers.OrderBy(pair => pair.Key))
        {
            if (!serialiser.TryGetVersion(out var foundVersion)) continue;
            if (version != foundVersion) continue;

            return serialiser.Deserialise();
        }

        // If there are no valid versions found there's either no file OR there is a file with no version
        // Attempt to deserialise using the 0th serialiser which is reserved for files from before the serialisation standardisation and latest serialiser

        // Note: 0th used for RouterManager migration
        if (serialisers.TryGetValue(0, out var zerothSerialiser)) return zerothSerialiser.Deserialise();
        if (serialisers.TryGetValue(latestSerialiserVersion, out var latestSerialiser)) return latestSerialiser.Deserialise();

        return false;
    }

    public bool Serialise()
    {
        return serialisers.MaxBy(pair => pair.Key).Value.Serialise();
    }
}
