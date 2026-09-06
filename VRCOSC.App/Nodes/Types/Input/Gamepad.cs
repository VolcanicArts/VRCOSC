// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;
using VRCOSC.App.Inputs;

namespace VRCOSC.App.Nodes.Types.Input;

[Node("Gamepad Source", "Input/Gamepad")]
public sealed class GamepadSourceNode() : ValueSourceNode<Gamepad>("Gamepad")
{
    private GlobalInputHandler globalInputHandler => AppManager.GetInstance().GlobalInputHandler;

    public GlobalStore<Gamepad> GamepadStore = new();

    [InputMode(InputModes.Inline)]
    public ValueInput<uint> DeviceId = new("Id");

    protected override Gamepad ComputeValue(IPulseContext c) => globalInputHandler.GetGamepad(DeviceId.Read(c));
}

[Node("Gamepad Rumble", "Input/Gamepad")]
public sealed class GamepadSetVibrationNode : ActionNode
{
    private GlobalInputHandler globalInputHandler => AppManager.GetInstance().GlobalInputHandler;

    public ValueInput<Gamepad> Gamepad = new();
    public ValueInput<float> IntensityHeavy = new();
    public ValueInput<float> IntensityLight = new();
    public ValueInput<int> Duration = new("Duration (ms)");

    protected override void DoAction(IPulseContext c)
    {
        var gamepad = Gamepad.Read(c);
        var intensityHeavy = float.Clamp(IntensityHeavy.Read(c), 0f, 1f);
        var intensityLight = float.Clamp(IntensityLight.Read(c), 0f, 1f);
        var duration = (uint)int.Clamp(Duration.Read(c), 0, int.MaxValue);

        var convertedIntensityHeavy = (ushort)(intensityHeavy * ushort.MaxValue);
        var convertedIntensityLight = (ushort)(intensityLight * ushort.MaxValue);

        globalInputHandler.RumbleGamepad(gamepad, convertedIntensityHeavy, convertedIntensityLight, duration);
    }
}

public abstract class GamepadConsumeNode() : ValueConsumeNode<Gamepad>(nameof(Gamepad)), IContinuousNode
{
    public int UpdateOffset => 0;

    protected override void ConsumeValue(Gamepad value, IPulseContext c)
    {
        if (value is null) return;

        ConsumeGamepad(value, c);
    }

    protected abstract void ConsumeGamepad(Gamepad gamepad, IPulseContext c);
}

[Node("Gamepad Left Stick", "Input/Gamepad")]
public sealed class GamepadLeftStickNode : GamepadConsumeNode
{
    public ValueOutput<Vector2> Position = new();
    public ValueOutput<bool> Click = new();

    protected override void ConsumeGamepad(Gamepad gamepad, IPulseContext c)
    {
        Position.Write(gamepad.LeftStick.Position, c);
        Click.Write(gamepad.LeftStick.Click, c);
    }
}

[Node("Gamepad Right Stick", "Input/Gamepad")]
public sealed class GamepadRightStickNode : GamepadConsumeNode
{
    public ValueOutput<Vector2> Position = new();
    public ValueOutput<bool> Click = new();

    protected override void ConsumeGamepad(Gamepad gamepad, IPulseContext c)
    {
        Position.Write(gamepad.RightStick.Position, c);
        Click.Write(gamepad.RightStick.Click, c);
    }
}

[Node("Gamepad Paddles", "Input/Gamepad")]
public sealed class GamepadPaddlesNode : GamepadConsumeNode
{
    public ValueOutput<bool> Left1 = new();
    public ValueOutput<bool> Left2 = new();
    public ValueOutput<bool> Right1 = new();
    public ValueOutput<bool> Right2 = new();

    protected override void ConsumeGamepad(Gamepad gamepad, IPulseContext c)
    {
        Left1.Write(gamepad.LeftPaddle1, c);
        Left2.Write(gamepad.LeftPaddle2, c);
        Right1.Write(gamepad.RightPaddle1, c);
        Right2.Write(gamepad.RightPaddle2, c);
    }
}

[Node("Gamepad Triggers", "Input/Gamepad")]
public sealed class GamepadTriggersNode : GamepadConsumeNode
{
    public ValueOutput<float> Left = new();
    public ValueOutput<float> Right = new();

    protected override void ConsumeGamepad(Gamepad gamepad, IPulseContext c)
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

    protected override void ConsumeGamepad(Gamepad gamepad, IPulseContext c)
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

    protected override void ConsumeGamepad(Gamepad gamepad, IPulseContext c)
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
    public ValueOutput<bool> South = new();
    public ValueOutput<bool> East = new();
    public ValueOutput<bool> West = new();
    public ValueOutput<bool> North = new();
    public ValueOutput<bool> Start = new();
    public ValueOutput<bool> Back = new();
    public ValueOutput<bool> Guide = new();

    protected override void ConsumeGamepad(Gamepad gamepad, IPulseContext c)
    {
        South.Write(gamepad.South, c);
        East.Write(gamepad.East, c);
        West.Write(gamepad.West, c);
        North.Write(gamepad.North, c);
        Start.Write(gamepad.Start, c);
        Back.Write(gamepad.Back, c);
        Guide.Write(gamepad.Guide, c);
    }
}