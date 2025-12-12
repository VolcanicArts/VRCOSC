// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.VRChat;

public class UserCamera
{
    private readonly VRChatOSCClient oscClient;

    public UserCameraMask Mask { get; private set; }
    public bool IsLocked { get; private set; }
    public bool SmoothMovement { get; private set; }
    public float SmoothingStrength { get; private set; }
    public UserCameraDirection Direction { get; private set; }
    public UserCameraAutoLevel AutoLevel { get; private set; }
    public bool IsFlying { get; private set; }
    public float FlySpeed { get; private set; }
    public bool RollWhileFlying { get; private set; }
    public bool TriggerTakesPhotos { get; private set; }
    public bool DollyPathsStayVisible { get; private set; }
    public bool ShowFocus { get; private set; }
    public UserCameraAudioSource AudioSource { get; private set; }
    public bool IsStreaming { get; private set; }
    public UserCameraOrientation Orientation { get; private set; }
    public float Zoom { get; private set; }
    public float Exposure { get; private set; }
    public float FocalDistance { get; private set; }
    public float Aperture { get; private set; }
    public ColorHSL GreenScreenBackground { get; private set; } = new(120, 1f, 0.5f);
    public Vector2 UserDirectionOffset { get; private set; } = Vector2.Zero;
    public float TurnSpeed { get; private set; }
    public float PhotoRate { get; private set; }
    public float Duration { get; private set; }
    public UserCameraMode Mode { get; private set; }
    public Transform Transform { get; private set; }

    public UserCamera(VRChatOSCClient oscClient)
    {
        this.oscClient = oscClient;
    }

    internal async Task RetrieveAllData()
    {
        Mask = await retrieveMask();
        IsLocked = await retrieveValue<bool>(VRChatCameraInput.Lock);
        SmoothMovement = await retrieveValue<bool>(VRChatCameraInput.SmoothMovement);
        Direction = await retrieveValue<bool>(VRChatCameraInput.LookAtMe) ? UserCameraDirection.User : UserCameraDirection.Back;

        if (await retrieveValue<bool>(VRChatCameraInput.AutoLevelPitch)) AutoLevel |= UserCameraAutoLevel.Pitch;
        if (await retrieveValue<bool>(VRChatCameraInput.AutoLevelRoll)) AutoLevel |= UserCameraAutoLevel.Roll;

        IsFlying = await retrieveValue<bool>(VRChatCameraInput.Flying);
        TriggerTakesPhotos = await retrieveValue<bool>(VRChatCameraInput.TriggerTakesPhotos);
        DollyPathsStayVisible = await retrieveValue<bool>(VRChatCameraInput.DollyPathsStayVisible);
        AudioSource = await retrieveValue<bool>(VRChatCameraInput.AudioFromCamera) ? UserCameraAudioSource.Camera : UserCameraAudioSource.User;
        ShowFocus = await retrieveValue<bool>(VRChatCameraInput.ShowFocus);
        IsStreaming = await retrieveValue<bool>(VRChatCameraInput.Streaming);
        RollWhileFlying = await retrieveValue<bool>(VRChatCameraInput.RollWhileFlying);
        Orientation = await retrieveValue<bool>(VRChatCameraInput.OrientationIsLandscape) ? UserCameraOrientation.Landscape : UserCameraOrientation.Portrait;
        Zoom = await retrieveZoom();
        Exposure = await retrieveValue<float>(VRChatCameraInput.Exposure);
        FocalDistance = await retrieveValue<float>(VRChatCameraInput.FocalDistance);
        Aperture = await retrieveValue<float>(VRChatCameraInput.Aperture);
        GreenScreenBackground = await retrieveGreenScreenBackground();
        UserDirectionOffset = new Vector2(await retrieveValue<float>(VRChatCameraInput.LookAtMeXOffset), await retrieveValue<float>(VRChatCameraInput.LookAtMeYOffset));
        FlySpeed = await retrieveValue<float>(VRChatCameraInput.FlySpeed);
        TurnSpeed = await retrieveValue<float>(VRChatCameraInput.TurnSpeed);
        SmoothingStrength = await retrieveValue<float>(VRChatCameraInput.SmoothingStrength);
        PhotoRate = await retrieveValue<float>(VRChatCameraInput.PhotoRate);
        Duration = await retrieveValue<float>(VRChatCameraInput.Duration);
        Mode = (UserCameraMode)await retrieveValue<int>(VRChatCameraInput.Mode);
        await retrievePose();
    }

