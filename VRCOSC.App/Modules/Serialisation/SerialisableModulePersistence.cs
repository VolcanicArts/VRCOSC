// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        Version = 2;

        reference.PersistentProperties.ForEach(pair =>
        {
            var propertyValue = JsonConvert.SerializeObject(pair.Value.GetValue(reference));
            var propertyKey = pair.Key.SerialisedName;

            var serialisablePersistentProperty = new SerialisablePersistentProperty
            {
                Key = propertyKey,
                Value = new JRaw(propertyValue)
            };

            Properties.Add(serialisablePersistentProperty);
        });
    }

    public class SerialisablePersistentProperty
    {
        [JsonProperty("key")]
        public string Key = null!;

        [JsonProperty("value")]
        public JRaw Value = null!;
    }
}
