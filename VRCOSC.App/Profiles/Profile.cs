// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.ObjectModel;
using System.Linq;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Profiles;

public class Profile
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

    public Profile Clone()
    {
        return new Profile(ID)
        {
            Name = new Observable<string>(Name.Value),
            LinkedAvatars = new ObservableCollection<Observable<string>>(LinkedAvatars.Select(linkedAvatarObservable => new Observable<string>(linkedAvatarObservable.Value)))
        };
    }

    public void CopyTo(Profile profile)
    {
        profile.Name.Value = Name.Value;
        profile.LinkedAvatars.Clear();
        profile.LinkedAvatars.AddRange(LinkedAvatars.Select(linkedAvatarObservable => new Observable<string>(linkedAvatarObservable.Value)));
    }
}
