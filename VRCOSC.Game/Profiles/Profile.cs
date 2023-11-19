// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using osu.Framework.Bindables;

namespace VRCOSC.Game.Profiles;

public class Profile
{
    public string SerialisedName => Name.Value;

    /// <summary>
    /// The name of this <see cref="Profile"/>
    /// </summary>
    public readonly Bindable<string> Name = new(string.Empty);

    /// <summary>
    /// The avatar ID of the bound avatar. Allows for binding an avatar to a profile to have the profile load when you change into an avatar
    /// </summary>
    public readonly Bindable<string> BoundAvatar = new(string.Empty);
}
