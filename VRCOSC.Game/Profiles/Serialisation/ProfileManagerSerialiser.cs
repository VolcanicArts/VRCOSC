// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Platform;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Profiles.Serialisation;

public class ProfileManagerSerialiser : Serialiser<ProfileManager, SerialisableProfileManager>
{
    protected override string Directory => "configuration";
    protected override string FileName => "profiles.json";

    public ProfileManagerSerialiser(Storage storage, ProfileManager reference)
        : base(storage, reference)
    {
    }

    protected override bool ExecuteAfterDeserialisation(SerialisableProfileManager data)
    {
        Reference.Profiles.AddRange(data.Profiles.Select(serialisableProfile =>
        {
            var profile = new Profile
            {
                Name =
                {
                    Value = serialisableProfile.Name
                }
            };
            profile.BoundAvatars.AddRange(new BindableList<string>(serialisableProfile.BoundAvatars));
            return profile;
        }));

        Reference.ActiveProfile.Value = Reference.Profiles.First(profile => profile.SerialisedName.Equals(data.ActiveProfile, StringComparison.InvariantCultureIgnoreCase));
        Reference.DefaultProfile.Value = Reference.Profiles.First(profile => profile.SerialisedName.Equals(data.DefaultProfile, StringComparison.InvariantCultureIgnoreCase));

        return false;
    }
}
