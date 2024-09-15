// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.VRChat;

public sealed class Player
{
    public Viseme Viseme { get; private set; }
    public float Voice { get; private set; }
    public Gesture GestureLeft { get; private set; }
    public Gesture GestureRight { get; private set; }
    public float GestureLeftWeight { get; private set; }
    public float GestureRightWeight { get; private set; }
    public float AngularY { get; private set; }
    public float VelocityX { get; private set; }
    public float VelocityY { get; private set; }
    public float VelocityZ { get; private set; }
    public float Upright { get; private set; }
    public bool Grounded { get; private set; }
    public bool Seated { get; private set; }
    public bool AFK { get; private set; }
    public TrackingType TrackingType { get; private set; }
    public bool IsVR { get; private set; }
    public bool IsMuted { get; private set; }
    public bool InStation { get; private set; }
    public bool Earmuffs { get; private set; }
    public bool ScaleModified { get; private set; }
    public float ScaleFactor { get; private set; }
    public float ScaleFactorInverse { get; private set; }
    public float EyeHeightAsMeters { get; private set; }
    public float EyeHeightAsPercent { get; private set; }

    private readonly VRChatOscClient oscClient;
    private bool hasChanged;

    internal Player(VRChatOscClient oscClient)
    {
        this.oscClient = oscClient;
    }

    internal async Task RetrieveAll()
    {
        foreach (var vrChatInputParameter in Enum.GetValues<VRChatAvatarParameter>())
        {
            await retrieve(vrChatInputParameter.ToString());
        }
    }

    private async Task retrieve(string parameterName)
    {
        var value = await AppManager.GetInstance().VRChatOscClient.FindParameterValue(parameterName);
        if (value is null) return;

        Update(parameterName, value);
    }

