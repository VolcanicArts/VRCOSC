// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.VRChat;

public record World
{
    public readonly string Id;
    public readonly string Name;

    public World(string id, string name)
    {
        Id = id;
        Name = name;
    }

    public override string ToString() => $"{Id}\n({Name})";

    public override int GetHashCode() => Id.GetHashCode();

    public virtual bool Equals(World? other) => Id == other?.Id;
}