// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;

namespace VRCOSC.App.Nodes.Types.Utility;

[Node("Is VRChat Open", "Utility")]
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