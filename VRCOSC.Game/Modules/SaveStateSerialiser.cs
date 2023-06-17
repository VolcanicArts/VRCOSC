// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Platform;
using VRCOSC.Game.Graphics.Notifications;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Modules;

public interface ISaveStateSerialiser : ISerialiser
{
}

public abstract class SaveStateSerialiser<TReference, TSerialisable> : Serialiser<TReference, TSerialisable>, ISaveStateSerialiser where TSerialisable : class where TReference : Module
{
    protected override string Directory => "module-states";
    protected override string FileName => $"{Reference.SerialisedName}.json";

    protected SaveStateSerialiser(Storage storage, NotificationContainer notification, TReference reference)
        : base(storage, notification, reference)
    {
    }
}
