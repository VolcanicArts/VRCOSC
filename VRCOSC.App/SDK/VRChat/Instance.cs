// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;

namespace VRCOSC.App.SDK.VRChat;

public record Instance
{
    public readonly string? Id;
    public readonly string? OwnerId;
    public readonly string? Number;
    public readonly InstanceType Type;
    public readonly InstanceRegion Region;
    public readonly bool AgeGated;
    public readonly bool HasQueue;
    public readonly World World;
    public readonly List<User> Users = [];

    public Instance(string? id, string? ownerId, string? number, InstanceType type, InstanceRegion region, bool ageGated, bool hasQueue, World world)
    {
        Id = id;
        OwnerId = ownerId;
        Number = number;
        Type = type;
        Region = region;
        AgeGated = ageGated;
        HasQueue = hasQueue;
        World = world;
    }

    public override string ToString() => Id ?? "null";

    public override int GetHashCode() => Id?.GetHashCode() ?? 0;

    public virtual bool Equals(Instance? other) => Id == other?.Id;
}

public static class InstanceHelper
{
    public static InstanceType CodeToType(string code, bool canRequestInvite, string? groupAccessType) => code.ToLowerInvariant() switch
    {
        "group" when groupAccessType == "members" => InstanceType.Group,
        "group" when groupAccessType == "plus" => InstanceType.GroupPlus,
        "group" when groupAccessType == "public" => InstanceType.GroupPublic,
        "private" when !canRequestInvite => InstanceType.Invite,
        "private" when canRequestInvite => InstanceType.InvitePlus,
        "friends" => InstanceType.Friends,
        "hidden" => InstanceType.FriendsPlus,
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "Unable to parse type code")
    };

    public static InstanceRegion CodeToRegion(string code) => code.ToLowerInvariant() switch
    {
        "us" => InstanceRegion.USWest,
        "use" => InstanceRegion.USEast,
        "eu" => InstanceRegion.Europe,
        "jp" => InstanceRegion.Japan,
        _ => throw new ArgumentOutOfRangeException(nameof(code), code, "Unable to parse region code")
    };
}

public enum InstanceType
{
    Group, //group~members
    GroupPlus, //group~plus
    GroupPublic, //group~public
    Invite, //private
    InvitePlus, //private~canRequestInvite
    Friends, //friends
    FriendsPlus, //hidden
    Public //nothing
}

public enum InstanceRegion
{
    USWest, //us
    USEast, //use
    Europe, //eu
    Japan //jp
}