// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Framework.Platform;
using VRCOSC.Game.Profiles.Serialisation;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Profiles;

public class ProfileManager
{
    public readonly BindableList<Profile> Profiles = new();

    /// <summary>
    /// The current active profile that the app is using
    /// </summary>
    public readonly Bindable<Profile> ActiveProfile = new();

    /// <summary>
    /// The default profile as selected in the settings. This is the profile that the app will default back to if automatic profile switching fails to find a suitable profile
    /// </summary>
    public readonly Bindable<Profile> DefaultProfile = new();

    private readonly SerialisationManager serialisationManager;

    public ProfileManager(Storage storage)
    {
        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(1, new ProfileManagerSerialiser(storage, this));

        ActiveProfile.BindValueChanged(e => Logger.Log($"Active profile changed to {e.NewValue.SerialisedName}"));
        DefaultProfile.BindValueChanged(e => Logger.Log($"Default profile changed to {e.NewValue.SerialisedName}"));
    }

    public void Serialise() => serialisationManager.Serialise();
    public void Deserialise() => serialisationManager.Deserialise();

    public void Load()
    {
        Deserialise();

        checkForDefault();

        Profiles.BindCollectionChanged((_, _) => Serialise());
        ActiveProfile.BindValueChanged(_ => Serialise());
        DefaultProfile.BindValueChanged(_ => Serialise());
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
