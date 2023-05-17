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

    public void Deserialise()
    {
        if (!serialisers.Values.Any(serialiser => serialiser.DoesFileExist()))
        {
            Serialise();
            return;
        }

        foreach (var (version, serialiser) in serialisers.OrderBy(pair => pair.Key))
        {
            if (!serialiser.TryGetVersion(out var foundVersion)) continue;
            if (version != foundVersion) continue;

            deserialise(serialiser);
            return;
        }

        // Attempt to deserialise using the 0th serialiser which is reserved for files from before the serialisation standardisation
        // Note: 0th used for RouterManager migration
        if (serialisers.TryGetValue(0, out var zerothSerialiser))
        {
            deserialise(zerothSerialiser);
            return;
        }

        // Since we've got to this point that means a file exists that has no version, or the file is corrupt
        // As a last resort, attempt to deserialise with the latest serialiser. This also triggers the error notification
        if (!serialisers.TryGetValue(latestSerialiserVersion, out var latestSerialiser)) return;

        deserialise(latestSerialiser);
    }

    private void deserialise(ISerialiser serialiser)
    {
        if (!serialiser.Deserialise()) return;

        Serialise();
    }

    public bool Serialise() => serialisers[latestSerialiserVersion].Serialise();
}
