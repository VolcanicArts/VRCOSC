// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.UI.Input.XboxController;
using VRCOSC.App.Utils;

namespace VRCOSC.App.Nodes.Types.Input;

[StructLayout(LayoutKind.Sequential)]
public struct Gamepad
{
    public uint Index;

    public Vector2 LeftStickPos;
    public Vector2 RightStickPos;

    public bool LeftStickClick;
    public bool RightStickClick;

    public float LeftTrigger;
    public float RightTrigger;

    public bool LeftShoulder;
    public bool RightShoulder;

    public bool DPadUp;
    public bool DPadDown;
    public bool DPadLeft;
    public bool DPadRight;

    public bool Start;
    public bool Back;

    public bool A;
    public bool B;
    public bool X;
    public bool Y;
}

[Node("Gamepad Source", "Input/Gamepad")]
public sealed class GamepadSourceNode() : ValueComputeNode<Gamepad>("Gamepad"), IUpdateNode
{
    public int UpdateOffset => 0;

    public GlobalStore<Gamepad> GamepadStore = new();

    [InputMode(InputModes.Inline)]
    public ValueInput<uint> DeviceIndex = new();

    protected override Gamepad ComputeValue(IPulseContext c) => GamepadStore.Read(c);

    public void OnUpdate(IPulseContext c)
    {
        var deviceIndex = DeviceIndex.Read(c);

        if (PInvoke.XInputGetState(deviceIndex, out var state) != 0)
        {
            GamepadStore.Write(new Gamepad(), c);
            return;
        }

        var pad = state.Gamepad;

        var gamepad = new Gamepad
        {
            Index = deviceIndex,

            A = hasFlag(pad, XINPUT_GAMEPAD_BUTTON_FLAGS.XINPUT_GAMEPAD_A),
            B = hasFlag(pad, XINPUT_GAMEPAD_BUTTON_FLAGS.XINPUT_GAMEPAD_B),
            X = hasFlag(pad, XINPUT_GAMEPAD_BUTTON_FLAGS.XINPUT_GAMEPAD_X),
            Y = hasFlag(pad, XINPUT_GAMEPAD_BUTTON_FLAGS.XINPUT_GAMEPAD_Y),

            DPadUp = hasFlag(pad, XINPUT_GAMEPAD_BUTTON_FLAGS.XINPUT_GAMEPAD_DPAD_UP),
            DPadDown = hasFlag(pad, XINPUT_GAMEPAD_BUTTON_FLAGS.XINPUT_GAMEPAD_DPAD_DOWN),
            DPadLeft = hasFlag(pad, XINPUT_GAMEPAD_BUTTON_FLAGS.XINPUT_GAMEPAD_DPAD_LEFT),
            DPadRight = hasFlag(pad, XINPUT_GAMEPAD_BUTTON_FLAGS.XINPUT_GAMEPAD_DPAD_RIGHT),

            LeftShoulder = hasFlag(pad, XINPUT_GAMEPAD_BUTTON_FLAGS.XINPUT_GAMEPAD_LEFT_SHOULDER),
            RightShoulder = hasFlag(pad, XINPUT_GAMEPAD_BUTTON_FLAGS.XINPUT_GAMEPAD_RIGHT_SHOULDER),

            Start = hasFlag(pad, XINPUT_GAMEPAD_BUTTON_FLAGS.XINPUT_GAMEPAD_START),
            Back = hasFlag(pad, XINPUT_GAMEPAD_BUTTON_FLAGS.XINPUT_GAMEPAD_BACK),

            LeftStickClick = hasFlag(pad, XINPUT_GAMEPAD_BUTTON_FLAGS.XINPUT_GAMEPAD_LEFT_THUMB),
            RightStickClick = hasFlag(pad, XINPUT_GAMEPAD_BUTTON_FLAGS.XINPUT_GAMEPAD_RIGHT_THUMB),

            LeftStickPos = new Vector2(remapStick(pad.sThumbLX), remapStick(pad.sThumbLY)),
            RightStickPos = new Vector2(remapStick(pad.sThumbRX), remapStick(pad.sThumbRY)),

            LeftTrigger = remapTrigger(pad.bLeftTrigger),
            RightTrigger = remapTrigger(pad.bRightTrigger)
        };

        GamepadStore.Write(gamepad, c);
        return;
    }

    private static float remapStick(short value) => Interpolation.Map(value, short.MinValue, short.MaxValue, -1f, 1f);
    private static float remapTrigger(byte value) => Interpolation.Map(value, byte.MinValue, byte.MaxValue, 0f, 1f);
    private static bool hasFlag(XINPUT_GAMEPAD pad, XINPUT_GAMEPAD_BUTTON_FLAGS flag) => ((ushort)pad.wButtons & (ushort)flag) != 0;
}

