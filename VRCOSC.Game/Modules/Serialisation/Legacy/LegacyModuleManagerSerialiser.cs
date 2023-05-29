// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Platform;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Managers;
using VRCOSC.Game.Modules.Serialisation.Legacy.Models;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Modules.Serialisation.Legacy;

public class LegacyModuleManagerSerialiser : Serialiser<ModuleManager, LegacySerialisableModuleManager>
{
    protected override string FileName => "modules.json";

    public LegacyModuleManagerSerialiser(Storage storage, NotificationContainer notification, ModuleManager reference)
        : base(storage, notification, reference)
    {
    }

    protected override LegacySerialisableModuleManager GetSerialisableData(ModuleManager moduleManager) => new(moduleManager);

    protected override void ExecuteAfterDeserialisation(ModuleManager moduleManager, LegacySerialisableModuleManager data)
    {
        data.Modules.ForEach(modulePair =>
        {
            var (moduleName, moduleData) = modulePair;

            var module = moduleManager.SingleOrDefault(module => module.SerialisedName == moduleName);
            if (module is null) return;

            module.Enabled.Value = moduleData.Enabled;

            moduleData.Settings.ForEach(settingPair =>
            {
                var (settingKey, settingValue) = settingPair;

                if (!module.DoesSettingExist(settingKey, out var setting)) return;

                setting.DeserialiseValue(settingValue);
            });

            moduleData.Parameters.ForEach(parameterPair =>
            {
                var (parameterKey, parameterValue) = parameterPair;

                if (!module.DoesParameterExist(parameterKey, out var parameter)) return;

                parameter.DeserialiseValue(parameterValue);
            });
        });
    }
}
