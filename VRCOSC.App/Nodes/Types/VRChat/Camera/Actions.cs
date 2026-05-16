// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using VRCOSC.App.SDK.VRChat;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.VRChat.Camera;

[Node("User Camera Close", "VRChat/User Camera/Actions")]
public sealed class UserCameraCloseNode : ActionNode
{
    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.Close();
}

[Node("User Camera Capture", "VRChat/User Camera/Actions")]
public sealed class UserCameraCaptureNode : ActionNode
{
    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.Capture();
}

[Node("User Camera Capture Delayed", "VRChat/User Camera/Actions")]
public sealed class UserCameraCaptureDelayedNode : ActionNode
{
    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.CaptureDelayed();
}

[Node("User Camera Set Mode", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetModeNode : ActionNode
{
    public ValueInput<UserCameraMode> Mode = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetMode(Mode.Read(c));
}

[Node("User Camera Set Zoom", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetZoomNode : ActionNode
{
    public ValueInput<float> Zoom = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetZoom(float.Clamp(Zoom.Read(c), 0f, 3f));
}

[Node("User Camera Set GreenScreen Background", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetGreenScreenBackgroundNode : ActionNode
{
    public ValueInput<ColorHSL> Color = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetGreenScreenBackground(Color.Read(c));
}

[Node("User Camera Set Orientation", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetOrientationNode : ActionNode
{
    public ValueInput<UserCameraOrientation> Orientation = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetOrientation(Orientation.Read(c));
}

[Node("User Camera Set Direction", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetDirectionNode : ActionNode
{
    public ValueInput<UserCameraDirection> Direction = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetDirection(Direction.Read(c));
}

[Node("User Camera Set Focal Distance", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetFocalDistanceNode : ActionNode
{
    public ValueInput<float> FocalDistance = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetFocalDistance(float.Clamp(FocalDistance.Read(c), 0f, 10f));
}

[Node("User Camera Set Aperture", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetApertureNode : ActionNode
{
    public ValueInput<float> Aperture = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetAperture(float.Clamp(Aperture.Read(c), 1.4f, 32f));
}

[Node("User Camera Set Streaming", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetStreamingNode : ActionNode
{
    public ValueInput<bool> Streaming = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetStreaming(Streaming.Read(c));
}

[Node("User Camera Set Exposure", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetExposureNode : ActionNode
{
    public ValueInput<float> Exposure = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetExposure(float.Clamp(Exposure.Read(c), -10f, 4f));
}

[Node("User Camera Set Mask", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetMaskNode : ActionNode
{
    public ValueInput<UserCameraMask> Mask = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetMask(Mask.Read(c));
}

[Node("User Camera Set Transform", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetTransformNode : ActionNode
{
    public ValueInput<Transform> Transform = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetPose(Transform.Read(c));
}

[Node("User Camera Set Auto Level", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetAutoLevelNode : ActionNode
{
    public ValueInput<UserCameraAutoLevel> Flags = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetAutoLevel(Flags.Read(c));
}

[Node("User Camera Set User Direction Offset", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetUserDirectionOffsetNode : ActionNode
{
    public ValueInput<Vector2> Offset = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetUserDirectionOffset(Offset.Read(c));
}

[Node("User Camera Set Smoothing Enabled", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetSmoothingEnabledNode : ActionNode
{
    public ValueInput<bool> Enabled = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetSmoothMovement(Enabled.Read(c));
}

[Node("User Camera Set Smoothing Strength", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetSmoothingStrengthNode : ActionNode
{
    public ValueInput<float> Strength = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetSmoothingStrength(Strength.Read(c));
}

[Node("User Camera Set Audio Source", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetAudioSourceNode : ActionNode
{
    public ValueInput<UserCameraAudioSource> Source = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetAudioSource(Source.Read(c));
}

[Node("User Camera Set Trigger Takes Photos", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetTriggerTakesPhotosNode : ActionNode
{
    public ValueInput<bool> Value = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetTriggerTakesPhotos(Value.Read(c));
}

[Node("User Camera Set Dolly Paths Stay Visible", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetDollyPathsStayVisibleNode : ActionNode
{
    public ValueInput<bool> Value = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetDollyPathsStayVisible(Value.Read(c));
}

[Node("User Camera Set Show Focus", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetShowFocusNode : ActionNode
{
    public ValueInput<bool> Value = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetShowFocus(Value.Read(c));
}

[Node("User Camera Set Fly Enabled", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetFlyEnabledNode : ActionNode
{
    public ValueInput<bool> Enabled = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetFlying(Enabled.Read(c));
}

[Node("User Camera Set Fly Speed", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetFlySpeedNode : ActionNode
{
    public ValueInput<float> Speed = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetFlySpeed(Speed.Read(c));
}

[Node("User Camera Set Fly Roll", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetFlyRollNode : ActionNode
{
    public ValueInput<bool> Enabled = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetRollWhileFlying(Enabled.Read(c));
}

[Node("User Camera Set Turn Speed", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetTurnSpeedNode : ActionNode
{
    public ValueInput<float> Speed = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetTurnSpeed(Speed.Read(c));
}

[Node("User Camera Set Duration", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetDurationNode : ActionNode
{
    public ValueInput<float> Duration = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetDuration(Duration.Read(c));
}

[Node("User Camera Set Photo Rate", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetPhotoRateNode : ActionNode
{
    public ValueInput<float> Rate = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetPhotoRate(Rate.Read(c));
}

[Node("User Camera Set Locked", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetLockedNode : ActionNode
{
    public ValueInput<bool> Locked = new();

    protected override void DoAction(IPulseContext c) => AppManager.GetInstance().VRChatClient.UserCamera.SetLock(Locked.Read(c));
}