    internal bool Update(string parameterName, object value)
    {
        try
        {
            if (!Enum.TryParse(parameterName, out VRChatAvatarParameter vrChatInputParameter)) return false;

            switch (vrChatInputParameter)
            {
                case VRChatAvatarParameter.Viseme:
                    Viseme = (Viseme)(int)value;
                    break;

                case VRChatAvatarParameter.Voice:
                    Voice = (float)value;
                    break;

                case VRChatAvatarParameter.GestureLeft:
                    GestureLeft = (Gesture)(int)value;
                    break;

                case VRChatAvatarParameter.GestureRight:
                    GestureRight = (Gesture)(int)value;
                    break;

                case VRChatAvatarParameter.GestureLeftWeight:
                    GestureLeftWeight = (float)value;
                    break;

                case VRChatAvatarParameter.GestureRightWeight:
                    GestureRightWeight = (float)value;
                    break;

                case VRChatAvatarParameter.AngularY:
                    AngularY = (float)value;
                    break;

                case VRChatAvatarParameter.VelocityX:
                    VelocityX = (float)value;
                    break;

                case VRChatAvatarParameter.VelocityY:
                    VelocityY = (float)value;
                    break;

                case VRChatAvatarParameter.VelocityZ:
                    VelocityZ = (float)value;
                    break;

                case VRChatAvatarParameter.Upright:
                    Upright = (float)value;
                    break;

                case VRChatAvatarParameter.Grounded:
                    Grounded = (bool)value;
                    break;

                case VRChatAvatarParameter.Seated:
                    Seated = (bool)value;
                    break;

                case VRChatAvatarParameter.AFK:
                    AFK = (bool)value;
                    break;

                case VRChatAvatarParameter.TrackingType:
                    TrackingType = (TrackingType)(int)value;
                    break;

                case VRChatAvatarParameter.VRMode:
                    IsVR = (int)value == 1;
                    break;

                case VRChatAvatarParameter.MuteSelf:
                    IsMuted = (bool)value;
                    break;

                case VRChatAvatarParameter.InStation:
                    InStation = (bool)value;
                    break;

                case VRChatAvatarParameter.Earmuffs:
                    Earmuffs = (bool)value;
                    break;

                case VRChatAvatarParameter.ScaleModified:
                    ScaleModified = (bool)value;
                    break;

                case VRChatAvatarParameter.ScaleFactor:
                    ScaleFactor = (float)value;
                    break;

                case VRChatAvatarParameter.ScaleFactorInverse:
                    ScaleFactorInverse = (float)value;
                    break;

                case VRChatAvatarParameter.EyeHeightAsMeters:
                    EyeHeightAsMeters = (float)value;
                    break;

                case VRChatAvatarParameter.EyeHeightAsPercent:
                    EyeHeightAsPercent = (float)value;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(vrChatInputParameter), vrChatInputParameter.ToString(), $"Unknown {nameof(VRChatAvatarParameter)}");
            }

            return true;
        }
        catch (Exception e)
        {
            ExceptionHandler.Handle(e, $"Error while processing default parameter {parameterName}. Type {value.GetType().ToReadableName()}. Value {value}");
            return false;
        }
    }

    private static string actionToAddress(VRChatButtonInput action) => $"/input/{action}";
    private static string actionToAddress(VRChatAxesInput action) => $"/input/{action}";

    private async void sendAndReset(VRChatButtonInput action)
    {
        oscClient.SendValue(actionToAddress(action), 1);
        await Task.Delay(15);
        oscClient.SendValue(actionToAddress(action), 0);
    }

    internal void ResetAll()
    {
        Viseme = default;
        Voice = default;
        GestureLeft = default;
        GestureRight = default;
        GestureLeftWeight = default;
        GestureRightWeight = default;
        AngularY = default;
        VelocityX = default;
        VelocityY = default;
        VelocityZ = default;
        Upright = default;
        Grounded = default;
        Seated = default;
        AFK = default;
        TrackingType = default;
        IsVR = default;
        IsMuted = default;
        InStation = default;
        Earmuffs = default;
        ScaleModified = default;
        ScaleFactor = default;
        ScaleFactorInverse = default;
        EyeHeightAsMeters = default;
        EyeHeightAsPercent = default;

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
        oscClient.SendValue(actionToAddress(VRChatButtonInput.MoveForward), 1);
        hasChanged = true;
    }

    public void StopMoveForward()
    {
        oscClient.SendValue(actionToAddress(VRChatButtonInput.MoveForward), 0);
    }

    public void MoveBackward()
    {
        oscClient.SendValue(actionToAddress(VRChatButtonInput.MoveBackward), 1);
        hasChanged = true;
    }

    public void StopMoveBackward()
    {
        oscClient.SendValue(actionToAddress(VRChatButtonInput.MoveBackward), 0);
    }

    public void MoveLeft()
    {
        oscClient.SendValue(actionToAddress(VRChatButtonInput.MoveLeft), 1);
        hasChanged = true;
    }

    public void StopMoveLeft()
    {
        oscClient.SendValue(actionToAddress(VRChatButtonInput.MoveLeft), 0);
    }

    public void MoveRight()
    {
        oscClient.SendValue(actionToAddress(VRChatButtonInput.MoveRight), 1);
        hasChanged = true;
    }

    public void StopMoveRight()
    {
        oscClient.SendValue(actionToAddress(VRChatButtonInput.MoveRight), 0);
    }

    public void LookLeft()
    {
        oscClient.SendValue(actionToAddress(VRChatButtonInput.LookLeft), 1);
        hasChanged = true;
    }

    public void StopLookLeft()
    {
        oscClient.SendValue(actionToAddress(VRChatButtonInput.LookLeft), 0);
    }

    public void LookRight()
    {
        oscClient.SendValue(actionToAddress(VRChatButtonInput.LookRight), 1);
        hasChanged = true;
    }

    public void StopLookRight()
    {
        oscClient.SendValue(actionToAddress(VRChatButtonInput.LookRight), 0);
    }

    public void Jump()
    {
        sendAndReset(VRChatButtonInput.Jump);
    }

    public void Run()
    {
        oscClient.SendValue(actionToAddress(VRChatButtonInput.Run), 1);
        hasChanged = true;
    }

    public void StopRun()
    {
        oscClient.SendValue(actionToAddress(VRChatButtonInput.Run), 0);
    }

    public void ComfortLeft()
    {
        sendAndReset(VRChatButtonInput.ComfortLeft);
    }

    public void ComfortRight()
    {
        sendAndReset(VRChatButtonInput.ComfortRight);
    }

    public void DropRight()
    {
        sendAndReset(VRChatButtonInput.DropRight);
    }

    public void UseRight()
    {
        sendAndReset(VRChatButtonInput.UseRight);
    }

    public void GrabRight()
    {
        sendAndReset(VRChatButtonInput.GrabRight);
    }

    public void DropLeft()
    {
        sendAndReset(VRChatButtonInput.DropLeft);
    }

    public void UseLeft()
    {
        sendAndReset(VRChatButtonInput.UseLeft);
    }

    public void GrabLeft()
    {
        sendAndReset(VRChatButtonInput.GrabLeft);
    }

    public void EnableSafeMode()
    {
        sendAndReset(VRChatButtonInput.PanicButton);
    }

    public void ToggleLeftQuickMenu()
    {
        sendAndReset(VRChatButtonInput.QuickMenuToggleLeft);
    }

    public void ToggleRightQuickMenu()
    {
        sendAndReset(VRChatButtonInput.QuickMenuToggleRight);
    }

    public void ToggleVoice()
    {
        sendAndReset(VRChatButtonInput.Voice);
    }

    public void Mute()
    {
        oscClient.SendValue(actionToAddress(VRChatButtonInput.Voice), 1);
    }

    public void UnMute()
    {
        oscClient.SendValue(actionToAddress(VRChatButtonInput.Voice), 0);
    }

    public void MoveVertical(float value)
    {
        oscClient.SendValue(actionToAddress(VRChatAxesInput.Vertical), value);
    }

    public void MoveHorizontal(float value)
    {
        oscClient.SendValue(actionToAddress(VRChatAxesInput.Horizontal), value);
    }

    public void LookHorizontal(float value)
    {
        oscClient.SendValue(actionToAddress(VRChatAxesInput.LookHorizontal), value);
    }
}

public enum VRChatButtonInput
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

public enum VRChatAxesInput
{
    Vertical,
    Horizontal,
    LookHorizontal
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
    Uninitialised = 0,
    Generic = 1,
    Hands = 2,
    HeadHands = 3,
    HeadHandsHip = 4,
    HeadHandsFeet = 5,
    HeadHandsHipFeet = 6
}

public enum VRChatAvatarParameter
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
    Earmuffs,
    ScaleModified,
    ScaleFactor,
    ScaleFactorInverse,
    EyeHeightAsMeters,
    EyeHeightAsPercent
}
