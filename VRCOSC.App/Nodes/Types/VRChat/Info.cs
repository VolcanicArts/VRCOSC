// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VRCOSC.App.SDK.VRChat;

namespace VRCOSC.App.Nodes.Types.VRChat;

[Node("Is VRChat Open", "VRChat")]
public sealed class VRChatIsOpenNode : UpdateNode<bool>
{
    public ValueOutput<bool> IsOpen = new("Is Open");

    protected override Task Process(PulseContext c)
    {
        IsOpen.Write(AppManager.GetInstance().VRChatClient.LastKnownOpenState, c);
        return Task.CompletedTask;
    }

    protected override bool GetValue(PulseContext c) => AppManager.GetInstance().VRChatClient.LastKnownOpenState;
}

[Node("User Source", "VRChat")]
public sealed class VRChatUserSourceNode : UpdateNode<User>
{
    public ValueOutput<User> User = new();

    protected override Task Process(PulseContext c)
    {
        User.Write(c.GetPlayer().User, c);
        return Task.CompletedTask;
    }

    protected override User GetValue(PulseContext c) => c.GetPlayer().User;
}

[Node("Avatar Source", "VRChat")]
public sealed class VRChatAvatarSourceNode : Node, INodeEventHandler
{
    public ValueOutput<string?> AvatarId = new("Id");
    public ValueOutput<string> Name = new();

    protected override async Task Process(PulseContext c)
    {
        var avatar = await c.GetCurrentAvatar();
        AvatarId.Write(avatar?.Id, c);
        Name.Write(avatar?.Name ?? string.Empty, c);
    }

    public bool HandleAvatarChange(PulseContext c, AvatarConfig? config) => true;
}

[Node("Instance Source", "VRChat")]
public sealed class VRChatInstanceSourceNode : UpdateNode<string?, int>, INodeEventHandler
{
    public ValueOutput<string?> WorldId = new("World Id");
    public ValueOutput<IEnumerable<User>> Users = new();

    protected override Task Process(PulseContext c)
    {
        var instance = c.GetInstance();

        WorldId.Write(instance.WorldId, c);
        Users.Write(instance.Users.ToList(), c);
        return Task.CompletedTask;
    }

    protected override (string?, int) GetValues(PulseContext c) => (c.GetInstance().WorldId, c.GetInstance().Users.Count);
}

[Node("User To Data", "VRChat")]
public sealed class UserToDataNode : Node
{
    public ValueInput<User> User = new();
    public ValueOutput<string> UserId = new("Id");
    public ValueOutput<string> Username = new("Username");

    protected override Task Process(PulseContext c)
    {
        var user = User.Read(c);
        UserId.Write(user.UserId, c);
        Username.Write(user.Username, c);
        return Task.CompletedTask;
    }
}