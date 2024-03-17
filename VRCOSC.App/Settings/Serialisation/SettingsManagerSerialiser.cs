// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

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
        data.Settings.ForEach(pair => Reference.Settings.Add(pair.Key, new Observable<object>(pair.Value)));
        return false;
    }
}
