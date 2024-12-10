// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.ObjectModel;
using System.Linq;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Profiles.Serialisation;

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
            var profile = new Profile(serialisableProfile.ID)
            {
                Name =
                {
                    Value = serialisableProfile.Name
                }
            };
            profile.LinkedAvatars.AddRange(new ObservableCollection<Observable<string>>(serialisableProfile.LinkedAvatars.Select(linkedAvatarId => new Observable<string>(linkedAvatarId))));
            return profile;
        }));

        Reference.ActiveProfile.Value = Reference.Profiles.First(profile => profile.ID == data.ActiveProfile);
        Reference.DefaultProfile.Value = Reference.Profiles.First(profile => profile.ID == data.DefaultProfile);

        return false;
    }
}