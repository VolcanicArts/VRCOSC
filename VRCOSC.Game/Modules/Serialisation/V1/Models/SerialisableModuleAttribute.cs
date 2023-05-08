// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

namespace VRCOSC.Game.Modules.Serialisation.V1.Models;

public class SerialisableModuleAttribute
{
    [JsonProperty("value")]
    public object Value = null!;

    [JsonConstructor]
    public SerialisableModuleAttribute()
    {
    }

    public SerialisableModuleAttribute(ModuleAttribute attribute)
    {
        Value = attribute.Attribute.Value;
    }
}
