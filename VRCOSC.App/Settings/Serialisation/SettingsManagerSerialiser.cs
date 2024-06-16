// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Settings.Serialisation;

public class SettingsManagerSerialiser : Serialiser<SettingsManager, SerialisableSettingsManager>
{
    protected override string Directory => "configuration";
    protected override string FileName => "settings.json";

    public SettingsManagerSerialiser(Storage storage, SettingsManager reference)
        : base(storage, reference)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableSettingsManager data)
    {
        var shouldReserialise = false;

        data.Settings.ForEach(pair =>
        {
            try
            {
                if (!Enum.TryParse(pair.Key, out VRCOSCSetting key))
                {
                    shouldReserialise = true;
                    return;
                }

                if (!Reference.Settings.TryGetValue(key, out var setting)) return;

                var value = pair.Value;

                if (value is long longValue)
                    value = (int)longValue;

                if (value is double doubleValue)
                    value = (float)doubleValue;

                setting.Value = value;
            }
            catch (Exception)
            {
                // ignore errors since it's probably a change in type
            }
        });

        return shouldReserialise;
    }
}
