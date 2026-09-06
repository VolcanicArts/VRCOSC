// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;

namespace VRCOSC.App.Inputs;

public class Gamepad
{
    public readonly uint Id;
    public readonly nint InstanceId;

    public GamepadStick LeftStick = new();
    public GamepadStick RightStick = new();

    public float LeftTrigger;
    public float RightTrigger;

    public bool LeftShoulder;
    public bool RightShoulder;

    public bool LeftPaddle1;
    public bool LeftPaddle2;
    public bool RightPaddle1;
    public bool RightPaddle2;

    public bool DPadUp;
    public bool DPadRight;
    public bool DPadDown;
    public bool DPadLeft;

    public bool Start;
    public bool Back;
    public bool Guide;

    public bool North;
    public bool East;
    public bool South;
    public bool West;

    public Gamepad(uint id, nint instanceId)
    {
        Id = id;
        InstanceId = instanceId;
    }
}

public class GamepadStick
{
    public Vector2 Position;
    public bool Click;
}