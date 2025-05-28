// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Sources;

[Node("Avatar Source", "Sources")]
public class AvatarSourceNode : Node
{
    public ValueOutput<string> AvatarId = new();
    public ValueOutput<string> Name = new();

    protected override void Process(PulseContext c)
    {
        var avatar = c.FindCurrentAvatar();
        if (avatar is null) return;

        AvatarId.Write(avatar.Id, c);
        Name.Write(avatar.Name, c);
    }
}