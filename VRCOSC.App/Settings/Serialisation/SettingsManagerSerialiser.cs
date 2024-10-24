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

        foreach (var pair in data.Settings)
        {
            if (!Enum.TryParse(pair.Key, out VRCOSCSetting key))
            {
                shouldReserialise = true;
                continue;
            }

            if (!Reference.Settings.ContainsKey(key))
            {
                shouldReserialise = true;
                continue;
            }

            var observable = Reference.GetObservable(key);
            var targetType = observable.GetValueType();

            if (!TryConvertToTargetType(pair.Value, targetType, out var parsedValue))
            {
                shouldReserialise = true;
                continue;
            }

            observable.SetValue(parsedValue);
        }

        foreach (var pair in data.Metadata)
        {
            if (!Enum.TryParse(pair.Key, out VRCOSCMetadata key))
            {
                shouldReserialise = true;
                continue;
            }

            if (!Reference.Metadata.ContainsKey(key))
            {
                shouldReserialise = true;
                continue;
            }

            var observable = Reference.GetObservable(key);
            var targetType = observable.GetValueType();

            if (!TryConvertToTargetType(pair.Value, targetType, out var parsedValue))
            {
                shouldReserialise = true;
                continue;
            }

            observable.SetValue(parsedValue);
        }

        return shouldReserialise;
    }
}