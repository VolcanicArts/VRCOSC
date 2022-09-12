// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

// ReSharper disable InconsistentNaming

using System.Threading.Tasks;
using VRCOSC.OSC;

namespace VRCOSC.Game.Modules;

public sealed class Player
{
    public Viseme? Viseme;
    public float? Voice;
    public Gesture? GestureLeft;
    public Gesture? GestureRight;
    public float? GestureLeftWeight;
    public float? GestureRightWeight;
    public float? AngularY;
    public float? VelocityX;
    public float? VelocityY;
    public float? VelocityZ;
    public float? Upright;
    public bool? Grounded;
    public bool? Seated;
    public bool? AFK;
    public TrackingType? TrackingType;
    public bool? IsVR;
    public bool? IsMuted;
    public bool? InStation;

    private readonly OscClient oscClient;
    private bool hasChanged = false;

    public Player(OscClient oscClient)
    {
        this.oscClient = oscClient;

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
    InStation
}
