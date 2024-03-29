﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;
using osu.Framework.Platform;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Modules.Persistence;

public class ModulePersistenceSerialiser : Serialiser<Module, SerialisableModulePersistence>
{
    protected override string FileName => $"{Reference.SerialisedName}.json";
    protected override string? LegacyFileName => Reference.LegacySerialisedName is null ? null : $"{Reference.LegacySerialisedName}.json";
    protected override string Directory => "module-states";

    public ModulePersistenceSerialiser(Storage storage, Module reference)
        : base(storage, reference)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableModulePersistence data)
    {
        data.Properties.ForEach(propertyData =>
        {
            if (!Reference.TryGetPersistentProperty(propertyData.Key, out var propertyInfo)) return;

            var targetType = propertyInfo.PropertyType;
            var propertyValue = JsonConvert.DeserializeObject(propertyData.Value.ToString(), targetType);
            propertyInfo.SetValue(Reference, propertyValue);
        });

        return false;
    }
}