    internal void HandleMessage(VRChatOSCMessage message)
    {
        if (!Enum.TryParse<VRChatCameraInput>(message.Address.Split('/').Last(), out var input)) return;

        switch (input)
        {
            case VRChatCameraInput.ShowUIInCamera:
                Mask = (bool)message.ParameterValue ? Mask | UserCameraMask.UI : Mask & ~UserCameraMask.UI;
                break;

            case VRChatCameraInput.Lock:
                IsLocked = (bool)message.ParameterValue;
                break;

            case VRChatCameraInput.LocalPlayer:
                Mask = (bool)message.ParameterValue ? Mask | UserCameraMask.LocalPlayer : Mask & ~UserCameraMask.LocalPlayer;
                break;

            case VRChatCameraInput.RemotePlayer:
                Mask = (bool)message.ParameterValue ? Mask | UserCameraMask.RemotePlayer : Mask & ~UserCameraMask.RemotePlayer;
                break;

            case VRChatCameraInput.Environment:
                Mask = (bool)message.ParameterValue ? Mask | UserCameraMask.Environment : Mask & ~UserCameraMask.Environment;
                break;

            case VRChatCameraInput.GreenScreen:
                Mask = (bool)message.ParameterValue ? Mask | UserCameraMask.GreenScreen : Mask & ~UserCameraMask.GreenScreen;
                break;

            case VRChatCameraInput.SmoothMovement:
                SmoothMovement = (bool)message.ParameterValue;
                break;

            case VRChatCameraInput.LookAtMe:
                Direction = (bool)message.ParameterValue ? UserCameraDirection.User : UserCameraDirection.Back;
                break;

            case VRChatCameraInput.AutoLevelPitch:
                if ((bool)message.ParameterValue)
                    AutoLevel |= UserCameraAutoLevel.Pitch;
                else
                    AutoLevel &= ~UserCameraAutoLevel.Pitch;
                break;

            case VRChatCameraInput.AutoLevelRoll:
                if ((bool)message.ParameterValue)
                    AutoLevel |= UserCameraAutoLevel.Roll;
                else
                    AutoLevel &= ~UserCameraAutoLevel.Roll;
                break;

            case VRChatCameraInput.Flying:
                IsFlying = (bool)message.ParameterValue;
                break;

            case VRChatCameraInput.TriggerTakesPhotos:
                TriggerTakesPhotos = (bool)message.ParameterValue;
                break;

            case VRChatCameraInput.DollyPathsStayVisible:
                DollyPathsStayVisible = (bool)message.ParameterValue;
                break;

            case VRChatCameraInput.AudioFromCamera:
                AudioSource = (bool)message.ParameterValue ? UserCameraAudioSource.Camera : UserCameraAudioSource.User;
                break;

            case VRChatCameraInput.ShowFocus:
                ShowFocus = (bool)message.ParameterValue;
                break;

            case VRChatCameraInput.Streaming:
                IsStreaming = (bool)message.ParameterValue;
                break;

            case VRChatCameraInput.RollWhileFlying:
                RollWhileFlying = (bool)message.ParameterValue;
                break;

            case VRChatCameraInput.OrientationIsLandscape:
                Orientation = (bool)message.ParameterValue ? UserCameraOrientation.Landscape : UserCameraOrientation.Portrait;
                break;

            case VRChatCameraInput.Zoom:
                Zoom = (float)message.ParameterValue;
                break;

            case VRChatCameraInput.Exposure:
                Exposure = (float)message.ParameterValue;
                break;

            case VRChatCameraInput.FocalDistance:
                FocalDistance = (float)message.ParameterValue;
                break;

            case VRChatCameraInput.Aperture:
                Aperture = (float)message.ParameterValue;
                break;

            case VRChatCameraInput.Hue:
                var hue = (int)(float)message.ParameterValue;
                GreenScreenBackground = GreenScreenBackground with { Hue = hue };
                break;

            case VRChatCameraInput.Saturation:
                var saturation = Interpolation.Map((float)message.ParameterValue, 0f, 100f, 0f, 1f);
                GreenScreenBackground = GreenScreenBackground with { Saturation = saturation };
                break;

            case VRChatCameraInput.Lightness:
                var lightness = Interpolation.Map((float)message.ParameterValue, 0f, 100f, 0f, 1f);
                GreenScreenBackground = GreenScreenBackground with { Lightness = lightness };
                break;

            case VRChatCameraInput.LookAtMeXOffset:
                UserDirectionOffset = UserDirectionOffset with { X = (float)message.ParameterValue };
                break;

            case VRChatCameraInput.LookAtMeYOffset:
                UserDirectionOffset = UserDirectionOffset with { Y = (float)message.ParameterValue };
                break;

            case VRChatCameraInput.FlySpeed:
                FlySpeed = (float)message.ParameterValue;
                break;

            case VRChatCameraInput.TurnSpeed:
                TurnSpeed = (float)message.ParameterValue;
                break;

            case VRChatCameraInput.SmoothingStrength:
                SmoothingStrength = (float)message.ParameterValue;
                break;

            case VRChatCameraInput.PhotoRate:
                PhotoRate = (float)message.ParameterValue;
                break;

            case VRChatCameraInput.Duration:
                Duration = (float)message.ParameterValue;
                break;

            case VRChatCameraInput.Mode:
                Mode = (UserCameraMode)(int)message.ParameterValue;
                break;

            case VRChatCameraInput.Pose:
                var data = message.Arguments;
                var pos = new Vector3((float)data[0]!, (float)data[1]!, (float)data[2]!);
                var rot = new Vector3((float)data[3]!, (float)data[4]!, (float)data[5]!);
                Transform = new Transform(pos, rot.ToQuaternion());
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private async Task<T> retrieveValue<T>(VRChatCameraInput input)
    {
        var address = await oscClient.FindAddress(inputToAddress(input), CancellationToken.None);
        if (address is null) return default!;

        var value = address.Value![0];
        if (value is double dValue) value = (float)dValue;
        if (value is long lValue) value = (int)lValue;

        return (T)value;
    }

    private async Task<UserCameraMask> retrieveMask()
    {
        var ui = await retrieveValue<bool>(VRChatCameraInput.ShowUIInCamera);
        var localPlayer = await retrieveValue<bool>(VRChatCameraInput.LocalPlayer);
        var remotePlayer = await retrieveValue<bool>(VRChatCameraInput.RemotePlayer);
        var environment = await retrieveValue<bool>(VRChatCameraInput.Environment);
        var greenScreen = await retrieveValue<bool>(VRChatCameraInput.GreenScreen);

        UserCameraMask mask = 0;

        if (ui) mask |= UserCameraMask.UI;
        if (localPlayer) mask |= UserCameraMask.LocalPlayer;
        if (remotePlayer) mask |= UserCameraMask.RemotePlayer;
        if (environment) mask |= UserCameraMask.Environment;
        if (greenScreen) mask |= UserCameraMask.GreenScreen;

        return mask;
    }

    private async Task<float> retrieveZoom()
    {
        var address = await oscClient.FindAddress(inputToAddress(VRChatCameraInput.Zoom), CancellationToken.None);
        if (address is null) return 0f;

        var value = address.Value![0];
        return value is "NaN" ? 0.15f : Interpolation.Map((float)(double)value, 0f, 100f, 0f, 1f);
    }

    private async Task<ColorHSL> retrieveGreenScreenBackground()
    {
        var hueAddress = await oscClient.FindAddress(inputToAddress(VRChatCameraInput.Hue), CancellationToken.None);
        var hue = hueAddress is null ? 0 : (int)(double)hueAddress.Value![0];

        var saturationAddress = await oscClient.FindAddress(inputToAddress(VRChatCameraInput.Saturation), CancellationToken.None);
        var saturation = saturationAddress is null ? 0f : (float)Interpolation.Map((double)saturationAddress.Value![0], 0f, 100f, 0f, 1f);

        var lightnessAddress = await oscClient.FindAddress(inputToAddress(VRChatCameraInput.Lightness), CancellationToken.None);
        var lightness = lightnessAddress is null ? 0f : (float)Interpolation.Map((double)lightnessAddress.Value![0], 0f, 100f, 0f, 1f);

        return new ColorHSL(hue, saturation, lightness);
    }

    private async Task retrievePose()
    {
        var address = await oscClient.FindAddress(inputToAddress(VRChatCameraInput.Pose), CancellationToken.None);
        if (address is null) return;

        var values = ((JArray)address.Value![0]).ToObject<double[]>()!;
        Transform = new Transform(new Vector3((float)values[0], (float)values[1], (float)values[2]), new Vector3((float)values[3], (float)values[4], (float)values[5]).ToQuaternion());
    }

    private string inputToAddress(VRChatCameraInput input) => $"{VRChatOSCConstants.ADDRESS_USERCAMERA_PREFIX}/{input}";

    private async Task sendAndReset(VRChatCameraInput input)
    {
        oscClient.Send(inputToAddress(input), 1);
        await Task.Delay(50);
        oscClient.Send(inputToAddress(input), 0);
    }

    private void send(VRChatCameraInput input, params object?[] values)
    {
        oscClient.Send(inputToAddress(input), values);
    }

    public void Close() => sendAndReset(VRChatCameraInput.Close).Forget();
    public void Capture() => sendAndReset(VRChatCameraInput.Capture).Forget();
    public void CaptureDelayed() => sendAndReset(VRChatCameraInput.CaptureDelayed).Forget();
    public void SetLock(bool locked) => send(VRChatCameraInput.Lock, locked);
    public void SetSmoothMovement(bool smoothMovement) => send(VRChatCameraInput.SmoothMovement, smoothMovement);
    public void SetDirection(UserCameraDirection direction) => send(VRChatCameraInput.LookAtMe, direction == UserCameraDirection.User);
    public void SetFlying(bool flying) => send(VRChatCameraInput.Flying, flying);
    public void SetTriggerTakesPhotos(bool triggerTakesPhotos) => send(VRChatCameraInput.TriggerTakesPhotos, triggerTakesPhotos);
    public void SetDollyPathsStayVisible(bool dollyPathsStayVisible) => send(VRChatCameraInput.DollyPathsStayVisible, dollyPathsStayVisible);
    public void SetAudioSource(UserCameraAudioSource audioSource) => send(VRChatCameraInput.AudioFromCamera, audioSource == UserCameraAudioSource.Camera);
    public void SetShowFocus(bool showFocus) => send(VRChatCameraInput.ShowFocus, showFocus);
    public void SetStreaming(bool streaming) => send(VRChatCameraInput.Streaming, streaming);
    public void SetRollWhileFlying(bool rollWhileFlying) => send(VRChatCameraInput.RollWhileFlying, rollWhileFlying);
    public void SetOrientation(UserCameraOrientation orientation) => send(VRChatCameraInput.OrientationIsLandscape, orientation == UserCameraOrientation.Landscape);
    public void SetZoom(float zoom) => send(VRChatCameraInput.Zoom, float.Lerp(0f, 100f, zoom));
    public void SetExposure(float exposure) => send(VRChatCameraInput.Exposure, exposure);
    public void SetFocalDistance(float focalDistance) => send(VRChatCameraInput.FocalDistance, focalDistance);
    public void SetAperture(float aperture) => send(VRChatCameraInput.Aperture, aperture);
    public void SetFlySpeed(float flySpeed) => send(VRChatCameraInput.FlySpeed, flySpeed);
    public void SetTurnSpeed(float turnSpeed) => send(VRChatCameraInput.TurnSpeed, turnSpeed);
    public void SetSmoothingStrength(float smoothingStrength) => send(VRChatCameraInput.SmoothingStrength, smoothingStrength);
    public void SetPhotoRate(float photoRate) => send(VRChatCameraInput.PhotoRate, photoRate);
    public void SetDuration(float duration) => send(VRChatCameraInput.Duration, duration);
    public void SetMode(UserCameraMode mode) => send(VRChatCameraInput.Mode, (int)mode);

    public void SetPose(Transform transform)
    {
        var pos = transform.Position;
        var rot = transform.Rotation.ToEulerDegrees();
        send(VRChatCameraInput.Pose, pos.X, pos.Y, pos.Z, rot.X, rot.Y, rot.Z);
    }

    public void SetMask(UserCameraMask mask)
    {
        send(VRChatCameraInput.ShowUIInCamera, mask.HasFlag(UserCameraMask.UI));
        send(VRChatCameraInput.LocalPlayer, mask.HasFlag(UserCameraMask.LocalPlayer));
        send(VRChatCameraInput.RemotePlayer, mask.HasFlag(UserCameraMask.RemotePlayer));
        send(VRChatCameraInput.Environment, mask.HasFlag(UserCameraMask.Environment));
        send(VRChatCameraInput.GreenScreen, mask.HasFlag(UserCameraMask.GreenScreen));
    }

    public void SetGreenScreenBackground(ColorHSL color)
    {
        send(VRChatCameraInput.Hue, (float)color.Hue);
        send(VRChatCameraInput.Saturation, float.Lerp(0f, 100f, color.Saturation));
        send(VRChatCameraInput.Lightness, float.Lerp(0f, 100f, float.Clamp(color.Lightness, 0f, 0.5f)));
    }

    public void SetUserDirectionOffset(Vector2 offset)
    {
        send(VRChatCameraInput.LookAtMeXOffset, offset.X);
        send(VRChatCameraInput.LookAtMeYOffset, offset.Y);
    }

    public void SetAutoLevel(UserCameraAutoLevel flags)
    {
        send(VRChatCameraInput.AutoLevelPitch, flags.HasFlag(UserCameraAutoLevel.Pitch));
        send(VRChatCameraInput.AutoLevelRoll, flags.HasFlag(UserCameraAutoLevel.Roll));
    }
}

public enum VRChatCameraInput
{
    Close,
    Capture,
    CaptureDelayed,
    ShowUIInCamera,
    Lock,
    LocalPlayer,
    RemotePlayer,
    Environment,
    GreenScreen,
    SmoothMovement,
    LookAtMe,
    AutoLevelPitch,
    AutoLevelRoll,
    Flying,
    TriggerTakesPhotos,
    DollyPathsStayVisible,
    AudioFromCamera,
    ShowFocus,
    Streaming,
    RollWhileFlying,
    OrientationIsLandscape,
    Zoom,
    Exposure,
    FocalDistance,
    Aperture,
    Hue,
    Saturation,
    Lightness,
    LookAtMeXOffset,
    LookAtMeYOffset,
    FlySpeed,
    TurnSpeed,
    SmoothingStrength,
    PhotoRate,
    Duration,
    Mode,
    Pose
}

// Can't set: GalleryPhoto, UserIcon, Sticker, Emoji
public enum UserCameraMode
{
    LocalPhoto = 1,
    Stream = 2,
    MultiLayer = 4,
    Print = 5,
    Drone = 6
}

public enum UserCameraOrientation
{
    Portrait,
    Landscape
}

[Flags]
public enum UserCameraMask
{
    LocalPlayer = 1 << 0,
    RemotePlayer = 1 << 1,
    Environment = 1 << 2,
    GreenScreen = 1 << 3,
    UI = 1 << 4
}

public enum UserCameraDirection
{
    Back,
    User
}

[Flags]
public enum UserCameraAutoLevel
{
    Pitch = 1 << 0,
    Roll = 1 << 1
}

public enum UserCameraAudioSource
{
    User,
    Camera
}