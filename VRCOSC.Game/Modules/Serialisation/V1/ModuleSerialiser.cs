// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Platform;
using VRCOSC.Game.Modules.SDK;
using VRCOSC.Game.Modules.SDK.Attributes;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Modules.Serialisation.V1;

public class ModuleSerialiser : Serialiser<Module, SerialisableModule>
{
    protected override string Directory => "configuration";

    // TODO: Replace with profiles
    protected override string FileName => $"{Reference.SerialisedName}.json";

    public ModuleSerialiser(Storage storage, Module reference)
        : base(storage, reference)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableModule data)
    {
        Reference.Enabled.Value = data.Enabled;

        data.Settings.ForEach(settingPair =>
        {
            var (settingKey, settingValue) = settingPair;

            var setting = Reference.GetSettingContainer<ModuleSetting>(settingKey);
            if (setting is null) return;

            setting.Deserialise(settingValue);
        });

        return false;
    }
}
