// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.OSC.Client;

namespace VRCOSC.OSC.VRChat;

public class VRChatOscClient : OscClient
{
    public Action<VRChatOscData>? OnParameterSent;
    public Action<VRChatOscData>? OnParameterReceived;

    protected override void OnDataSend(OscData data)
    {
        OnParameterSent?.Invoke(new VRChatOscData(data));
    }

    protected override void OnDataReceived(OscData data)
    {
        if (!data.Values.Any()) return;

        OnParameterReceived?.Invoke(new VRChatOscData(data));
    }
}
