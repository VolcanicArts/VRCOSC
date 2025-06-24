// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.SDK.VRChat;

namespace VRCOSC.App.Nodes.Types.Sources;

[Node("Avatar Source", "Sources")]
public sealed class AvatarSourceNode : Node, INodeEventHandler
{
    public ValueOutput<string> AvatarId = new("Id");
    public ValueOutput<string> Name = new();

    protected override Task Process(PulseContext c)
    {
        var avatar = c.FindCurrentAvatar();

        if (avatar is null)
        {
            AvatarId.Write(null!, c);
            Name.Write(string.Empty, c);
        }
        else
        {
            AvatarId.Write(avatar.Id, c);
            Name.Write(avatar.Name, c);
        }

        return Task.CompletedTask;
    }

    public bool HandleAvatarChange(PulseContext c, AvatarConfig? config) => true;
}