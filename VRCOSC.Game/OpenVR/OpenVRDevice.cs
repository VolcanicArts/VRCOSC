using Valve.VR;

namespace VRCOSC.Game.OpenVR;

public abstract class OpenVRDevice
{
    private readonly OpenVRInterface instance;

    public bool IsConnected { get; private set; }
    public bool CanProvideBatteryInfo { get; private set; }
    public bool IsCharging { get; private set; }
    public float BatteryPercentage { get; private set; }

    protected OpenVRDevice(OpenVRInterface instance)
    {
        this.instance = instance;
    }

    public void Update(uint id)
    {
        IsConnected = id != uint.MaxValue && IsTrackedDeviceConnected(id);
        CanProvideBatteryInfo = instance.GetBoolTrackedDeviceProperty(id, ETrackedDeviceProperty.Prop_DeviceProvidesBatteryStatus_Bool);
        IsCharging = instance.GetBoolTrackedDeviceProperty(id, ETrackedDeviceProperty.Prop_DeviceIsCharging_Bool);
        BatteryPercentage = instance.GetFloatTrackedDeviceProperty(id, ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float);
    }

    protected virtual bool IsTrackedDeviceConnected(uint id) => Valve.VR.OpenVR.System.IsTrackedDeviceConnected(id);
}

public class OpenVRHMD : OpenVRDevice
{
    public OpenVRHMD(OpenVRInterface instance)
        : base(instance)
    {
    }

    // TODO Check if this override is actually needed
    protected override bool IsTrackedDeviceConnected(uint _) => Valve.VR.OpenVR.IsHmdPresent();
}

public class OpenVRController : OpenVRDevice
{
    public readonly ControllerData Data = new();

    public OpenVRController(OpenVRInterface instance)
        : base(instance)
    {
    }
}

public class OpenVRTracker : OpenVRDevice
{
    public OpenVRTracker(OpenVRInterface instance)
        : base(instance)
    {
    }
}
