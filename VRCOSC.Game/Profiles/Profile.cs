// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using osu.Framework.Bindables;

namespace VRCOSC.Game.Profiles;

public class Profile
{
    /// <summary>
    /// The unique ID of this <see cref="Profile"/>
    /// </summary>
    public Guid ID;

    /// <summary>
    /// The name of this <see cref="Profile"/>
    /// </summary>
    public Bindable<string> Name { get; private init; } = new(string.Empty);

    /// <summary>
    /// The avatar IDs of the linked avatars. Allows for linking avatars to a profile to have the profile load when you change into an avatar
    /// </summary>
    public BindableList<Bindable<string>> LinkedAvatars { get; private init; } = new();

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
            Name = Name.GetUnboundCopy(),
            LinkedAvatars = new BindableList<Bindable<string>>(LinkedAvatars.Select(linkedAvatarBindable => linkedAvatarBindable.GetUnboundCopy()))
        };
    }

    public void CopyTo(Profile profile)
    {
        profile.Name.Value = Name.Value;
        profile.LinkedAvatars.ReplaceRange(0, profile.LinkedAvatars.Count, new BindableList<Bindable<string>>(LinkedAvatars.Select(linkedAvatarBindable => linkedAvatarBindable.GetUnboundCopy())));
    }
}
