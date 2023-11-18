// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.IO;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Platform;
using VRCOSC.Game.Modules.SDK;
using VRCOSC.Game.Modules.SDK.Attributes;
using VRCOSC.Game.Profiles;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Modules.Serialisation;

public class ModuleSerialiser : ProfiledSerialiser<Module, SerialisableModule>
{
    protected override string Directory => Path.Join(base.Directory, "modules");
    protected override string FileName => $"{Reference.SerialisedName}.json";

    public ModuleSerialiser(Storage storage, Module reference, Bindable<Profile> activeProfile)
        : base(storage, reference, activeProfile)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableModule data)
    {
        Reference.Enabled.Value = data.Enabled;

        data.Settings.ForEach(settingPair =>
        {
            var (settingKey, settingValue) = settingPair;

            var setting = Reference.GetSetting<ModuleSetting>(settingKey);
            if (setting is null) return;

            setting.Deserialise(settingValue);
        });

        data.Parameters.ForEach(parameterPair =>
        {
            var (parameterKey, parameterValue) = parameterPair;

            var parameter = Reference.GetParameter(parameterKey);
            if (parameter is null) return;

            parameter.Deserialise(parameterValue);
        });

        return false;
    }
}
