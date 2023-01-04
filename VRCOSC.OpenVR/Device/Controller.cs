using VRCOSC.OpenVR.Input;

namespace VRCOSC.OpenVR.Device;

public class Controller : OVRDevice
{
    public readonly InputStates Input = new();
}
