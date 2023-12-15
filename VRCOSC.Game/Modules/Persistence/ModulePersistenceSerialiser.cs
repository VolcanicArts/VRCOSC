// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.IO;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Platform;
using VRCOSC.Profiles;
using VRCOSC.SDK;
using VRCOSC.Serialisation;

namespace VRCOSC.Modules.Persistence;

public class ModulePersistenceSerialiser : ProfiledSerialiser<Module, SerialisableModulePersistence>
{
    private readonly Bindable<bool> globalPersistence;

    protected override string FileName => $"{Reference.SerialisedName}.json";
    protected override string Directory => globalPersistence.Value ? "persistence" : Path.Join(base.Directory, "persistence");

    public ModulePersistenceSerialiser(Storage storage, Module reference, Bindable<Profile> activeProfile, Bindable<bool> globalPersistence)
        : base(storage, reference, activeProfile)
    {
        this.globalPersistence = globalPersistence;
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
