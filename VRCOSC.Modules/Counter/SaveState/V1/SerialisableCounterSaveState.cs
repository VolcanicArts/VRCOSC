// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Newtonsoft.Json;

namespace VRCOSC.Modules.Counter.SaveState.V1;

public class SerialisableCounterSaveState
{
    [JsonProperty("version")]
    public int Version;

    [JsonProperty("instances")]
    public List<CounterInstanceSaveState> Instances = new();

    [JsonConstructor]
    public SerialisableCounterSaveState()
    {
    }

    public SerialisableCounterSaveState(CounterModule module)
    {
        Version = 1;

        Instances.AddRange(module.Counts.Values.Select(instance => new CounterInstanceSaveState(instance)));
    }
}

public class CounterInstanceSaveState
{
    [JsonProperty("key")]
    public string Key = null!;

    [JsonProperty("count")]
    public int Count;

    [JsonConstructor]
    public CounterInstanceSaveState()
    {
    }

    public CounterInstanceSaveState(CountInstance instance)
    {
        Key = instance.Key;
        Count = instance.Count;
    }
}
