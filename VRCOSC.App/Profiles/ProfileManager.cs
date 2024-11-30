// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using VRCOSC.App.Profiles.Serialisation;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Settings;
using VRCOSC.App.UI.Windows.Profiles;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Profiles;

public class ProfileManager : INotifyPropertyChanged
{
    private static ProfileManager? instance;
    internal static ProfileManager GetInstance() => instance ??= new ProfileManager();

    private readonly Storage storage = AppManager.GetInstance().Storage;

    public ObservableCollection<Profile> Profiles { get; } = new();

    /// <summary>
    /// The current active profile that the app is using
    /// </summary>
    public Observable<Profile> ActiveProfile { get; } = new();

    /// <summary>
    /// The default profile as selected in the settings. This is the profile that the app will default back to if automatic profile switching fails to find a suitable profile
    /// </summary>
    public Observable<Profile> DefaultProfile { get; } = new();

    public bool EnableAutomaticSwitching
    {
        get => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.AutomaticProfileSwitching).Value;
        set => SettingsManager.GetInstance().GetObservable<bool>(VRCOSCSetting.AutomaticProfileSwitching).Value = value;
    }

    private readonly SerialisationManager serialisationManager;

    public ProfileManager()
    {
        serialisationManager = new SerialisationManager();
        serialisationManager.RegisterSerialiser(1, new ProfileManagerSerialiser(storage, this));

        ActiveProfile.Subscribe(newProfile =>
        {
            Logger.Log($"Active profile changed to {newProfile.ID}");
            UIActiveProfile.Value = newProfile;
            OnPropertyChanged(nameof(UIActiveProfile.Value));
        });

        DefaultProfile.Subscribe(newProfile => Logger.Log($"Default profile changed to {newProfile.ID}"));
    }

    public void Serialise() => serialisationManager.Serialise();
    public void Deserialise() => serialisationManager.Deserialise();

    public void Load()
    {
        serialisationManager.Deserialise(false);

        checkForDefault();

        UIActiveProfile.Value = ActiveProfile.Value;
        UIActiveProfile.Subscribe(newProfile => AppManager.GetInstance().ChangeProfile(newProfile));

        ActiveProfile.Subscribe(_ => Serialise());
        DefaultProfile.Subscribe(_ => Serialise());

        Profiles.OnCollectionChanged(onProfilesOnCollectionChanged, true);

        Serialise();
    }

    private void onProfilesOnCollectionChanged(IEnumerable<Profile> newItems, IEnumerable<Profile> oldItems)
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

        foreach (Profile profile in newItems)
        {
            profile.Name.Subscribe(_ => Serialise());
            profile.LinkedAvatars.CollectionChanged += (_, _) => Serialise();
            profile.LinkedAvatars.OnCollectionChanged(onLinkedAvatarsOnCollectionChanged);
        }

        Profiles.ForEach(profile => profile.UpdateUI());

        Serialise();
    }

    private void onLinkedAvatarsOnCollectionChanged(IEnumerable<Observable<string>> newItems, IEnumerable<Observable<string>> oldItems)
    {
        if (!newItems.Any()) return;

        foreach (Observable<string> linkedAvatar in newItems)
        {
            linkedAvatar.Subscribe(_ => Serialise());
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
        if (!SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.AutomaticProfileSwitching)) return false;

        var avatarBoundProfile = Profiles.FirstOrDefault(profile => profile.LinkedAvatars.Select(linkedAvatar => linkedAvatar.Value).Contains(avatarId));
        var newProfile = avatarBoundProfile ?? DefaultProfile.Value;

        if (newProfile == ActiveProfile.Value) return false;

        AppManager.GetInstance().ChangeProfile(newProfile);
        return true;
    }

    public void CopyProfile(Profile originalProfile)
    {
        var copiedProfile = originalProfile.Clone(false);

        var originalProfileDirectory = storage.GetStorageForDirectory($"profiles/{originalProfile.ID}");
        originalProfileDirectory.CopyTo(storage.GetFullPath($"profiles/{copiedProfile.ID}"));

        Profiles.Add(copiedProfile);
    }

    #region UI

    public void SpawnProfileEditWindow(Profile? profile = null)
    {
        ProfileEditWindow = new ProfileEditWindow(profile);
        ProfileEditWindow.ShowDialog();
    }

    public void ExitProfileEditWindow()
    {
        ProfileEditWindow?.Close();
    }

    public ProfileEditWindow? ProfileEditWindow { get; private set; }

    public Observable<Profile> UIActiveProfile { get; } = new();

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}