// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.Nodes.Types.VRChat;

[Node("Set Avatar Eye Height", "VRChat/Avatar/Actions")]
public sealed class AvatarSetEyeHeightNode : ActionNode
{
    public ValueInput<float> Meters = new();

    protected override void DoAction(IPulseContext c)
    {
        var client = AppManager.GetInstance().VRChatClient;
        if (!client.IsInAvatar) return;

        client.Avatar.SetEyeHeight(Meters.Read(c));
    }
}