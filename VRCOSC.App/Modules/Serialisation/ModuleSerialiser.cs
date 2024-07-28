// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.IO;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.SDK.Modules.Attributes.Settings;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Modules.Serialisation;

public class ModuleSerialiser : ProfiledSerialiser<Module, SerialisableModule>
{
    protected override string Directory => Path.Join(base.Directory, "modules");
    protected override string FileName => $"{Reference.FullID}.json";

    public ModuleSerialiser(Storage storage, Module reference)
        : base(storage, reference)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableModule data)
    {
        var shouldReserialise = false;

        Reference.Enabled.Value = data.Enabled;

        data.Settings.ForEach(settingPair =>
        {
            var (settingKey, settingValue) = settingPair;

            var setting = Reference.GetSetting<ModuleSetting>(settingKey);

            if (setting is null)
            {
                shouldReserialise = true;
                return;
            }

            setting.Deserialise(settingValue);
        });

        data.Parameters.ForEach(parameterPair =>
        {
            var (parameterKey, parameterValue) = parameterPair;

            var parameter = Reference.GetParameter(parameterKey);

            if (parameter is null)
            {
                shouldReserialise = true;
                return;
            }

            parameter.Deserialise(parameterValue);
        });

        return shouldReserialise;
    }
}
