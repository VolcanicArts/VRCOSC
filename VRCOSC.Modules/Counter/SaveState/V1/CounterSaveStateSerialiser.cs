// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Platform;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Modules;

namespace VRCOSC.Modules.Counter.SaveState.V1;

public class CounterSaveStateSerialiser : SaveStateSerialiser<CounterModule, SerialisableCounterSaveState>
{
    public CounterSaveStateSerialiser(Storage storage, NotificationContainer notification, CounterModule reference)
        : base(storage, notification, reference)
    {
    }

    protected override SerialisableCounterSaveState GetSerialisableData(CounterModule reference) => new(reference);

    protected override void ExecuteAfterDeserialisation(CounterModule reference, SerialisableCounterSaveState data)
    {
        data.Instances.ForEach(instance => reference.Counts[instance.Key].Count = instance.Count);
    }
}
