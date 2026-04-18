// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using VRCOSC.App.SDK.VRChat;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.VRChat.Camera;

[Node("User Camera Close", "VRChat/User Camera/Actions")]
public sealed class UserCameraCloseNode : SimpleActionNode
{
    protected override void DoAction(PulseContext c) => c.GetUserCamera().Close();
}

[Node("User Camera Capture", "VRChat/User Camera/Actions")]
public sealed class UserCameraCaptureNode : SimpleActionNode
{
    protected override void DoAction(PulseContext c) => c.GetUserCamera().Capture();
}

[Node("User Camera Capture Delayed", "VRChat/User Camera/Actions")]
public sealed class UserCameraCaptureDelayedNode : SimpleActionNode
{
    protected override void DoAction(PulseContext c) => c.GetUserCamera().CaptureDelayed();
}

[Node("User Camera Set Mode", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetModeNode : SimpleActionNode
{
    public ValueInput<UserCameraMode> Mode = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetMode(Mode.Read(c));
}

[Node("User Camera Set Zoom", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetZoomNode : SimpleActionNode
{
    public ValueInput<float> Zoom = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetZoom(float.Clamp(Zoom.Read(c), 0f, 3f));
}

[Node("User Camera Set GreenScreen Background", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetGreenScreenBackgroundNode : SimpleActionNode
{
    public ValueInput<ColorHSL> Color = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetGreenScreenBackground(Color.Read(c));
}

[Node("User Camera Set Orientation", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetOrientationNode : SimpleActionNode
{
    public ValueInput<UserCameraOrientation> Orientation = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetOrientation(Orientation.Read(c));
}

[Node("User Camera Set Direction", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetDirectionNode : SimpleActionNode
{
    public ValueInput<UserCameraDirection> Direction = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetDirection(Direction.Read(c));
}

[Node("User Camera Set Focal Distance", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetFocalDistanceNode : SimpleActionNode
{
    public ValueInput<float> FocalDistance = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetFocalDistance(float.Clamp(FocalDistance.Read(c), 0f, 10f));
}

[Node("User Camera Set Aperture", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetApertureNode : SimpleActionNode
{
    public ValueInput<float> Aperture = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetAperture(float.Clamp(Aperture.Read(c), 1.4f, 32f));
}

[Node("User Camera Set Streaming", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetStreamingNode : SimpleActionNode
{
    public ValueInput<bool> Streaming = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetStreaming(Streaming.Read(c));
}

[Node("User Camera Set Exposure", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetExposureNode : SimpleActionNode
{
    public ValueInput<float> Exposure = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetExposure(float.Clamp(Exposure.Read(c), -10f, 4f));
}

[Node("User Camera Set Mask", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetMaskNode : SimpleActionNode
{
    public ValueInput<UserCameraMask> Mask = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetMask(Mask.Read(c));
}

[Node("User Camera Set Transform", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetTransformNode : SimpleActionNode
{
    public ValueInput<Transform> Transform = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetPose(Transform.Read(c));
}

[Node("User Camera Set Auto Level", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetAutoLevelNode : SimpleActionNode
{
    public ValueInput<UserCameraAutoLevel> Flags = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetAutoLevel(Flags.Read(c));
}

[Node("User Camera Set User Direction Offset", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetUserDirectionOffsetNode : SimpleActionNode
{
    public ValueInput<Vector2> Offset = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetUserDirectionOffset(Offset.Read(c));
}

[Node("User Camera Set Smoothing Enabled", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetSmoothingEnabledNode : SimpleActionNode
{
    public ValueInput<bool> Enabled = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetSmoothMovement(Enabled.Read(c));
}

[Node("User Camera Set Smoothing Strength", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetSmoothingStrengthNode : SimpleActionNode
{
    public ValueInput<float> Strength = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetSmoothingStrength(Strength.Read(c));
}

[Node("User Camera Set Audio Source", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetAudioSourceNode : SimpleActionNode
{
    public ValueInput<UserCameraAudioSource> Source = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetAudioSource(Source.Read(c));
}

[Node("User Camera Set Trigger Takes Photos", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetTriggerTakesPhotosNode : SimpleActionNode
{
    public ValueInput<bool> Value = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetTriggerTakesPhotos(Value.Read(c));
}

[Node("User Camera Set Dolly Paths Stay Visible", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetDollyPathsStayVisibleNode : SimpleActionNode
{
    public ValueInput<bool> Value = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetDollyPathsStayVisible(Value.Read(c));
}

[Node("User Camera Set Show Focus", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetShowFocusNode : SimpleActionNode
{
    public ValueInput<bool> Value = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetShowFocus(Value.Read(c));
}

[Node("User Camera Set Fly Enabled", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetFlyEnabledNode : SimpleActionNode
{
    public ValueInput<bool> Enabled = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetFlying(Enabled.Read(c));
}

[Node("User Camera Set Fly Speed", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetFlySpeedNode : SimpleActionNode
{
    public ValueInput<float> Speed = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetFlySpeed(Speed.Read(c));
}

[Node("User Camera Set Fly Roll", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetFlyRollNode : SimpleActionNode
{
    public ValueInput<bool> Enabled = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetRollWhileFlying(Enabled.Read(c));
}

[Node("User Camera Set Turn Speed", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetTurnSpeedNode : SimpleActionNode
{
    public ValueInput<float> Speed = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetTurnSpeed(Speed.Read(c));
}

[Node("User Camera Set Duration", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetDurationNode : SimpleActionNode
{
    public ValueInput<float> Duration = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetDuration(Duration.Read(c));
}

[Node("User Camera Set Photo Rate", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetPhotoRateNode : SimpleActionNode
{
    public ValueInput<float> Rate = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetPhotoRate(Rate.Read(c));
}

[Node("User Camera Set Locked", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetLockedNode : SimpleActionNode
{
    public ValueInput<bool> Locked = new();

    protected override void DoAction(PulseContext c) => c.GetUserCamera().SetLock(Locked.Read(c));
}