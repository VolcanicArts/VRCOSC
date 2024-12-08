// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using Newtonsoft.Json;
using VRCOSC.App.SDK.Modules;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Modules.Serialisation;

public class SerialisableModulePersistence
{
    [JsonProperty("version")]
    public int Version;

    [JsonProperty("properties")]
    public List<SerialisablePersistentProperty> Properties = new();

    [JsonConstructor]
    public SerialisableModulePersistence()
    {
    }

    public SerialisableModulePersistence(Module reference)
    {
        Version = 1;

        reference.PersistentProperties.ForEach(pair =>
        {
            var propertyKey = pair.Key.SerialisedName;

            var serialisablePersistentProperty = new SerialisablePersistentProperty
            {
                Key = propertyKey,
                Value = pair.Value.GetValue(reference)
            };

            Properties.Add(serialisablePersistentProperty);
        });
    }

    public class SerialisablePersistentProperty
    {
        [JsonProperty("key")]
        public string Key = null!;

        [JsonProperty("value")]
        public object? Value;
    }
}
