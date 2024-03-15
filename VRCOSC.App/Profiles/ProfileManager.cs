// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using VRCOSC.App.Pages.Profiles;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Profiles;

public class ProfileManager
{
    private static ProfileManager? instance;
    public static ProfileManager GetInstance() => instance ??= new ProfileManager();

    private readonly Storage storage = new NativeStorage($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/VRCOSC-V2-WPF");
    //private readonly VRCOSCConfigManager configManager;

    public ObservableCollection<Profile> Profiles { get; } = new();

    /// <summary>
    /// The current active profile that the app is using
    /// </summary>
    public Observable<Profile> ActiveProfile { get; } = new();

    /// <summary>
    /// The default profile as selected in the settings. This is the profile that the app will default back to if automatic profile switching fails to find a suitable profile
    /// </summary>
    public Observable<Profile> DefaultProfile { get; } = new();

    private readonly SerialisationManager serialisationManager;

    public ProfileManager()
    {
        //this.configManager = configManager;
        serialisationManager = new SerialisationManager();
        //serialisationManager.RegisterSerialiser(1, new ProfileManagerSerialiser(storage, this));

        ActiveProfile.Subscribe(newProfile => Logger.Log($"Active profile changed to {newProfile.ID}"));
        DefaultProfile.Subscribe(newProfile => Logger.Log($"Default profile changed to {newProfile.ID}"));
    }

    public void Serialise() => serialisationManager.Serialise();
    public void Deserialise() => serialisationManager.Deserialise();

    public void Load()
    {
        //Deserialise();

        checkForDefault();

        UIActiveProfile.Value = ActiveProfile.Value;
        UIActiveProfile.Subscribe(newProfile => AppManager.GetInstance().ChangeProfile(newProfile));

        //ActiveProfile.Subscribe(newProfile => Serialise());
        //DefaultProfile.Subscribe(_ => Serialise());

        Profiles.CollectionChanged += (_, e) => onProfilesOnCollectionChanged(e.OldItems, e.NewItems);
        onProfilesOnCollectionChanged(null, Profiles);

        //Serialise();
    }

    private void onProfilesOnCollectionChanged(IList? oldItems, IList? newItems)
    {
        if (oldItems is not null)
        {
            foreach (Profile oldProfile in oldItems)
            {
                if (DefaultProfile.Value.ID.Equals(oldProfile.ID)) DefaultProfile.Value = Profiles[0];

                if (ActiveProfile.Value.ID.Equals(oldProfile.ID))
                {
                    UIActiveProfile.Value = DefaultProfile.Value;
                    AppManager.GetInstance().ChangeProfile(DefaultProfile.Value);
                }

                storage.DeleteDirectory($"profiles/{oldProfile.ID}");
            }
        }

        if (newItems is not null)
        {
            foreach (Profile profile in newItems)
            {
                //profile.Name.Subscribe(_ => Serialise());
                //profile.LinkedAvatars.CollectionChanged += (_, _) => Serialise();
                profile.LinkedAvatars.CollectionChanged += (_, e) => onLinkedAvatarsOnCollectionChanged(e.OldItems, e.NewItems);
                onLinkedAvatarsOnCollectionChanged(null, profile.LinkedAvatars);
            }
        }

        Profiles.ForEach(profile => profile.Update());

        // Serialise()
    }

    private void onLinkedAvatarsOnCollectionChanged(IList? _, IList? newItems)
    {
        if (newItems is null) return;

        foreach (Observable<string> linkedAvatar in newItems)
        {
            //linkedAvatar.Subscribe(_ => Serialise());
        }
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
        //if (!configManager.Get<bool>(VRCOSCSetting.AutomaticProfileSwitching)) return false;

        var avatarBoundProfile = Profiles.FirstOrDefault(profile => profile.LinkedAvatars.Select(linkedAvatar => linkedAvatar.Value).Contains(avatarId));
        var newProfile = avatarBoundProfile ?? DefaultProfile.Value;

        if (newProfile == ActiveProfile.Value) return false;

        AppManager.GetInstance().ChangeProfile(newProfile);
        return true;
    }

    #region UI

    public void SpawnProfileEditWindow(Profile? profile = null)
    {
        if (ProfileEditWindow is null)
        {
            ProfileEditWindow = new ProfileEditWindow(profile);

            ProfileEditWindow!.Closed += (_, _) =>
            {
                var mainWindow = Application.Current.MainWindow;
                if (mainWindow is null) return;

                mainWindow.WindowState = WindowState.Normal;
                mainWindow.Focus();

                ProfileEditWindow = null;
            };

            ProfileEditWindow.Show();
        }
        else
        {
            ProfileEditWindow.Focus();
        }
    }

    public void ExitProfileEditWindow()
    {
        if (ProfileEditWindow is null) return;

        ProfileEditWindow.ForceClose = true;
        ProfileEditWindow.Close();
    }

    public ProfileEditWindow? ProfileEditWindow { get; private set; }

    public ICommand UICreateProfileButton => new RelayCommand(_ => OnCreateProfileButtonClick());
    private void OnCreateProfileButtonClick() => SpawnProfileEditWindow();

    public Observable<Profile> UIActiveProfile { get; } = new();

    #endregion
}
