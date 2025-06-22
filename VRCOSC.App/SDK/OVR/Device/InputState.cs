// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Numerics;

namespace VRCOSC.App.SDK.OVR.Device;

public struct InputState
{
    public Stick Stick;
    public Trigger Trigger;
    public Button Primary;
    public Button Secondary;
    public Grip Grip;
    public Pad Pad;
    public Skeleton Skeleton;
}

public struct Button
{
    public bool Touch;
    public bool Click;

    public Button(bool touch, bool click)
    {
        Touch = touch;
        Click = click;
    }
}

public struct Grip
{
    public float Pull;
    public bool Click;

    public Grip(float pull, bool click)
    {
        Pull = pull;
        Click = click;
    }
}

public struct Trigger
{
    public float Pull;
    public bool Touch;
    public bool Click;

    public Trigger(float pull, bool touch, bool click)
    {
        Pull = pull;
        Touch = touch;
        Click = click;
    }
}

public struct Stick
{
    public Vector2 Position;
    public bool Touch;
    public bool Click;

    public Stick(Vector2 position, bool touch, bool click)
    {
        Position = position;
        Touch = touch;
        Click = click;
    }
}

public struct Pad
{
    public Vector2 Position;
    public bool Touch;
    public bool Click;

    public Pad(Vector2 position, bool touch, bool click)
    {
        Position = position;
        Touch = touch;
        Click = click;
    }
}

public struct Skeleton
{
    public float Index;
    public float Middle;
    public float Ring;
    public float Pinky;

    public Skeleton(float index, float middle, float ring, float pinky)
    {
        Index = index;
        Middle = middle;
        Ring = ring;
        Pinky = pinky;
    }
}