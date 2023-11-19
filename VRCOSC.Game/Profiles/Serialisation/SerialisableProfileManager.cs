// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Profiles.Serialisation;

public class SerialisableProfileManager : SerialisableVersion
{
    [JsonProperty("profiles")]
    public List<SerialisableProfile> Profiles = new();

    [JsonProperty("default_profile")]
    public string DefaultProfile = null!;

    [JsonProperty("active_profile")]
    public string ActiveProfile = null!;

    [JsonConstructor]
    public SerialisableProfileManager()
    {
    }

    public SerialisableProfileManager(ProfileManager profileManager)
    {
        Version = 1;

        Profiles.AddRange(profileManager.Profiles.Select(profile => new SerialisableProfile(profile)));
        DefaultProfile = profileManager.DefaultProfile.Value.SerialisedName;
        ActiveProfile = profileManager.ActiveProfile.Value.SerialisedName;
    }
}

public class SerialisableProfile
{
    [JsonProperty("name")]
    public string Name = null!;

    [JsonProperty("bound_avatars")]
    public List<string> BoundAvatars = null!;

    [JsonConstructor]
    public SerialisableProfile()
    {
    }

    public SerialisableProfile(Profile profile)
    {
        Name = profile.Name.Value;
        BoundAvatars = profile.BoundAvatars.ToList();
    }
}
