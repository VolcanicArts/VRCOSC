// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Threading.Tasks;
using VRCOSC.App.OSC.VRChat;
using VRCOSC.App.Utils;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace VRCOSC.App.SDK.VRChat;

public sealed class Player
{
    public Viseme Viseme => (Viseme)getParameter<int>(VRChatAvatarParameter.Viseme);
    public float Voice => getParameter<float>(VRChatAvatarParameter.Voice);
    public GestureType GestureTypeLeft => (GestureType)getParameter<int>(VRChatAvatarParameter.GestureLeft);
    public GestureType GestureTypeRight => (GestureType)getParameter<int>(VRChatAvatarParameter.GestureRight);
    public float GestureLeftWeight => getParameter<float>(VRChatAvatarParameter.GestureLeftWeight);
    public float GestureRightWeight => getParameter<float>(VRChatAvatarParameter.GestureRightWeight);
    public float AngularY => getParameter<float>(VRChatAvatarParameter.AngularY);
    public float VelocityX => getParameter<float>(VRChatAvatarParameter.VelocityX);
    public float VelocityY => getParameter<float>(VRChatAvatarParameter.VelocityY);
    public float VelocityZ => getParameter<float>(VRChatAvatarParameter.VelocityZ);
    public float Upright => getParameter<float>(VRChatAvatarParameter.Upright);
    public bool Grounded => getParameter<bool>(VRChatAvatarParameter.Grounded);
    public bool Seated => getParameter<bool>(VRChatAvatarParameter.Seated);
    public bool AFK => getParameter<bool>(VRChatAvatarParameter.AFK);
    public TrackingType TrackingType => (TrackingType)getParameter<int>(VRChatAvatarParameter.TrackingType);
    public bool IsVR => getParameter<int>(VRChatAvatarParameter.VRMode) == 1;
    public bool IsMuted => getParameter<bool>(VRChatAvatarParameter.MuteSelf);
    public bool InStation => getParameter<bool>(VRChatAvatarParameter.InStation);
    public bool Earmuffs => getParameter<bool>(VRChatAvatarParameter.Earmuffs);
    public bool ScaleModified => getParameter<bool>(VRChatAvatarParameter.ScaleModified);
    public float ScaleFactor => getParameter<float>(VRChatAvatarParameter.ScaleFactor);
    public float ScaleFactorInverse => getParameter<float>(VRChatAvatarParameter.ScaleFactorInverse);
    public float EyeHeightAsMeters => getParameter<float>(VRChatAvatarParameter.EyeHeightAsMeters);
    public float EyeHeightAsPercent => getParameter<float>(VRChatAvatarParameter.EyeHeightAsPercent);

    private readonly VRChatOSCClient oscClient;

    internal Player(VRChatOSCClient oscClient)
    {
        this.oscClient = oscClient;
    }

    private static string actionToAddress(VRChatButtonInput action) => $"/input/{action}";
    private static string actionToAddress(VRChatAxesInput action) => $"/input/{action}";
    private static T getParameter<T>(VRChatAvatarParameter parameterName) where T : unmanaged => AppManager.GetInstance().GetParameter<T>(parameterName.ToString())?.GetValue<T>() ?? default;

    private async Task sendAndReset(VRChatButtonInput action)
    {
        oscClient.Send(actionToAddress(action), 1);
        await Task.Delay(50);
        oscClient.Send(actionToAddress(action), 0);
    }

    internal void ResetAll()
    {
        StopMoveForward();
        StopMoveBackward();
        StopMoveLeft();
        StopMoveRight();
        StopLookLeft();
        StopLookRight();
        StopRun();
        MoveHorizontal(0f);
        MoveVertical(0f);
        LookHorizontal(0f);
        LookVertical(0f);
    }

    public void MoveForward() => oscClient.Send(actionToAddress(VRChatButtonInput.MoveForward), 1);
    public void StopMoveForward() => oscClient.Send(actionToAddress(VRChatButtonInput.MoveForward), 0);

    public void MoveBackward() => oscClient.Send(actionToAddress(VRChatButtonInput.MoveBackward), 1);
    public void StopMoveBackward() => oscClient.Send(actionToAddress(VRChatButtonInput.MoveBackward), 0);

    public void MoveLeft() => oscClient.Send(actionToAddress(VRChatButtonInput.MoveLeft), 1);
    public void StopMoveLeft() => oscClient.Send(actionToAddress(VRChatButtonInput.MoveLeft), 0);

    public void MoveRight() => oscClient.Send(actionToAddress(VRChatButtonInput.MoveRight), 1);
    public void StopMoveRight() => oscClient.Send(actionToAddress(VRChatButtonInput.MoveRight), 0);

    public void LookLeft() => oscClient.Send(actionToAddress(VRChatButtonInput.LookLeft), 1);
    public void StopLookLeft() => oscClient.Send(actionToAddress(VRChatButtonInput.LookLeft), 0);

    public void LookRight() => oscClient.Send(actionToAddress(VRChatButtonInput.LookRight), 1);
    public void StopLookRight() => oscClient.Send(actionToAddress(VRChatButtonInput.LookRight), 0);

    public void Jump() => sendAndReset(VRChatButtonInput.Jump).Forget();

    public void Run() => oscClient.Send(actionToAddress(VRChatButtonInput.Run), 1);
    public void StopRun() => oscClient.Send(actionToAddress(VRChatButtonInput.Run), 0);

    public void ComfortLeft() => sendAndReset(VRChatButtonInput.ComfortLeft).Forget();
    public void ComfortRight() => sendAndReset(VRChatButtonInput.ComfortRight).Forget();

    public void DropRight() => sendAndReset(VRChatButtonInput.DropRight).Forget();
    public void UseRight() => sendAndReset(VRChatButtonInput.UseRight).Forget();
    public void GrabRight() => sendAndReset(VRChatButtonInput.GrabRight).Forget();

    public void DropLeft() => sendAndReset(VRChatButtonInput.DropLeft).Forget();
    public void UseLeft() => sendAndReset(VRChatButtonInput.UseLeft).Forget();
    public void GrabLeft() => sendAndReset(VRChatButtonInput.GrabLeft).Forget();

    public void EnableSafeMode() => sendAndReset(VRChatButtonInput.PanicButton).Forget();

    public void ToggleLeftQuickMenu() => sendAndReset(VRChatButtonInput.QuickMenuToggleLeft).Forget();
    public void ToggleRightQuickMenu() => sendAndReset(VRChatButtonInput.QuickMenuToggleRight).Forget();

    public void ToggleVoice() => sendAndReset(VRChatButtonInput.Voice).Forget();
    public void PushToTalk(bool active) => oscClient.Send(actionToAddress(VRChatButtonInput.Voice), active ? 1 : 0);

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

    public void MoveVertical(float value) => oscClient.Send(actionToAddress(VRChatAxesInput.Vertical), value);
    public void MoveHorizontal(float value) => oscClient.Send(actionToAddress(VRChatAxesInput.Horizontal), value);
    public void LookHorizontal(float value) => oscClient.Send(actionToAddress(VRChatAxesInput.LookHorizontal), value);
    public void LookVertical(float value) => oscClient.Send(actionToAddress(VRChatAxesInput.LookVertical), value);
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
    LookVertical,
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