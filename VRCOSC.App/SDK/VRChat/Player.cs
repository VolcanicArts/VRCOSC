// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

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

    internal Player(VRChatOSCClient oscClient)
    {
        this.oscClient = oscClient;
    }

    private static string actionToAddress(VRChatButtonInput action) => $"/input/{action}";
    private static string actionToAddress(VRChatAxesInput action) => $"/input/{action}";

    private async Task sendAndReset(VRChatButtonInput action)
    {
        oscClient.Send(actionToAddress(action), 1);
        await Task.Delay(50);
        oscClient.Send(actionToAddress(action), 0);
    }

    internal void UpdateParameterValue(VRChatParameter parameter)
    {
        var name = parameter.Name;
        var type = parameter.Type;

        switch (name)
        {
            case nameof(VRChatAvatarParameter.Viseme) when type == ParameterType.Int:
                Viseme = (Viseme)parameter.GetValue<int>();
                break;

            case nameof(VRChatAvatarParameter.Voice) when type == ParameterType.Float:
                Voice = parameter.GetValue<float>();
                break;

            case nameof(VRChatAvatarParameter.GestureLeft) when type == ParameterType.Int:
                GestureTypeLeft = (GestureType)parameter.GetValue<int>();
                break;

            case nameof(VRChatAvatarParameter.GestureLeftWeight) when type == ParameterType.Float:
                GestureLeftWeight = parameter.GetValue<float>();
                break;

            case nameof(VRChatAvatarParameter.GestureRight) when type == ParameterType.Int:
                GestureTypeRight = (GestureType)parameter.GetValue<int>();
                break;

            case nameof(VRChatAvatarParameter.GestureRightWeight) when type == ParameterType.Float:
                GestureRightWeight = parameter.GetValue<float>();
                break;

            case nameof(VRChatAvatarParameter.AngularY) when type == ParameterType.Float:
                AngularY = parameter.GetValue<float>();
                break;

            case nameof(VRChatAvatarParameter.VelocityX) when type == ParameterType.Float:
                VelocityX = parameter.GetValue<float>();
                break;

            case nameof(VRChatAvatarParameter.VelocityY) when type == ParameterType.Float:
                VelocityY = parameter.GetValue<float>();
                break;

            case nameof(VRChatAvatarParameter.VelocityZ) when type == ParameterType.Float:
                VelocityZ = parameter.GetValue<float>();
                break;

            case nameof(VRChatAvatarParameter.Upright) when type == ParameterType.Float:
                Upright = parameter.GetValue<float>();
                break;

            case nameof(VRChatAvatarParameter.Grounded) when type == ParameterType.Bool:
                Grounded = parameter.GetValue<bool>();
                break;

            case nameof(VRChatAvatarParameter.Seated) when type == ParameterType.Bool:
                Seated = parameter.GetValue<bool>();
                break;

            case nameof(VRChatAvatarParameter.AFK) when type == ParameterType.Bool:
                AFK = parameter.GetValue<bool>();
                break;

            case nameof(VRChatAvatarParameter.TrackingType) when type == ParameterType.Int:
                TrackingType = (TrackingType)parameter.GetValue<int>();
                break;

            case nameof(VRChatAvatarParameter.VRMode) when type == ParameterType.Int:
                IsVR = parameter.GetValue<int>() == 1;
                break;

            case nameof(VRChatAvatarParameter.MuteSelf) when type == ParameterType.Bool:
                IsMuted = parameter.GetValue<bool>();
                break;

            case nameof(VRChatAvatarParameter.InStation) when type == ParameterType.Bool:
                InStation = parameter.GetValue<bool>();
                break;

            case nameof(VRChatAvatarParameter.Earmuffs) when type == ParameterType.Bool:
                Earmuffs = parameter.GetValue<bool>();
                break;

            case nameof(VRChatAvatarParameter.ScaleModified) when type == ParameterType.Bool:
                ScaleModified = parameter.GetValue<bool>();
                break;

            case nameof(VRChatAvatarParameter.ScaleFactor) when type == ParameterType.Float:
                ScaleFactor = parameter.GetValue<float>();
                break;

            case nameof(VRChatAvatarParameter.ScaleFactorInverse) when type == ParameterType.Float:
                ScaleFactorInverse = parameter.GetValue<float>();
                break;

            case nameof(VRChatAvatarParameter.EyeHeightAsMeters) when type == ParameterType.Float:
                EyeHeightAsMeters = parameter.GetValue<float>();
                break;

            case nameof(VRChatAvatarParameter.EyeHeightAsPercent) when type == ParameterType.Float:
                EyeHeightAsPercent = parameter.GetValue<float>();
                break;
        }
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