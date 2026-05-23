// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.SDK.VRChat;
using User = VRCOSC.App.SDK.VRChat.User;

namespace VRCOSC.App.Nodes.Types.VRChat;

[Node("Is VRChat Open")]
public sealed class VRChatIsOpenNode() : SimpleValueSourceNode<bool>(() => AppManager.GetInstance().VRChatClient.IsOpen, "Is Open");

[Node("VRChat State Source", "VRChat")]
public sealed class VRChatStateSourceNode : Node, IContinuousNode
{
    public int UpdateOffset => 0;

    public ValueOutput<bool> IsOpen = new();
    public ValueOutput<bool> IsLoggedIn = new();
    public ValueOutput<bool> IsInInstance = new();
    public ValueOutput<bool> IsInAvatar = new();

    protected override Task Process(IPulseContext c)
    {
        var client = AppManager.GetInstance().VRChatClient;
        IsOpen.Write(client.IsOpen, c);
        IsLoggedIn.Write(client.IsLoggedIn, c);
        IsInInstance.Write(client.IsInInstance, c);
        IsInAvatar.Write(client.IsInAvatar, c);
        return Task.CompletedTask;
    }
}

[Node("User Source", "VRChat")]
public sealed class VRChatUserSourceNode() : ValueSourceNode<User?>("User")
{
    protected override User? ComputeValue(IPulseContext c) => AppManager.GetInstance().VRChatClient.User;
}

[Node("Avatar Source", "VRChat")]
public sealed class VRChatAvatarSourceNode() : ValueSourceNode<Avatar?>("Avatar")
{
    protected override Avatar? ComputeValue(IPulseContext c) => AppManager.GetInstance().VRChatClient.Avatar;
}

[Node("Instance Source", "VRChat")]
public sealed class VRChatInstanceSourceNode() : ValueSourceNode<Instance?>("Instance")
{
    protected override Instance? ComputeValue(IPulseContext c) => AppManager.GetInstance().VRChatClient.Instance;
}

[Node("Unpack User", "VRChat/Structs/User")]
public sealed class UserUnpackNode : Node
{
    public ValueInput<User?> User = new();
    public ValueOutput<string> UserId = new("Id");
    public ValueOutput<string> Username = new();

    protected override Task Process(IPulseContext c)
    {
        var user = User.Read(c);
        if (user is null) return Task.CompletedTask;

        UserId.Write(user.Id, c);
        Username.Write(user.Username, c);
        return Task.CompletedTask;
    }
}

[Node("Pack User", "VRChat/Structs/User")]
public sealed class UserPackNode : ValueComputeNode<User?>
{
    public ValueInput<string?> UserId = new("Id");
    public ValueInput<string?> Username = new("Username");

    protected override User? ComputeValue(IPulseContext c)
    {
        var userId = UserId.Read(c);
        var username = Username.Read(c) ?? string.Empty;
        return userId is null ? null : new User(userId, username);
    }
}

[Node("Unpack Instance", "VRChat/Structs/Instance")]
[NodeForceReprocess]
public sealed class InstanceUnpackNode : Node, IActiveUpdateNode
{
    public int UpdateOffset => 0;

    private readonly GlobalStore<IEnumerable<User>?> userStore = new();

    public ValueInput<Instance?> Instance = new();
    public ValueOutput<string?> InstanceId = new("Id");
    public ValueOutput<string?> OwnerId = new();
    public ValueOutput<string?> Number = new();
    public ValueOutput<InstanceType> Type = new();
    public ValueOutput<InstanceRegion> Region = new();
    public ValueOutput<bool> AgeGated = new();
    public ValueOutput<bool> HasQueue = new();
    public ValueOutput<World> World = new();
    public ValueOutput<IReadOnlyList<User>?> Users = new();

    protected override Task Process(IPulseContext c)
    {
        var instance = Instance.Read(c);
        if (instance is null) return Task.CompletedTask;

        InstanceId.Write(instance.Id, c);
        OwnerId.Write(instance.OwnerId, c);
        Number.Write(instance.Number, c);
        Type.Write(instance.Type, c);
        Region.Write(instance.Region, c);
        AgeGated.Write(instance.AgeGated, c);
        HasQueue.Write(instance.HasQueue, c);
        World.Write(instance.World, c);
        Users.Write(instance.Users.ToImmutableList(), c);
        return Task.CompletedTask;
    }

