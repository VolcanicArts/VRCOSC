// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VRCOSC.App.Serialisation;

namespace VRCOSC.App.Profiles.Serialisation;

public class SerialisableProfileManager : SerialisableVersion
{
    [JsonProperty("profiles")]
    public List<SerialisableProfile> Profiles = new();

    [JsonProperty("default_profile")]
    public Guid DefaultProfile;

    [JsonProperty("active_profile")]
    public Guid ActiveProfile;

    [JsonConstructor]
    public SerialisableProfileManager()
    {
    }

    public SerialisableProfileManager(ProfileManager profileManager)
    {
        Version = 1;

        Profiles.AddRange(profileManager.Profiles.Select(profile => new SerialisableProfile(profile)));
        DefaultProfile = profileManager.DefaultProfile.Value.ID;
        ActiveProfile = profileManager.ActiveProfile.Value.ID;
    }
}

public class SerialisableProfile
{
    [JsonProperty("id")]
    public Guid ID = Guid.NewGuid();

    [JsonProperty("name")]
    public string Name = null!;

    [JsonProperty("linked_avatars")]
    public List<string> LinkedAvatars = null!;

    [JsonConstructor]
    public SerialisableProfile()
    {
    }

    public SerialisableProfile(Profile profile)
    {
        ID = profile.ID;
        Name = profile.Name.Value;
        LinkedAvatars = profile.LinkedAvatars.Select(avatarId => avatarId.Value).ToList();
    }
}
