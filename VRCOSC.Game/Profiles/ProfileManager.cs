// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Platform;
using VRCOSC.Game.Profiles.Serialisation;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Profiles;

public class ProfileManager
{
    public readonly List<Profile> Profiles = new();

    /// <summary>
    /// The current active profile that the app is using
    /// </summary>
    public readonly Bindable<Profile> ActiveProfile = new();

    /// <summary>
    /// The default profile as selected in the settings
    /// </summary>
    public readonly Bindable<Profile> DefaultProfile = new();

    private readonly SerialisationManager<ProfileManager> serialisationManager;

    public ProfileManager(Storage storage)
    {
        serialisationManager = new SerialisationManager<ProfileManager>(storage, this);
        serialisationManager.RegisterSerialiser<ProfileManagerSerialiser>(1);
    }

    public void Serialise() => serialisationManager.Serialise();
    public void Deserialise() => serialisationManager.Deserialise();

    public void Load()
    {
        Deserialise();

        checkForDefault();
    }

    private void checkForDefault()
    {
        if (Profiles.Any()) return;

        var defaultProfile = new Profile
        {
            Name = { Value = "Default" }
        };

        Profiles.Add(defaultProfile);
        ActiveProfile.Value = defaultProfile;
        DefaultProfile.Value = defaultProfile;

        Serialise();
    }

    /// <summary>
    /// Called when the user changes avatar
    /// </summary>
    /// <param name="avatarId">The avatar ID of the new avatar</param>
    /// <returns>True if the profile was changed, otherwise false</returns>
    public bool AvatarChange(string avatarId)
    {
        var avatarBoundProfile = Profiles.FirstOrDefault(profile => profile.BoundAvatar.Value == avatarId);
        var newProfile = avatarBoundProfile ?? DefaultProfile.Value;

        if (newProfile == ActiveProfile.Value) return false;

        ActiveProfile.Value = newProfile;
        return true;
    }
}
