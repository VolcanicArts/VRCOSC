// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;

namespace VRCOSC.App.OpenVR.Device;

public record DeviceInput
{
    public required Vector2Input Stick { get; init; }
    public required Triggers Triggers { get; init; }
    public required Buttons Buttons { get; init; }
    public required Vector1Input Grip { get; init; }
    public required Vector2Input Pad { get; init; }
    public required Skeleton Skeleton { get; init; }
    public required BoolInput System { get; init; }
    public required BoolInput Menu { get; init; }
}

public record BoolInput(bool Touch, bool Click);

public record Vector2Input(Vector2 Position, bool Touch, bool Click) : BoolInput(Touch, Click);

public record Vector1Input(float Depth, bool Touch, bool Click) : BoolInput(Touch, Click);

public record Skeleton(float Index, float Middle, float Ring, float Pinky);

public record Buttons(BoolInput North, BoolInput East, BoolInput South, BoolInput West);

public record Triggers(Vector1Input Trigger, BoolInput Bumper);