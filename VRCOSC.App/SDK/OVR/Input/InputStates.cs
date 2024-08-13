// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.OVR.Input;

public class InputStates
{
    public Button A { get; } = new();
    public Button B { get; } = new();

    public bool StickTouched { get; internal set; }
    public bool PadTouched { get; internal set; }

    public float IndexFinger { get; internal set; }
    public float MiddleFinger { get; internal set; }
    public float RingFinger { get; internal set; }
    public float PinkyFinger { get; internal set; }

    public bool ThumbUp => !ThumbDown;
    public bool ThumbDown => A.Touched || B.Touched || StickTouched || PadTouched;
}
