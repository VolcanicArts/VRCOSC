// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading;
using System.Threading.Tasks;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.SDK.Parameters;
using VRCOSC.App.Utils;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace VRCOSC.App.SDK.VRChat;

public sealed class Player
{
    public Viseme Viseme { get; private set; }
    public float Voice { get; private set; }
    public GestureType GestureTypeLeft { get; private set; }
    public GestureType GestureTypeRight { get; private set; }
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

    private readonly VRChatOSCClient oscClient;
    private bool hasChanged;

    internal Player(VRChatOSCClient oscClient)
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
        var parameter = await AppManager.GetInstance().VRChatOscClient.FindParameter(parameterName, CancellationToken.None);
        if (parameter is null) return;

        Update(parameter);
    }

    internal bool Update(VRChatParameter parameter)
    {
        try
        {
            if (!Enum.TryParse(parameter.Name, out VRChatAvatarParameter vrChatInputParameter)) return false;

            switch (vrChatInputParameter)
            {
                case VRChatAvatarParameter.Viseme:
                    Viseme = (Viseme)(int)parameter.Value;
                    break;

                case VRChatAvatarParameter.Voice:
                    Voice = (float)parameter.Value;
                    break;

                case VRChatAvatarParameter.GestureLeft:
                    GestureTypeLeft = (GestureType)(int)parameter.Value;
                    break;

                case VRChatAvatarParameter.GestureRight:
                    GestureTypeRight = (GestureType)(int)parameter.Value;
                    break;

                case VRChatAvatarParameter.GestureLeftWeight:
                    GestureLeftWeight = (float)parameter.Value;
                    break;

                case VRChatAvatarParameter.GestureRightWeight:
                    GestureRightWeight = (float)parameter.Value;
                    break;

                case VRChatAvatarParameter.AngularY:
                    AngularY = (float)parameter.Value;
                    break;

                case VRChatAvatarParameter.VelocityX:
                    VelocityX = (float)parameter.Value;
                    break;

                case VRChatAvatarParameter.VelocityY:
                    VelocityY = (float)parameter.Value;
                    break;

                case VRChatAvatarParameter.VelocityZ:
                    VelocityZ = (float)parameter.Value;
                    break;

                case VRChatAvatarParameter.Upright:
                    Upright = (float)parameter.Value;
                    break;

                case VRChatAvatarParameter.Grounded:
                    Grounded = (bool)parameter.Value;
                    break;

                case VRChatAvatarParameter.Seated:
                    Seated = (bool)parameter.Value;
                    break;

                case VRChatAvatarParameter.AFK:
                    AFK = (bool)parameter.Value;
                    break;

                case VRChatAvatarParameter.TrackingType:
                    TrackingType = (TrackingType)(int)parameter.Value;
                    break;

                case VRChatAvatarParameter.VRMode:
                    IsVR = (int)parameter.Value == 1;
                    break;

                case VRChatAvatarParameter.MuteSelf:
                    IsMuted = (bool)parameter.Value;
                    break;

                case VRChatAvatarParameter.InStation:
                    InStation = (bool)parameter.Value;
                    break;

                case VRChatAvatarParameter.Earmuffs:
                    Earmuffs = (bool)parameter.Value;
                    break;

                case VRChatAvatarParameter.ScaleModified:
                    ScaleModified = (bool)parameter.Value;
                    break;

                case VRChatAvatarParameter.ScaleFactor:
                    ScaleFactor = (float)parameter.Value;
                    break;

                case VRChatAvatarParameter.ScaleFactorInverse:
                    ScaleFactorInverse = (float)parameter.Value;
                    break;

                case VRChatAvatarParameter.EyeHeightAsMeters:
                    EyeHeightAsMeters = (float)parameter.Value;
                    break;

                case VRChatAvatarParameter.EyeHeightAsPercent:
                    EyeHeightAsPercent = (float)parameter.Value;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(vrChatInputParameter), vrChatInputParameter.ToString(), $"Unknown {nameof(VRChatAvatarParameter)}");
            }

            return true;
        }
        catch (Exception e)
        {
            Logger.Error(e, $"Error while processing default parameter {parameter.Name}. Type {parameter.Value.GetType().GetFriendlyName()}. Value {parameter.Value}");
            return false;
        }
    }

    private static string actionToAddress(VRChatButtonInput action) => $"/input/{action}";
    private static string actionToAddress(VRChatAxesInput action) => $"/input/{action}";

    private async void sendAndReset(VRChatButtonInput action)
    {
        oscClient.Send(actionToAddress(action), 1);
        await Task.Delay(15);
        oscClient.Send(actionToAddress(action), 0);
    }

    internal void ResetAll()
    {
        Viseme = default;
        Voice = 0;
        GestureTypeLeft = default;
        GestureTypeRight = default;
        GestureLeftWeight = 0;
        GestureRightWeight = 0;
        AngularY = 0;
        VelocityX = 0;
        VelocityY = 0;
        VelocityZ = 0;
        Upright = 0;
        Grounded = false;
        Seated = false;
        AFK = false;
        TrackingType = default;
        IsVR = false;
        IsMuted = false;
        InStation = false;
        Earmuffs = false;
        ScaleModified = false;
        ScaleFactor = 0;
        ScaleFactorInverse = 0;
        EyeHeightAsMeters = 0;
        EyeHeightAsPercent = 0;

        if (!hasChanged) return;

        StopMoveForward();
        StopMoveBackward();
        StopMoveLeft();
        StopMoveRight();
        StopLookLeft();
        StopLookRight();
        StopRun();
        MoveHorizontal(0);
        MoveVertical(0);
        LookHorizontal(0);
    }

    public void MoveForward()
    {
        oscClient.Send(actionToAddress(VRChatButtonInput.MoveForward), 1);
        hasChanged = true;
    }

    public void StopMoveForward()
    {
        oscClient.Send(actionToAddress(VRChatButtonInput.MoveForward), 0);
    }

    public void MoveBackward()
    {
        oscClient.Send(actionToAddress(VRChatButtonInput.MoveBackward), 1);
        hasChanged = true;
    }

    public void StopMoveBackward()
    {
        oscClient.Send(actionToAddress(VRChatButtonInput.MoveBackward), 0);
    }

    public void MoveLeft()
    {
        oscClient.Send(actionToAddress(VRChatButtonInput.MoveLeft), 1);
        hasChanged = true;
    }

    public void StopMoveLeft()
    {
        oscClient.Send(actionToAddress(VRChatButtonInput.MoveLeft), 0);
    }

    public void MoveRight()
    {
        oscClient.Send(actionToAddress(VRChatButtonInput.MoveRight), 1);
        hasChanged = true;
    }

    public void StopMoveRight()
    {
        oscClient.Send(actionToAddress(VRChatButtonInput.MoveRight), 0);
    }

    public void LookLeft()
    {
        oscClient.Send(actionToAddress(VRChatButtonInput.LookLeft), 1);
        hasChanged = true;
    }

    public void StopLookLeft()
    {
        oscClient.Send(actionToAddress(VRChatButtonInput.LookLeft), 0);
    }

    public void LookRight()
    {
        oscClient.Send(actionToAddress(VRChatButtonInput.LookRight), 1);
        hasChanged = true;
    }

    public void StopLookRight()
    {
        oscClient.Send(actionToAddress(VRChatButtonInput.LookRight), 0);
    }

    public void Jump()
    {
        sendAndReset(VRChatButtonInput.Jump);
    }

    public void Run()
    {
        oscClient.Send(actionToAddress(VRChatButtonInput.Run), 1);
        hasChanged = true;
    }

    public void StopRun()
    {
        oscClient.Send(actionToAddress(VRChatButtonInput.Run), 0);
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
        if (IsMuted) return;

        ToggleVoice();
    }

    public void UnMute()
    {
        if (!IsMuted) return;

        ToggleVoice();
    }

    public void PushToTalk(bool active)
    {
        oscClient.Send(actionToAddress(VRChatButtonInput.Voice), active ? 1 : 0);
    }

    public void MoveVertical(float value)
    {
        oscClient.Send(actionToAddress(VRChatAxesInput.Vertical), value);
        hasChanged = true;
    }

    public void MoveHorizontal(float value)
    {
        oscClient.Send(actionToAddress(VRChatAxesInput.Horizontal), value);
        hasChanged = true;
    }

    public void LookHorizontal(float value)
    {
        oscClient.Send(actionToAddress(VRChatAxesInput.LookHorizontal), value);
        hasChanged = true;
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

public enum GestureType
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