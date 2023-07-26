// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;
using osu.Framework.Platform;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Modules.Persistence;

public class ModulePersistenceSerialiser : Serialiser<Module, SerialisableModulePersistence>
{
    protected override string FileName => $"{Reference.SerialisedName}.json";
    protected override string? LegacyFileName => Reference.LegacySerialisedName is null ? null : $"{Reference.LegacySerialisedName}.json";
    protected override string Directory => "module-states";

    public ModulePersistenceSerialiser(Storage storage, NotificationContainer notification, Module reference)
        : base(storage, notification, reference)
    {
    }

    protected override bool ExecuteAfterDeserialisation(Module reference, SerialisableModulePersistence data)
    {
        data.Properties.ForEach(propertyData =>
        {
            if (!reference.TryGetPersistentProperty(propertyData.Key, out var propertyInfo)) return;

            var targetType = propertyInfo.PropertyType;
            var propertyValue = JsonConvert.DeserializeObject(propertyData.Value.ToString(), targetType);
            propertyInfo.SetValue(reference, propertyValue);
        });

        return false;
    }
}
