// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.IO;
using Newtonsoft.Json;
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
        data.Properties.ForEach(propertyData =>
        {
            try
            {
                if (!Reference.TryGetPersistentProperty(propertyData.Key, out var propertyInfo)) return;

                var targetType = propertyInfo.PropertyType;
                var propertyValue = JsonConvert.DeserializeObject(propertyData.Value.ToString(), targetType);
                propertyInfo.SetValue(Reference, propertyValue);
            }
            catch (Exception)
            {
                // probably fine to ignore since it's corrupted anyway
            }
        });

        return false;
    }
}