    public Task<bool> OnUpdate(IPulseContext c)
    {
        var instance = Instance.Read(c);
        var storedUsers = userStore.Read(c);
        var currentUsers = instance?.Users;

        if (storedUsers is null && currentUsers is null)
            return Task.FromResult(false);

        if (storedUsers is null || currentUsers is null)
        {
            userStore.Write(currentUsers?.ToList(), c);
            return Task.FromResult(true);
        }

        if (!storedUsers.ToHashSet().SetEquals(currentUsers))
        {
            userStore.Write(currentUsers.ToList(), c);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }
}

[Node("Unpack World", "VRChat/Structs/World")]
public sealed class WorldUnpackNode : Node
{
    public ValueInput<World?> World = new();
    public ValueOutput<string> WorldId = new("Id");
    public ValueOutput<string> Name = new();

    protected override Task Process(IPulseContext c)
    {
        var world = World.Read(c);
        if (world is null) return Task.CompletedTask;

        WorldId.Write(world.Id, c);
        Name.Write(world.Name, c);
        return Task.CompletedTask;
    }
}

[Node("Pack World", "VRChat/Structs/World")]
public sealed class WorldPackNode : ValueComputeNode<World?>
{
    public ValueInput<string?> WorldId = new("Id");
    public ValueInput<string?> Name = new();

    protected override World? ComputeValue(IPulseContext c)
    {
        var worldId = WorldId.Read(c);
        var name = Name.Read(c) ?? string.Empty;
        return worldId is null ? null : new World(worldId, name);
    }
}

[Node("Unpack Avatar", "VRChat/Structs/Avatar")]
public sealed class AvatarUnpackNode : Node
{
    public ValueInput<Avatar?> Avatar = new();
    public ValueOutput<string> AvatarId = new("Id");
    public ValueOutput<string> Name = new();
    public ValueOutput<IReadOnlyList<ParameterDefinition>> Parameters = new();

    protected override Task Process(IPulseContext c)
    {
        var avatar = Avatar.Read(c);
        if (avatar is null) return Task.CompletedTask;

        AvatarId.Write(avatar.Id, c);
        Name.Write(avatar.Name, c);
        Parameters.Write(avatar.Parameters.ToImmutableList(), c);
        return Task.CompletedTask;
    }
}

[Node("Pack Avatar", "VRChat/Structs/Avatar")]
public sealed class AvatarPackNode : ValueComputeNode<Avatar?>
{
    public ValueInput<string?> AvatarId = new("Id");
    public ValueInput<string?> Name = new();

    protected override Avatar? ComputeValue(IPulseContext c)
    {
        var avatarId = AvatarId.Read(c);
        var name = Name.Read(c) ?? string.Empty;
        return avatarId is null ? null : new Avatar(avatarId, name, []);
    }
}

[Node("Avatar Height", "VRChat/Avatar/Info")]
public sealed class AvatarHeightDataNode : Node, IContinuousNode
{
    public int UpdateOffset => 0;

    public ValueOutput<float> EyeHeight = new();
    public ValueOutput<float> EyeHeightMin = new();
    public ValueOutput<float> EyeHeightMax = new();
    public ValueOutput<bool> ScalingAllowed = new();

    protected override Task Process(IPulseContext c)
    {
        var avatar = AppManager.GetInstance().VRChatClient.Avatar;
        if (avatar is null) return Task.CompletedTask;

        EyeHeight.Write(avatar.EyeHeight, c);
        EyeHeightMin.Write(avatar.EyeHeightMin, c);
        EyeHeightMax.Write(avatar.EyeHeightMax, c);
        ScalingAllowed.Write(avatar.EyeHeightScalingAllowed, c);

        return Task.CompletedTask;
    }
}

[Node("Unpack Parameter Definition", "VRChat/Structs/Parameter Definition")]
public sealed class ParameterDefinitionUnpackNode : Node
{
    public ValueInput<ParameterDefinition?> Definition = new();
    public ValueOutput<string> Name = new();
    public ValueOutput<ParameterType> Type = new();

    protected override Task Process(IPulseContext c)
    {
        var definition = Definition.Read(c);
        if (definition is null) return Task.CompletedTask;

        Name.Write(definition.Name, c);
        Type.Write(definition.Type, c);
        return Task.CompletedTask;
    }
}

[Node("Pack Parameter Definition", "VRChat/Structs/Parameter Definition")]
public sealed class ParameterDefinitionPackNode : ValueComputeNode<ParameterDefinition?>
{
    public ValueInput<string?> Name = new();
    public ValueInput<ParameterType> Type = new();

    protected override ParameterDefinition? ComputeValue(IPulseContext c)
    {
        var name = Name.Read(c);
        var type = Type.Read(c);
        return name is null ? null : new ParameterDefinition(name, type);
    }
}

[Node("FPS", "VRChat")]
[NodeCollapsed]
public sealed class VRChatFPSNode() : SimpleValueSourceNode<int>(() => (int)double.Round(AppManager.GetInstance().VRChatClient.FPS, MidpointRounding.AwayFromZero), "FPS");