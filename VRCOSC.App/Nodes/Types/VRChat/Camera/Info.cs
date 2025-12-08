// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Threading.Tasks;
using VRCOSC.App.SDK.VRChat;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.VRChat.Camera;

[Node("User Camera Mask", "VRChat/User Camera/Info")]
public sealed class UserCameraMaskSourceNode : UpdateNode<UserCameraMask>
{
    public ValueOutput<UserCameraMask> Mask = new();

    protected override Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        Mask.Write(uc.Mask, c);
        return Task.CompletedTask;
    }

    protected override Task<UserCameraMask> GetValue(PulseContext c)
    {
        var uc = c.GetUserCamera();
        return Task.FromResult(uc.Mask);
    }
}

[Node("User Camera Locked", "VRChat/User Camera/Info")]
public sealed class UserCameraLockedSourceNode : UpdateNode<bool>
{
    public ValueOutput<bool> Locked = new();

    protected override Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        Locked.Write(uc.IsLocked, c);
        return Task.CompletedTask;
    }

    protected override Task<bool> GetValue(PulseContext c)
    {
        var uc = c.GetUserCamera();
        return Task.FromResult(uc.IsLocked);
    }
}

[Node("User Camera Smoothing", "VRChat/User Camera/Info")]
public sealed class UserCameraSmoothingSourceNode : UpdateNode<bool, float>
{
    public ValueOutput<bool> Enabled = new();
    public ValueOutput<float> Strength = new();

    protected override Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        Enabled.Write(uc.SmoothMovement, c);
        Strength.Write(uc.SmoothingStrength, c);
        return Task.CompletedTask;
    }

    protected override Task<(bool, float)> GetValues(PulseContext c)
    {
        var uc = c.GetUserCamera();
        return Task.FromResult((uc.SmoothMovement, uc.SmoothingStrength));
    }
}

[Node("User Camera Direction", "VRChat/User Camera/Info")]
public sealed class UserCameraDirectionSourceNode : UpdateNode<UserCameraDirection, Vector2>
{
    public ValueOutput<UserCameraDirection> Direction = new();
    public ValueOutput<Vector2> UserDirectionOffset = new("User Direction Offset");

    protected override Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        Direction.Write(uc.Direction, c);
        UserDirectionOffset.Write(uc.UserDirectionOffset, c);
        return Task.CompletedTask;
    }

    protected override Task<(UserCameraDirection, Vector2)> GetValues(PulseContext c)
    {
        var uc = c.GetUserCamera();
        return Task.FromResult((uc.Direction, uc.UserDirectionOffset));
    }
}

[Node("User Camera Auto Level", "VRChat/User Camera/Info")]
public sealed class UserCameraAutoLevelSourceNode : UpdateNode<UserCameraAutoLevel>
{
    public ValueOutput<UserCameraAutoLevel> Flags = new();

    protected override Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        Flags.Write(uc.AutoLevel, c);
        return Task.CompletedTask;
    }

    protected override Task<UserCameraAutoLevel> GetValue(PulseContext c)
    {
        var uc = c.GetUserCamera();
        return Task.FromResult(uc.AutoLevel);
    }
}

[Node("User Camera Flying", "VRChat/User Camera/Info")]
public sealed class UserCameraFlyingSourceNode : UpdateNode<bool, float, bool>
{
    public ValueOutput<bool> IsFlying = new("Is Flying");
    public ValueOutput<float> Speed = new("Speed");
    public ValueOutput<bool> CanRoll = new("Can Roll");

    protected override Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        IsFlying.Write(uc.IsFlying, c);
        Speed.Write(uc.FlySpeed, c);
        CanRoll.Write(uc.RollWhileFlying, c);
        return Task.CompletedTask;
    }

    protected override Task<(bool, float, bool)> GetValues(PulseContext c)
    {
        var uc = c.GetUserCamera();
        return Task.FromResult((uc.IsFlying, uc.FlySpeed, uc.RollWhileFlying));
    }
}

[Node("User Camera Toggles", "VRChat/User Camera/Info")]
public sealed class UserCameraTogglesSourceNode : UpdateNode<bool, bool, bool, bool>
{
    public ValueOutput<bool> TriggerTakesPhotos = new("Trigger Takes Photos");
    public ValueOutput<bool> DollyPathsStayVisible = new("Dolly Paths Stay Visible");
    public ValueOutput<bool> ShowFocus = new("Show Focus");
    public ValueOutput<bool> IsStreaming = new("Is Streaming");

    protected override Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        TriggerTakesPhotos.Write(uc.TriggerTakesPhotos, c);
        DollyPathsStayVisible.Write(uc.DollyPathsStayVisible, c);
        ShowFocus.Write(uc.ShowFocus, c);
        IsStreaming.Write(uc.IsStreaming, c);
        return Task.CompletedTask;
    }

    protected override Task<(bool, bool, bool, bool)> GetValues(PulseContext c)
    {
        var uc = c.GetUserCamera();
        return Task.FromResult((uc.TriggerTakesPhotos, uc.DollyPathsStayVisible, uc.ShowFocus, uc.IsStreaming));
    }
}

