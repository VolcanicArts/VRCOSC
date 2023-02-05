using VRCOSC.Game.OpenVR.Input;

namespace VRCOSC.Game.OpenVR.Device;

public class Controller : OVRDevice
{
    public readonly InputStates Input = new();
}
