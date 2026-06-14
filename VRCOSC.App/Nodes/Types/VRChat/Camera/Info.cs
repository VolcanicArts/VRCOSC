// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;
using VRCOSC.App.SDK.VRChat;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.VRChat.Camera;

[Node("User Camera Mask", "VRChat/User Camera/Info")]
public sealed class UserCameraMaskSourceNode() : ValueSourceNode<UserCameraMask>("Mask")
{
    protected override UserCameraMask ComputeValue(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.Mask;
}

[Node("User Camera Locked", "VRChat/User Camera/Info")]
public sealed class UserCameraLockedSourceNode() : ValueSourceNode<bool>("Locked")
{
    protected override bool ComputeValue(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.IsLocked;
}

[Node("User Camera Smoothing", "VRChat/User Camera/Info")]
public sealed class UserCameraSmoothingSourceNode : Node, IContinuousNode
{
    public int UpdateOffset => 0;

    public ValueOutput<bool> Enabled = new();
    public ValueOutput<float> Strength = new();

    protected override Task Process(IPulseContext c)
    {
        var uc = AppManager.GetInstance().VRChatClient.UserCamera;
        Enabled.Write(uc.SmoothMovement, c);
        Strength.Write(uc.SmoothingStrength, c);
        return Task.CompletedTask;
    }
}

[Node("User Camera Direction", "VRChat/User Camera/Info")]
public sealed class UserCameraDirectionSourceNode : Node, IContinuousNode
{
    public int UpdateOffset => 0;

    public ValueOutput<UserCameraDirection> Direction = new();
    public ValueOutput<Vector2> UserDirectionOffset = new();

    protected override Task Process(IPulseContext c)
    {
        var uc = AppManager.GetInstance().VRChatClient.UserCamera;
        Direction.Write(uc.Direction, c);
        UserDirectionOffset.Write(uc.UserDirectionOffset, c);
        return Task.CompletedTask;
    }
}

[Node("User Camera Auto Level", "VRChat/User Camera/Info")]
public sealed class UserCameraAutoLevelSourceNode() : ValueSourceNode<UserCameraAutoLevel>("Flags")
{
    protected override UserCameraAutoLevel ComputeValue(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.AutoLevel;
}

[Node("User Camera Flying", "VRChat/User Camera/Info")]
public sealed class UserCameraFlyingSourceNode : Node, IContinuousNode
{
    public int UpdateOffset => 0;

    public ValueOutput<bool> IsFlying = new();
    public ValueOutput<float> Speed = new();
    public ValueOutput<bool> CanRoll = new();

    protected override Task Process(IPulseContext c)
    {
        var uc = AppManager.GetInstance().VRChatClient.UserCamera;
        IsFlying.Write(uc.IsFlying, c);
        Speed.Write(uc.FlySpeed, c);
        CanRoll.Write(uc.RollWhileFlying, c);
        return Task.CompletedTask;
    }
}

[Node("User Camera Toggles", "VRChat/User Camera/Info")]
public sealed class UserCameraTogglesSourceNode : Node, IContinuousNode
{
    public int UpdateOffset => 0;

    public ValueOutput<bool> TriggerTakesPhotos = new();
    public ValueOutput<bool> DollyPathsStayVisible = new();
    public ValueOutput<bool> ShowFocus = new();
    public ValueOutput<bool> IsStreaming = new();

    protected override Task Process(IPulseContext c)
    {
        var uc = AppManager.GetInstance().VRChatClient.UserCamera;
        TriggerTakesPhotos.Write(uc.TriggerTakesPhotos, c);
        DollyPathsStayVisible.Write(uc.DollyPathsStayVisible, c);
        ShowFocus.Write(uc.ShowFocus, c);
        IsStreaming.Write(uc.IsStreaming, c);
        return Task.CompletedTask;
    }
}

[Node("User Camera Lens", "VRChat/User Camera/Info")]
public sealed class UserCameraLensSourceNode : Node, IContinuousNode
{
    public int UpdateOffset => 0;

    public ValueOutput<float> Zoom = new();
    public ValueOutput<float> Exposure = new();
    public ValueOutput<float> FocalDistance = new();
    public ValueOutput<float> Aperture = new();

    protected override Task Process(IPulseContext c)
    {
        var uc = AppManager.GetInstance().VRChatClient.UserCamera;
        Zoom.Write(uc.Zoom, c);
        Exposure.Write(uc.Exposure, c);
        FocalDistance.Write(uc.FocalDistance, c);
        Aperture.Write(uc.Aperture, c);
        return Task.CompletedTask;
    }
}

[Node("User Camera Turn Speed", "VRChat/User Camera/Info")]
public sealed class UserCameraTurnSpeedSourceNode() : ValueSourceNode<float>("Turn Speed")
{
    protected override float ComputeValue(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.TurnSpeed;
}

[Node("User Camera Photo Rate", "VRChat/User Camera/Info")]
public sealed class UserCameraPhotoRateSourceNode() : ValueSourceNode<float>("Photo Rate")
{
    protected override float ComputeValue(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.PhotoRate;
}

[Node("User Camera Duration", "VRChat/User Camera/Info")]
public sealed class UserCameraDurationSourceNode() : ValueSourceNode<float>("Duration")
{
    protected override float ComputeValue(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.Duration;
}

[Node("User Camera Mode", "VRChat/User Camera/Info")]
public sealed class UserCameraModeSourceNode() : ValueSourceNode<UserCameraMode>("Mode")
{
    protected override UserCameraMode ComputeValue(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.Mode;
}

[Node("User Camera Transform", "VRChat/User Camera/Info")]
public sealed class UserCameraTransformSourceNode() : ValueSourceNode<Transform>("Transform")
{
    protected override Transform ComputeValue(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.Transform;
}

[Node("User Camera GreenScreen Background", "VRChat/User Camera/Info")]
public sealed class UserCameraGreenScreenBackgroundSourceNode() : ValueSourceNode<ColorHSL>("Color")
{
    protected override ColorHSL ComputeValue(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.GreenScreenBackground;
}

[Node("User Camera Orientation", "VRChat/User Camera/Info")]
public sealed class UserCameraOrientationSourceNode : ValueSourceNode<UserCameraOrientation>
{
    protected override UserCameraOrientation ComputeValue(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.Orientation;
}