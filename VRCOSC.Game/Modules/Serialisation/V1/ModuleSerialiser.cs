// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Platform;
using VRCOSC.Game.Modules.Serialisation.V1.Models;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Modules.Serialisation.V1;

public class ModuleSerialiser : Serialiser<Module, SerialisableModule>
{
    protected override string Directory => "modules";
    protected override string FileName => $"{Reference.SerialisedName}.json";
    protected override string? LegacyFileName => Reference.LegacySerialisedName is null ? null : $"{Reference.LegacySerialisedName}.json";

    public ModuleSerialiser(Storage storage, Module reference)
        : base(storage, reference)
    {
    }

    protected override bool ExecuteAfterDeserialisation(Module module, SerialisableModule data)
    {
        module.Enabled.Value = data.Enabled;

        data.Settings.ForEach(settingPair =>
        {
            var (settingKey, settingValue) = settingPair;

            if (module.TryGetSetting(settingKey, out var setting)) setting.DeserialiseValue(settingValue);
        });

        data.Parameters.ForEach(parameterPair =>
        {
            var (parameterKey, parameterValue) = parameterPair;

            if (module.TryGetParameter(parameterKey, out var parameter)) parameter.DeserialiseValue(parameterValue);
        });

        return false;
    }
}
