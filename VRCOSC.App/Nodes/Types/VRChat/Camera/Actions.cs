// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;
using VRCOSC.App.SDK.VRChat;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.VRChat.Camera;

[Node("User Camera Close", "VRChat/User Camera/Actions")]
public sealed class UserCameraCloseNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.Close();
        await Next.Execute(c);
    }
}

[Node("User Camera Capture", "VRChat/User Camera/Actions")]
public sealed class UserCameraCaptureNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.Capture();
        await Next.Execute(c);
    }
}

[Node("User Camera Capture Delayed", "VRChat/User Camera/Actions")]
public sealed class UserCameraCaptureDelayedNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.CaptureDelayed();
        await Next.Execute(c);
    }
}

[Node("User Camera Set Mode", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetModeNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<UserCameraMode> Mode = new();

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetMode(Mode.Read(c));
        await Next.Execute(c);
    }
}

[Node("User Camera Set Zoom", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetZoomNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<float> Zoom = new();

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetZoom(float.Clamp(Zoom.Read(c), 0f, 3f));
        await Next.Execute(c);
    }
}

[Node("User Camera Set GreenScreen Background", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetGreenScreenBackgroundNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<ColorHSL> Color = new();

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetGreenScreenBackground(Color.Read(c));
        await Next.Execute(c);
    }
}

[Node("User Camera Set Orientation", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetOrientationNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<UserCameraOrientation> Orientation = new();

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetOrientation(Orientation.Read(c));
        await Next.Execute(c);
    }
}

[Node("User Camera Set Direction", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetDirectionNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<UserCameraDirection> Direction = new();

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetDirection(Direction.Read(c));
        await Next.Execute(c);
    }
}

[Node("User Camera Set Focal Distance", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetFocalDistanceNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<float> FocalDistance = new("Focal Distance");

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetFocalDistance(float.Clamp(FocalDistance.Read(c), 0f, 10f));
        await Next.Execute(c);
    }
}

[Node("User Camera Set Focal Distance", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetApertureNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<float> Aperture = new();

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetAperture(float.Clamp(Aperture.Read(c), 1.4f, 32f));
        await Next.Execute(c);
    }
}

[Node("User Camera Set Streaming", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetStreamingNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<bool> Streaming = new();

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetStreaming(Streaming.Read(c));
        await Next.Execute(c);
    }
}

[Node("User Camera Set Exposure", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetExposureNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<float> Exposure = new();

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetExposure(float.Clamp(Exposure.Read(c), -10f, 4f));
        await Next.Execute(c);
    }
}

[Node("User Camera Set Mask", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetMaskNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<UserCameraMask> Mask = new();

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetMask(Mask.Read(c));
        await Next.Execute(c);
    }
}

[Node("User Camera Set Transform", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetTransformNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<Transform> Transform = new();

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetPose(Transform.Read(c));
        await Next.Execute(c);
    }
}

[Node("User Camera Set Auto Level", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetAutoLevelNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<UserCameraAutoLevel> Flags = new();

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetAutoLevel(Flags.Read(c));
        await Next.Execute(c);
    }
}

[Node("User Camera Set User Direction Offset", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetUserDirectionOffsetNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<Vector2> Offset = new();

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetUserDirectionOffset(Offset.Read(c));
        await Next.Execute(c);
    }
}

[Node("User Camera Set Smoothing Enabled", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetSmoothingEnabledNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<bool> Enabled = new();

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetSmoothMovement(Enabled.Read(c));
        await Next.Execute(c);
    }
}

[Node("User Camera Set Smoothing Strength", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetSmoothingStrengthNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<float> Strength = new();

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetSmoothingStrength(Strength.Read(c));
        await Next.Execute(c);
    }
}

[Node("User Camera Set Audio Source", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetAudioSourceNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<UserCameraAudioSource> Source = new("Source");

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetAudioSource(Source.Read(c));
        await Next.Execute(c);
    }
}

[Node("User Camera Set Trigger Takes Photos", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetTriggerTakesPhotosNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<bool> Value = new();

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetTriggerTakesPhotos(Value.Read(c));
        await Next.Execute(c);
    }
}

[Node("User Camera Set Dolly Paths Stay Visible", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetDollyPathsStayVisibleNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<bool> Value = new();

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetDollyPathsStayVisible(Value.Read(c));
        await Next.Execute(c);
    }
}

[Node("User Camera Set Show Focus", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetShowFocusNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<bool> Value = new();

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetShowFocus(Value.Read(c));
        await Next.Execute(c);
    }
}

[Node("User Camera Set Fly Enabled", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetFlyEnabledNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<bool> Enabled = new();

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetFlying(Enabled.Read(c));
        await Next.Execute(c);
    }
}

[Node("User Camera Set Fly Speed", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetFlySpeedNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<float> Speed = new();

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetFlySpeed(Speed.Read(c));
        await Next.Execute(c);
    }
}

[Node("User Camera Set Fly Roll", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetFlyRollNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<bool> Enabled = new();

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetRollWhileFlying(Enabled.Read(c));
        await Next.Execute(c);
    }
}

[Node("User Camera Set Turn Speed", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetTurnSpeedNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<float> Speed = new();

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetTurnSpeed(Speed.Read(c));
        await Next.Execute(c);
    }
}

[Node("User Camera Set Duration", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetDurationNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<float> Duration = new();

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetDuration(Duration.Read(c));
        await Next.Execute(c);
    }
}

[Node("User Camera Set Photo Rate", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetPhotoRateNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<float> Rate = new();

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetPhotoRate(Rate.Read(c));
        await Next.Execute(c);
    }
}

[Node("User Camera Set Locked", "VRChat/User Camera/Actions")]
public sealed class UserCameraSetLockedNode : Node, IFlowInput
{
    public FlowContinuation Next = new("Next");

    public ValueInput<bool> Locked = new();

    protected override async Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        uc.SetLock(Locked.Read(c));
        await Next.Execute(c);
    }
}