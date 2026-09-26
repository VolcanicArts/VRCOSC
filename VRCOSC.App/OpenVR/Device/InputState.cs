// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;

namespace VRCOSC.App.OpenVR.Device;

public record DeviceInput
{
    public Vector2Input Stick;
    public Triggers Triggers;
    public Buttons Buttons;
    public Vector1Input Grip;
    public Vector2Input Pad;
    public Skeleton Skeleton;
    public BoolInput System;
    public BoolInput Menu;
}

public record BoolInput(bool Touch, bool Click);

public record Vector2Input(Vector2 Position, bool Touch, bool Click) : BoolInput(Touch, Click);

public record Vector1Input(float Depth, bool Touch, bool Click) : BoolInput(Touch, Click);

public record Skeleton(float Index, float Middle, float Ring, float Pinky);

public record Buttons(BoolInput North, BoolInput East, BoolInput South, BoolInput West);

public record Triggers(Vector1Input Trigger, BoolInput Bumper);