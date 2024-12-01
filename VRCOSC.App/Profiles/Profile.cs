// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Profiles;

public class Profile : INotifyPropertyChanged
{
    /// <summary>
    /// The unique ID of this <see cref="Profile"/>
    /// </summary>
    public Guid ID;

    /// <summary>
    /// The name of this <see cref="Profile"/>
    /// </summary>
    public Observable<string> Name { get; private init; } = new(string.Empty);

    /// <summary>
    /// The avatar IDs of the linked avatars. Allows for linking avatars to a profile to have the profile load when you change into an avatar
    /// </summary>
    public ObservableCollection<Observable<string>> LinkedAvatars { get; private init; } = new();

    public Profile()
    {
        ID = Guid.NewGuid();
    }

    public Profile(Guid id)
    {
        ID = id;
    }

    /// <summary>
    /// Clones this profile
    /// </summary>
    /// <param name="hardCopy">Hard copy means the ID and name won't be changed. Soft copying will generate a new ID and append 'New' to the profile name</param>
    public Profile Clone(bool hardCopy)
    {
        if (hardCopy)
        {
            return new Profile(ID)
            {
                Name = new Observable<string>($"{Name.Value}"),
                LinkedAvatars = new ObservableCollection<Observable<string>>(LinkedAvatars.Select(linkedAvatarObservable => new Observable<string>(linkedAvatarObservable.Value)))
            };
        }
        else
        {
            return new Profile
            {
                Name = new Observable<string>($"{Name.Value} New"),
                LinkedAvatars = new ObservableCollection<Observable<string>>(LinkedAvatars.Select(linkedAvatarObservable => new Observable<string>(linkedAvatarObservable.Value)))
            };
        }
    }

    public void CopyTo(Profile profile)
    {
        profile.Name.Value = Name.Value;
        profile.LinkedAvatars.Clear();
        profile.LinkedAvatars.AddRange(LinkedAvatars.Select(linkedAvatarObservable => new Observable<string>(linkedAvatarObservable.Value)));
    }

    public Visibility UIRemoveProfileButtonVisibility => ProfileManager.GetInstance().Profiles.Count > 1 ? Visibility.Visible : Visibility.Collapsed;

    public void UpdateUI()
    {
        OnPropertyChanged(nameof(UIRemoveProfileButtonVisibility));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}