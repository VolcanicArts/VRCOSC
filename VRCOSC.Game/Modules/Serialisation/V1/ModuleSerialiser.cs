// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Platform;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Managers;
using VRCOSC.Game.Modules.Serialisation.V1.Models;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Modules.Serialisation.V1;

public class ModuleSerialiser : Serialiser<ModuleManager, SerialisableModuleManager>
{
    protected override string FileName => "modules.json";

    public ModuleSerialiser(Storage storage, NotificationContainer notification, ModuleManager reference)
        : base(storage, notification, reference)
    {
    }

    protected override object GetSerialisableData(ModuleManager moduleManager) => new SerialisableModuleManager(moduleManager.Modules);

    protected override void ExecuteAfterDeserialisation(ModuleManager moduleManager, SerialisableModuleManager data)
    {
        data.Modules.ForEach(modulePair =>
        {
            var (moduleName, moduleData) = modulePair;

            var module = moduleManager.Modules.SingleOrDefault(module => module.SerialisedName == moduleName);
            if (module is null) return;

            module.Enabled.Value = moduleData.Enabled;

            moduleData.Settings.ForEach(settingPair =>
            {
                var (settingKey, settingValue) = settingPair;

                if (!module.DoesSettingExist(settingKey, out var setting)) return;

                if (setting.Type.IsEnum)
                {
                    setting.Attribute.Value = Enum.ToObject(setting.Type, settingValue);
                    return;
                }

                setting.Attribute.Value = Convert.ChangeType(settingValue, setting.Type);
            });

            moduleData.Parameters.ForEach(parameterPair =>
            {
                var (parameterKey, parameterValue) = parameterPair;

                if (!module.DoesParameterExist(parameterKey, out var parameter)) return;

                parameter.Attribute.Value = Convert.ChangeType(parameterValue, parameter.Type);
            });
        });
    }
}
