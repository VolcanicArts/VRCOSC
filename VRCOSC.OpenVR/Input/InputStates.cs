namespace VRCOSC.OpenVR.Input;

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
