// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.VRChat;

public record User
{
    public readonly string Id;
    public readonly string Username;

    public User(string id, string username)
    {
        Id = id;
        Username = username;
    }

    public override string ToString() => $"{Id}\n({Username})";

    public override int GetHashCode() => Id.GetHashCode();

    public virtual bool Equals(User? other) => Id == other?.Id;
}