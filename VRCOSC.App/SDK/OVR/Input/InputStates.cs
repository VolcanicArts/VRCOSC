// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

namespace VRCOSC.App.SDK.OVR.Input;

public class InputStates
{
    public Button A = new();
    public Button B = new();

    public bool StickTouched;

    public bool PadTouched;

    public float IndexFinger;
    public float MiddleFinger;
    public float RingFinger;
    public float PinkyFinger;

    public bool ThumbUp => !ThumbDown;
    public bool ThumbDown => A.Touched || B.Touched || StickTouched || PadTouched;
}
