// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using VRCOSC.App.OSC.VRChat;

namespace VRCOSC.App.SDK.VRChat;

public sealed class Player
{
    public Viseme? Viseme { get; private set; }
    public float? Voice { get; private set; }
    public Gesture? GestureLeft { get; private set; }
    public Gesture? GestureRight { get; private set; }
    public float? GestureLeftWeight { get; private set; }
    public float? GestureRightWeight { get; private set; }
    public float? AngularY { get; private set; }
    public float? VelocityX { get; private set; }
    public float? VelocityY { get; private set; }
    public float? VelocityZ { get; private set; }
    public float? Upright { get; private set; }
    public bool? Grounded { get; private set; }
    public bool? Seated { get; private set; }
    public bool? AFK { get; private set; }
    public TrackingType? TrackingType { get; private set; }
    public bool? IsVR { get; private set; }
    public bool? IsMuted { get; private set; }
    public bool? InStation { get; private set; }
    public bool? Earmuffs { get; private set; }

    private readonly VRChatOscClient oscClient;
    private bool hasChanged;

    public Player(VRChatOscClient oscClient)
    {
        this.oscClient = oscClient;
    }

    public bool Update(string parameterName, object value)
    {
        if (!Enum.TryParse(parameterName, out VRChatInputParameter vrChatInputParameter)) return false;

        switch (vrChatInputParameter)
        {
            case VRChatInputParameter.Viseme:
                Viseme = (Viseme)(int)value;
                break;

            case VRChatInputParameter.Voice:
                Voice = (float)value;
                break;

            case VRChatInputParameter.GestureLeft:
                GestureLeft = (Gesture)(int)value;
                break;

            case VRChatInputParameter.GestureRight:
                GestureRight = (Gesture)(int)value;
                break;

            case VRChatInputParameter.GestureLeftWeight:
                GestureLeftWeight = (float)value;
                break;

            case VRChatInputParameter.GestureRightWeight:
                GestureRightWeight = (float)value;
                break;

            case VRChatInputParameter.AngularY:
                AngularY = (float)value;
                break;

            case VRChatInputParameter.VelocityX:
                VelocityX = (float)value;
                break;

            case VRChatInputParameter.VelocityY:
                VelocityY = (float)value;
                break;

            case VRChatInputParameter.VelocityZ:
                VelocityZ = (float)value;
                break;

            case VRChatInputParameter.Upright:
                Upright = (float)value;
                break;

            case VRChatInputParameter.Grounded:
                Grounded = (bool)value;
                break;

            case VRChatInputParameter.Seated:
                Seated = (bool)value;
                break;

            case VRChatInputParameter.AFK:
                AFK = (bool)value;
                break;

            case VRChatInputParameter.TrackingType:
                TrackingType = (TrackingType)(int)value;
                break;

            case VRChatInputParameter.VRMode:
                IsVR = (int)value == 1;
                break;

            case VRChatInputParameter.MuteSelf:
                IsMuted = (bool)value;
                break;

            case VRChatInputParameter.InStation:
                InStation = (bool)value;
                break;

            case VRChatInputParameter.Earmuffs:
                Earmuffs = (bool)value;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(vrChatInputParameter), vrChatInputParameter, $"Unknown {nameof(VRChatInputParameter)}");
        }

