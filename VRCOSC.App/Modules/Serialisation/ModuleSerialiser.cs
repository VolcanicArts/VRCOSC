// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
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

        foreach (var settingPair in data.Settings)
        {
            var (settingKey, settingValue) = settingPair;

            try
            {
                var setting = Reference.GetSetting<ModuleSetting>(settingKey);

                if (!setting.InternalDeserialise(settingValue))
                    shouldReserialise = true;
            }
            catch (Exception)
            {
                // setting doesn't exist
                shouldReserialise = true;
            }
        }

        foreach (var parameterPair in data.Parameters)
        {
            var (parameterKey, parameterValue) = parameterPair;

            try
            {
                var parameter = Reference.GetParameter(parameterKey);
                parameter.Enabled.Value = parameterValue.Enabled;
                parameter.Name.Value = parameterValue.ParameterName;
            }
            catch (Exception)
            {
                // parameter doesn't exist
            }
        }

        return shouldReserialise;
    }
}