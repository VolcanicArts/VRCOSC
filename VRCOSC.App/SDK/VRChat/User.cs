// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.VRChat;

public record User(string UserId, string Username)
{
    public override string ToString() => $"{Username} ({UserId})";

    public virtual bool Equals(User? other) => UserId == other?.UserId;

    public override int GetHashCode() => UserId.GetHashCode();
}