        return true;
    }

    private static string actionToAddress(VRChatInputAction action) => $"/input/{action}";

    private async void sendAndReset(VRChatInputAction action)
    {
        oscClient.SendValue(actionToAddress(action), 1);
        await Task.Delay(15);
        oscClient.SendValue(actionToAddress(action), 0);
    }

    public void ResetAll()
    {
        Viseme = null;
        Voice = null;
        GestureLeft = null;
        GestureRight = null;
        GestureLeftWeight = null;
        GestureRightWeight = null;
        AngularY = null;
        VelocityX = null;
        VelocityY = null;
        VelocityZ = null;
        Upright = null;
        Grounded = null;
        Seated = null;
        AFK = null;
        TrackingType = null;
        IsVR = null;
        IsMuted = null;
        InStation = null;
        Earmuffs = null;

        if (!hasChanged) return;

        StopMoveForward();
        StopMoveBackward();
        StopMoveLeft();
        StopMoveRight();
        StopLookLeft();
        StopLookRight();
        StopRun();
    }

    public void MoveForward()
    {
        oscClient.SendValue(actionToAddress(VRChatInputAction.MoveForward), 1);
        hasChanged = true;
    }

    public void StopMoveForward()
    {
        oscClient.SendValue(actionToAddress(VRChatInputAction.MoveForward), 0);
    }

    public void MoveBackward()
    {
        oscClient.SendValue(actionToAddress(VRChatInputAction.MoveBackward), 1);
        hasChanged = true;
    }

    public void StopMoveBackward()
    {
        oscClient.SendValue(actionToAddress(VRChatInputAction.MoveBackward), 0);
    }

    public void MoveLeft()
    {
        oscClient.SendValue(actionToAddress(VRChatInputAction.MoveLeft), 1);
        hasChanged = true;
    }

    public void StopMoveLeft()
    {
        oscClient.SendValue(actionToAddress(VRChatInputAction.MoveLeft), 0);
    }

    public void MoveRight()
    {
        oscClient.SendValue(actionToAddress(VRChatInputAction.MoveRight), 1);
        hasChanged = true;
    }

    public void StopMoveRight()
    {
        oscClient.SendValue(actionToAddress(VRChatInputAction.MoveRight), 0);
    }

    public void LookLeft()
    {
        oscClient.SendValue(actionToAddress(VRChatInputAction.LookLeft), 1);
        hasChanged = true;
    }

    public void StopLookLeft()
    {
        oscClient.SendValue(actionToAddress(VRChatInputAction.LookLeft), 0);
    }

    public void LookRight()
    {
        oscClient.SendValue(actionToAddress(VRChatInputAction.LookRight), 1);
        hasChanged = true;
    }

    public void StopLookRight()
    {
        oscClient.SendValue(actionToAddress(VRChatInputAction.LookRight), 0);
    }

    public void Jump()
    {
        sendAndReset(VRChatInputAction.Jump);
    }

    public void Run()
    {
        oscClient.SendValue(actionToAddress(VRChatInputAction.Run), 1);
        hasChanged = true;
    }

    public void StopRun()
    {
        oscClient.SendValue(actionToAddress(VRChatInputAction.Run), 0);
    }

    public void ComfortLeft()
    {
        sendAndReset(VRChatInputAction.ComfortLeft);
    }

    public void ComfortRight()
    {
        sendAndReset(VRChatInputAction.ComfortRight);
    }

    public void DropRight()
    {
        sendAndReset(VRChatInputAction.DropRight);
    }

    public void UseRight()
    {
        sendAndReset(VRChatInputAction.UseRight);
    }

    public void GrabRight()
    {
        sendAndReset(VRChatInputAction.GrabRight);
    }

    public void DropLeft()
    {
        sendAndReset(VRChatInputAction.DropLeft);
    }

    public void UseLeft()
    {
        sendAndReset(VRChatInputAction.UseLeft);
    }

    public void GrabLeft()
    {
        sendAndReset(VRChatInputAction.GrabLeft);
    }

    public void EnableSafeMode()
    {
        sendAndReset(VRChatInputAction.PanicButton);
    }

    public void ToggleLeftQuickMenu()
    {
        sendAndReset(VRChatInputAction.QuickMenuToggleLeft);
    }

    public void ToggleRightQuickMenu()
    {
        sendAndReset(VRChatInputAction.QuickMenuToggleRight);
    }

    public void ToggleVoice()
    {
        sendAndReset(VRChatInputAction.Voice);
    }

    public void Mute()
    {
        oscClient.SendValue(actionToAddress(VRChatInputAction.Voice), 1);
    }

    public void UnMute()
    {
        oscClient.SendValue(actionToAddress(VRChatInputAction.Voice), 0);
    }
}

public enum VRChatInputAction
{
    MoveForward,
    MoveBackward,
    MoveLeft,
    MoveRight,
    LookLeft,
    LookRight,
    Jump,
    Run,
    ComfortLeft,
    ComfortRight,
    DropRight,
    UseRight,
    GrabRight,
    DropLeft,
    UseLeft,
    GrabLeft,
    PanicButton,
    QuickMenuToggleLeft,
    QuickMenuToggleRight,
    Voice
}

public enum Viseme
{
    SIL,
    PP,
    FF,
    TH,
    DD,
    KK,
    CH,
    SS,
    NN,
    RR,
    AA,
    E,
    I,
    O,
    U
}

public enum Gesture
{
    Neutral,
    Fist,
    HandOpen,
    FingerPoint,
    Victory,
    RockNRoll,
    HandGun,
    ThumbsUp
}

public enum TrackingType
{
    Uninitialised,
    Generic,
    HandsOnly,
    HeadAndHands,
    HeadHandsAndHip,
    FullBody
}

public enum VRChatInputParameter
{
    Viseme,
    Voice,
    GestureLeft,
    GestureRight,
    GestureLeftWeight,
    GestureRightWeight,
    AngularY,
    VelocityX,
    VelocityY,
    VelocityZ,
    Upright,
    Grounded,
    Seated,
    AFK,
    TrackingType,
    VRMode,
    MuteSelf,
    InStation,
    Earmuffs
}
