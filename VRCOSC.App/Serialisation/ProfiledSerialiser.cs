// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.IO;
using VRCOSC.App.Profiles;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Serialisation;

public abstract class ProfiledSerialiser<TReference, TSerialisable> : Serialiser<TReference, TSerialisable> where TSerialisable : class
{
    protected override string Directory => Path.Join("profiles", ProfileManager.GetInstance().ActiveProfile.Value.ID.ToString());

    protected ProfiledSerialiser(Storage storage, TReference reference)
        : base(storage, reference)
    {
    }
}
