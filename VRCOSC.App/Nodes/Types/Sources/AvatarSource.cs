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

    protected override async Task Process(PulseContext c)
    {
        var avatar = await c.GetCurrentAvatar();
        AvatarId.Write(avatar?.Id ?? null!, c);
        Name.Write(avatar?.Name ?? string.Empty, c);
    }

    public bool HandleAvatarChange(PulseContext c, AvatarConfig? config) => true;
}