// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.SDK.VRChat;

namespace VRCOSC.App.Nodes.Types.VRChat;

[Node("Set Face Parameter", "VRChat/Face Tracking")]
public sealed class SetFaceParameter : ActionNode
{
    public ValueInput<FaceParameter> FaceParameter = new();
    public ValueInput<float> Value = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.Player.SetFaceParameter(FaceParameter.Read(c), Value.Read(c));
}

[Node("Reset Face Parameters", "VRChat/Face Tracking")]
public sealed class ResetFaceParameters : ActionNode
{
    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.Player.ResetFaceParameters();
}