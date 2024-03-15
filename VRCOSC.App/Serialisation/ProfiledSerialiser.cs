// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.IO;
using VRCOSC.App.Profiles;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Serialisation;

public abstract class ProfiledSerialiser<TReference, TSerialisable> : Serialiser<TReference, TSerialisable> where TSerialisable : class
{
    private readonly Observable<Profile> activeProfile;

    protected override string Directory => Path.Join("profiles", activeProfile.Value.ID.ToString());

    protected ProfiledSerialiser(Storage storage, TReference reference, Observable<Profile> activeProfile)
        : base(storage, reference)
    {
        this.activeProfile = activeProfile;
    }
}
