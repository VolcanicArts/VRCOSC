// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.IO;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Modules.Serialisation;

public class ModulePersistenceSerialiser : ProfiledSerialiser<Module, SerialisableModulePersistence>
{
    protected override string FileName => $"{Reference.FullID}.json";
    protected override string Directory => Path.Join(base.Directory, "persistence");

    public ModulePersistenceSerialiser(Storage storage, Module reference)
        : base(storage, reference)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableModulePersistence data)
    {
        foreach (var propertyData in data.Properties)
        {
            if (!Reference.TryGetPersistentProperty(propertyData.Key, out var propertyInfo)) continue;
            if (!TryConvertToTargetType(propertyData.Value, propertyInfo.PropertyType, out var parsedValue)) continue;

            propertyInfo.SetValue(Reference, parsedValue);
        }

        return false;
    }
}
