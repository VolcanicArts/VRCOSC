// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Framework.Platform;
using VRCOSC.Game.Config;
using VRCOSC.Game.Profiles.Serialisation;
using VRCOSC.Game.Serialisation;

namespace VRCOSC.Game.Profiles;

public class ProfileManager
{
    private readonly AppManager appManager;
    private readonly Storage storage;
    private readonly VRCOSCConfigManager configManager;

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

    public ProfileManager(AppManager appManager, Storage storage, VRCOSCConfigManager configManager)
    {
        this.appManager = appManager;
        this.storage = storage;
        this.configManager = configManager;
        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(1, new ProfileManagerSerialiser(storage, this));

        ActiveProfile.BindValueChanged(e => Logger.Log($"Active profile changed to {e.NewValue.ID}"));
        DefaultProfile.BindValueChanged(e => Logger.Log($"Default profile changed to {e.NewValue.ID}"));
    }

    public void Serialise() => serialisationManager.Serialise();
    public void Deserialise() => serialisationManager.Deserialise();

    public void Load()
    {
        Deserialise();

        checkForDefault();

        Profiles.BindCollectionChanged((_, e) =>
        {
            if (Profiles.All(profile => profile != DefaultProfile.Value))
            {
                DefaultProfile.Value = Profiles[0];
                appManager.ChangeProfile(DefaultProfile.Value);
            }

            if (e.OldItems is not null)
            {
                foreach (Profile oldProfile in e.OldItems)
                {
                    storage.DeleteDirectory($"profiles/{oldProfile.ID}");
                }
            }

            Serialise();
        });

        ActiveProfile.BindValueChanged(_ => Serialise());
        DefaultProfile.BindValueChanged(_ => Serialise());

        Profiles.BindCollectionChanged((_, e) =>
        {
            if (e.NewItems is null) return;

            foreach (Profile profile in e.NewItems)
            {
                profile.Name.BindValueChanged(_ => Serialise());
                profile.LinkedAvatars.BindCollectionChanged((_, _) => Serialise());

                profile.LinkedAvatars.BindCollectionChanged((_, e2) =>
                {
                    if (e2.NewItems is null) return;

                    foreach (Bindable<string> linkedAvatar in e2.NewItems)
                    {
                        linkedAvatar.BindValueChanged(_ => Serialise());
                    }
                }, true);
            }
        }, true);

        Serialise();
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
    }

    /// <summary>
    /// Called when the user changes avatar
    /// </summary>
    /// <param name="avatarId">The avatar ID of the new avatar</param>
    /// <returns>True if the profile was changed, otherwise false</returns>
    public bool AvatarChange(string avatarId)
    {
        if (!configManager.Get<bool>(VRCOSCSetting.EnableAutomaticProfileSwitching)) return false;

        var avatarBoundProfile = Profiles.FirstOrDefault(profile => profile.LinkedAvatars.Select(linkedAvatar => linkedAvatar.Value).Contains(avatarId));
        var newProfile = avatarBoundProfile ?? DefaultProfile.Value;

        if (newProfile == ActiveProfile.Value) return false;

        appManager.ChangeProfile(newProfile);
        return true;
    }
}
