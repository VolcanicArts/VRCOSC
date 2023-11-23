// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.IO;
using osu.Framework.Bindables;
using osu.Framework.Platform;
using VRCOSC.Game.Profiles;

namespace VRCOSC.Game.Serialisation;

public abstract class ProfiledSerialiser<TReference, TSerialisable> : Serialiser<TReference, TSerialisable> where TSerialisable : class
{
    private readonly Bindable<Profile> activeProfile;

    protected override string Directory => Path.Join("profiles", activeProfile.Value.ID.ToString());

    protected ProfiledSerialiser(Storage storage, TReference reference, Bindable<Profile> activeProfile)
        : base(storage, reference)
    {
        this.activeProfile = activeProfile;
    }
}
