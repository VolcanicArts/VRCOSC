﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;

namespace VRCOSC.Game.Serialisation;

public class SerialisationManager
{
    private readonly Dictionary<int, ISerialiser> serialisers = new();

    public void RegisterSerialiser(int version, ISerialiser serialiser) => serialisers.Add(version, serialiser);

    public bool Deserialise()
    {
        var doesFileExist = false;

        foreach (var (_, serialiser) in serialisers)
        {
            if (serialiser.DoesFileExist()) doesFileExist = true;
        }

        if (!doesFileExist)
        {
            Serialise();
            return false;
        }

        foreach (var (version, serialiser) in serialisers)
        {
            if (!serialiser.TryGetVersion(out var foundVersion)) continue;
            if (version != foundVersion) continue;

            return serialiser.Deserialise();
        }

        // If there are no valid versions found there's either no file OR there is a file with no version
        // Attempt to deserialise using the 0th serialiser which is reserved for files from before the serialisation standardisation
        // Note: Used for RouterManager migration
        if (serialisers.TryGetValue(0, out var zerothSerialiser))
        {
            return zerothSerialiser.Deserialise();
        }

        return false;
    }

    public void Serialise()
    {
        serialisers.MaxBy(pair => pair.Key).Value.Serialise();
    }
}