[Node("Gamepad Set Vibration", "Input/Gamepad")]
public sealed class GamepadSetVibrationNode : ActionNode
{
    public ValueInput<Gamepad> Gamepad = new();
    public ValueInput<float> IntensityHeavy = new();
    public ValueInput<float> IntensityLight = new();

    protected override void DoAction(IPulseContext c)
    {
        var gamepad = Gamepad.Read(c);
        var intensityHeavy = float.Clamp(IntensityHeavy.Read(c), 0f, 1f);
        var intensityLight = float.Clamp(IntensityLight.Read(c), 0f, 1f);

        var vibration = new XINPUT_VIBRATION
        {
            wLeftMotorSpeed = (ushort)(intensityHeavy * ushort.MaxValue),
            wRightMotorSpeed = (ushort)(intensityLight * ushort.MaxValue)
        };

        PInvoke.XInputSetState(gamepad.Index, vibration);
    }
}

public abstract class GamepadConsumeNode() : ValueConsumeNode<Gamepad>(nameof(Gamepad)), IContinuousNode
{
    public int UpdateOffset => 0;

    protected override void ConsumeValue(Gamepad value, IPulseContext c) => ConsumeGampad(value, c);

    protected abstract void ConsumeGampad(Gamepad gamepad, IPulseContext c);
}

[Node("Gamepad Left Stick", "Input/Gamepad")]
public sealed class GamepadLeftStickNode : GamepadConsumeNode
{
    public ValueOutput<Vector2> Position = new();
    public ValueOutput<bool> Click = new();

    protected override void ConsumeGampad(Gamepad gamepad, IPulseContext c)
    {
        Position.Write(gamepad.LeftStickPos, c);
        Click.Write(gamepad.LeftStickClick, c);
    }
}

[Node("Gamepad Right Stick", "Input/Gamepad")]
public sealed class GamepadRightStickNode : GamepadConsumeNode
{
    public ValueOutput<Vector2> Position = new();
    public ValueOutput<bool> Click = new();

    protected override void ConsumeGampad(Gamepad gamepad, IPulseContext c)
    {
        Position.Write(gamepad.RightStickPos, c);
        Click.Write(gamepad.RightStickClick, c);
    }
}

[Node("Gamepad Triggers", "Input/Gamepad")]
public sealed class GamepadTriggersNode : GamepadConsumeNode
{
    public ValueOutput<float> Left = new();
    public ValueOutput<float> Right = new();

    protected override void ConsumeGampad(Gamepad gamepad, IPulseContext c)
    {
        Left.Write(gamepad.LeftTrigger, c);
        Right.Write(gamepad.RightTrigger, c);
    }
}

[Node("Gamepad Shoulders", "Input/Gamepad")]
public sealed class GamepadShouldersNode : GamepadConsumeNode
{
    public ValueOutput<bool> Left = new();
    public ValueOutput<bool> Right = new();

    protected override void ConsumeGampad(Gamepad gamepad, IPulseContext c)
    {
        Left.Write(gamepad.LeftShoulder, c);
        Right.Write(gamepad.RightShoulder, c);
    }
}

[Node("Gamepad DPad", "Input/Gamepad")]
public sealed class GamepadDPadNode : GamepadConsumeNode
{
    public ValueOutput<bool> Up = new();
    public ValueOutput<bool> Down = new();
    public ValueOutput<bool> Left = new();
    public ValueOutput<bool> Right = new();

    protected override void ConsumeGampad(Gamepad gamepad, IPulseContext c)
    {
        Up.Write(gamepad.DPadUp, c);
        Down.Write(gamepad.DPadDown, c);
        Left.Write(gamepad.DPadLeft, c);
        Right.Write(gamepad.DPadRight, c);
    }
}

[Node("Gamepad Buttons", "Input/Gamepad")]
public sealed class GamepadButtonsNode : GamepadConsumeNode
{
    public ValueOutput<bool> A = new();
    public ValueOutput<bool> B = new();
    public ValueOutput<bool> X = new();
    public ValueOutput<bool> Y = new();
    public ValueOutput<bool> Start = new();
    public ValueOutput<bool> Back = new();

    protected override void ConsumeGampad(Gamepad gamepad, IPulseContext c)
    {
        A.Write(gamepad.A, c);
        B.Write(gamepad.B, c);
        X.Write(gamepad.X, c);
        Y.Write(gamepad.Y, c);
        Start.Write(gamepad.Start, c);
        Back.Write(gamepad.Back, c);
    }
}