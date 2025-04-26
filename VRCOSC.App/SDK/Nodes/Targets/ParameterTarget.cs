// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.OSC.VRChat;

namespace VRCOSC.App.SDK.Nodes.Targets;

public class ParameterTarget<T> : ITarget<T>
{
    public string ParameterName { get; }

    public ParameterTarget(string parameterName)
    {
        ParameterName = parameterName;
    }

    public void SetValue(T value)
    {
        AppManager.GetInstance().VRChatOscClient.Send($"{VRChatOscConstants.ADDRESS_AVATAR_PARAMETERS_PREFIX}{ParameterName}", value);
    }
}