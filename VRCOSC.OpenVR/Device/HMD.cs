namespace VRCOSC.OpenVR.Device;

public class HMD : OVRDevice
{
    // TODO Check if this override is actually needed
    protected override bool IsTrackedDeviceConnected() => Valve.VR.OpenVR.IsHmdPresent();
}
