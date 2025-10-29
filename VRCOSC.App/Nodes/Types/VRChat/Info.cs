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

[Node("Instance Info", "VRChat")]
public sealed class VRChatInstanceInfo : UpdateNode<string?, int>, INodeEventHandler
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