[Node("User Camera Lens", "VRChat/User Camera/Info")]
public sealed class UserCameraLensSourceNode : UpdateNode<float, float, float, float>
{
    public ValueOutput<float> Zoom = new();
    public ValueOutput<float> Exposure = new();
    public ValueOutput<float> FocalDistance = new("Focal Distance");
    public ValueOutput<float> Aperture = new();

    protected override Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        Zoom.Write(uc.Zoom, c);
        Exposure.Write(uc.Exposure, c);
        FocalDistance.Write(uc.FocalDistance, c);
        Aperture.Write(uc.Aperture, c);
        return Task.CompletedTask;
    }

    protected override Task<(float, float, float, float)> GetValues(PulseContext c)
    {
        var uc = c.GetUserCamera();
        return Task.FromResult((uc.Zoom, uc.Exposure, uc.FocalDistance, uc.Aperture));
    }
}

[Node("User Camera Turn Speed", "VRChat/User Camera/Info")]
public sealed class UserCameraTurnSpeedSourceNode : UpdateNode<float>
{
    public ValueOutput<float> TurnSpeed = new("Turn Speed");

    protected override Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        TurnSpeed.Write(uc.TurnSpeed, c);
        return Task.CompletedTask;
    }

    protected override Task<float> GetValue(PulseContext c)
    {
        var uc = c.GetUserCamera();
        return Task.FromResult(uc.TurnSpeed);
    }
}

[Node("User Camera Photo Rate", "VRChat/User Camera/Info")]
public sealed class UserCameraPhotoRateSourceNode : UpdateNode<float>
{
    public ValueOutput<float> PhotoRate = new("Photo Rate");

    protected override Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        PhotoRate.Write(uc.PhotoRate, c);
        return Task.CompletedTask;
    }

    protected override Task<float> GetValue(PulseContext c)
    {
        var uc = c.GetUserCamera();
        return Task.FromResult(uc.PhotoRate);
    }
}

[Node("User Camera Duration", "VRChat/User Camera/Info")]
public sealed class UserCameraDurationSourceNode : UpdateNode<float>
{
    public ValueOutput<float> Duration = new();

    protected override Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        Duration.Write(uc.Duration, c);
        return Task.CompletedTask;
    }

    protected override Task<float> GetValue(PulseContext c)
    {
        var uc = c.GetUserCamera();
        return Task.FromResult(uc.Duration);
    }
}

[Node("User Camera Mode", "VRChat/User Camera/Info")]
public sealed class UserCameraModeSourceNode : UpdateNode<UserCameraMode>
{
    public ValueOutput<UserCameraMode> Mode = new();

    protected override Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        Mode.Write(uc.Mode, c);
        return Task.CompletedTask;
    }

    protected override Task<UserCameraMode> GetValue(PulseContext c)
    {
        var uc = c.GetUserCamera();
        return Task.FromResult(uc.Mode);
    }
}

[Node("User Camera Transform", "VRChat/User Camera/Info")]
public sealed class UserCameraTransformSourceNode : UpdateNode<Transform>
{
    public ValueOutput<Transform> Transform = new();

    protected override Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        Transform.Write(uc.Transform, c);
        return Task.CompletedTask;
    }

    protected override Task<Transform> GetValue(PulseContext c)
    {
        var uc = c.GetUserCamera();
        return Task.FromResult(uc.Transform);
    }
}

[Node("User Camera GreenScreen Background", "VRChat/User Camera/Info")]
public sealed class UserCameraGreenScreenBackgroundSourceNode : UpdateNode<ColorHSL>
{
    public ValueOutput<ColorHSL> Color = new();

    protected override Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        Color.Write(uc.GreenScreenBackground, c);
        return Task.CompletedTask;
    }

    protected override Task<ColorHSL> GetValue(PulseContext c)
    {
        var uc = c.GetUserCamera();
        return Task.FromResult(uc.GreenScreenBackground);
    }
}

[Node("User Camera Orientation", "VRChat/User Camera/Info")]
public sealed class UserCameraOrientationSourceNode : UpdateNode<UserCameraOrientation>
{
    public ValueOutput<UserCameraOrientation> Orientation = new();

    protected override Task Process(PulseContext c)
    {
        var uc = c.GetUserCamera();
        Orientation.Write(uc.Orientation, c);
        return Task.CompletedTask;
    }

    protected override Task<UserCameraOrientation> GetValue(PulseContext c)
    {
        var uc = c.GetUserCamera();
        return Task.FromResult(uc.Orientation);
    }
}