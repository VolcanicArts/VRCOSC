// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.SDK.Nodes;

namespace VRCOSC.App.Nodes.Types.Actions;

[Node("Indirect Send Parameter", "Actions")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public class IndirectSendParameterNode<T> : Node, IFlowInput where T : unmanaged
{
    public FlowContinuation Next = new("Next");

    public ValueInput<string> Name = new();
    public ValueInput<T> Value = new();

    protected override void Process(PulseContext c)
    {
        var name = Name.Read(c);
        if (string.IsNullOrEmpty(name)) return;

        AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOSCConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}{name}", Value.Read(c));
        Next.Execute(c);
    }
}

[Node("Direct Send Parameter", "Actions")]
[NodeGenericTypeFilter([typeof(bool), typeof(int), typeof(float)])]
public class DirectSendParameterNode<T> : Node, IFlowInput, IHasTextProperty where T : unmanaged
{
    [NodeProperty("text")]
    public string Text { get; set; } = string.Empty;

    public FlowContinuation Next = new("Next");

    public ValueInput<T> Value = new();

    protected override void Process(PulseContext c)
    {
        if (string.IsNullOrEmpty(Text)) return;

        AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOSCConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}{Text}", Value.Read(c));
        Next.Execute(c);
    }
}