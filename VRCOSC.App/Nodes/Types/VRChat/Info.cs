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
public sealed class VRChatStateSourceNode : UpdateNode<bool, bool, bool, bool>
{
    public ValueOutput<bool> IsOpen = new();
    public ValueOutput<bool> IsLoggedIn = new();
    public ValueOutput<bool> IsInInstance = new();
    public ValueOutput<bool> IsInAvatar = new();

    protected override Task Process(PulseContext c)
    {
        var client = c.GetClient();
        IsOpen.Write(client.IsOpen, c);
        IsLoggedIn.Write(client.IsLoggedIn, c);
        IsInInstance.Write(client.IsInInstance, c);
        IsInAvatar.Write(client.IsInAvatar, c);
        return Task.CompletedTask;
    }

    protected override Task<(bool, bool, bool, bool)> GetValues(PulseContext c)
    {
        var client = c.GetClient();
        return Task.FromResult((LastKnownOpenState: client.IsOpen, client.IsLoggedIn, client.IsInInstance, client.IsInAvatar));
    }
}

[Node("User Source", "VRChat")]
public sealed class VRChatUserSourceNode : UpdateNode<User?>
{
    public ValueOutput<User?> User = new();

    protected override Task Process(PulseContext c)
    {
        User.Write(c.GetClient().User, c);
        return Task.CompletedTask;
    }

    protected override Task<User?> GetValue(PulseContext c) => Task.FromResult(c.GetClient().User);
}

[Node("Avatar Source", "VRChat")]
public sealed class VRChatAvatarSourceNode : UpdateNode<Avatar?>
{
    public ValueOutput<Avatar?> Avatar = new();

    protected override Task Process(PulseContext c)
    {
        Avatar.Write(c.GetClient().Avatar, c);
        return Task.CompletedTask;
    }

    protected override Task<Avatar?> GetValue(PulseContext c) => Task.FromResult(c.GetClient().Avatar);
}

[Node("Instance Source", "VRChat")]
public sealed class VRChatInstanceSourceNode : UpdateNode<Instance?>
{
    public ValueOutput<Instance?> Instance = new();

    protected override Task Process(PulseContext c)
    {
        Instance.Write(c.GetClient().Instance, c);
        return Task.CompletedTask;
    }

    protected override Task<Instance?> GetValue(PulseContext c) => Task.FromResult(c.GetClient().Instance);
}

[Node("Unpack User", "VRChat/Structs/User")]
public sealed class UserUnpackNode : Node
{
    public ValueInput<User?> User = new();
    public ValueOutput<string> UserId = new("Id");
    public ValueOutput<string> Username = new();

    protected override Task Process(PulseContext c)
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

    protected override User? ComputeValue(PulseContext c)
    {
        var userId = UserId.Read(c);
        var username = Username.Read(c) ?? string.Empty;
        return userId is null ? null : new User(userId, username);
    }
}

[Node("Unpack Instance", "VRChat/Structs/Instance")]
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

    protected override Task Process(PulseContext c)
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

    public Task<bool> OnUpdate(PulseContext c)
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

    protected override Task Process(PulseContext c)
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

    protected override World? ComputeValue(PulseContext c)
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

    protected override Task Process(PulseContext c)
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

    protected override Avatar? ComputeValue(PulseContext c)
    {
        var avatarId = AvatarId.Read(c);
        var name = Name.Read(c) ?? string.Empty;
        return avatarId is null ? null : new Avatar(avatarId, name, []);
    }
}

[Node("Avatar Height", "VRChat/Avatar/Info")]
public sealed class AvatarHeightDataNode : UpdateNode<float, float, float, bool>
{
    public ValueOutput<float> EyeHeight = new();
    public ValueOutput<float> EyeHeightMin = new();
    public ValueOutput<float> EyeHeightMax = new();
    public ValueOutput<bool> ScalingAllowed = new();

    protected override Task Process(PulseContext c)
    {
        var avatar = AppManager.GetInstance().VRChatClient.Avatar;
        if (avatar is null) return Task.CompletedTask;

        EyeHeight.Write(avatar.EyeHeight, c);
        EyeHeightMin.Write(avatar.EyeHeightMin, c);
        EyeHeightMax.Write(avatar.EyeHeightMax, c);
        ScalingAllowed.Write(avatar.EyeHeightScalingAllowed, c);

        return Task.CompletedTask;
    }

    protected override Task<(float, float, float, bool)> GetValues(PulseContext c)
    {
        var avatar = AppManager.GetInstance().VRChatClient.Avatar;
        if (avatar is null) return Task.FromResult((0f, 0f, 0f, false));

        return Task.FromResult((avatar.EyeHeight, avatar.EyeHeightMin, avatar.EyeHeightMax, avatar.EyeHeightScalingAllowed));
    }
}

[Node("Unpack Parameter Definition", "VRChat/Structs/Parameter Definition")]
public sealed class ParameterDefinitionUnpackNode : Node
{
    public ValueInput<ParameterDefinition?> Definition = new();
    public ValueOutput<string> Name = new();
    public ValueOutput<ParameterType> Type = new();

    protected override Task Process(PulseContext c)
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

    protected override ParameterDefinition? ComputeValue(PulseContext c)
    {
        var name = Name.Read(c);
        var type = Type.Read(c);
        return name is null ? null : new ParameterDefinition(name, type);
    }
}

[Node("FPS", "VRChat")]
[NodeCollapsed]
public sealed class VRChatFPSNode : UpdateNode<int>
{
    public ValueOutput<int> FPS = new();

    protected override Task Process(PulseContext c)
    {
        FPS.Write((int)double.Round(c.GetClient().FPS, MidpointRounding.AwayFromZero), c);
        return Task.CompletedTask;
    }

    protected override Task<int> GetValue(PulseContext c) => Task.FromResult((int)double.Round(c.GetClient().FPS, MidpointRounding.AwayFromZero));
}