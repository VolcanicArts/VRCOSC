using Valve.VR;

namespace VRCOSC.OpenVR.Device;

public abstract class OVRDevice
{
    /// <summary>
    /// The OVR ID of the device
    /// </summary>
    public readonly uint Id;

    /// <summary>
    /// Whether the device is currently connected to this OVR session
    /// </summary>
    public bool IsConnected { get; private set; }

    /// <summary>
    /// Whether the device can provide battery information
    /// </summary>
    public bool ProvidesBatteryStatus { get; private set; }

    /// <summary>
    /// Whether the device is currently charging
    /// </summary>
    public bool IsCharging { get; private set; }

    /// <summary>
    /// The device's battery between 0 and 1
    /// </summary>
    public float BatteryPercentage { get; private set; }

    protected OVRDevice(uint id)
    {
        Id = id;
        Update();
    }

    public void Update()
    {
        IsConnected = Valve.VR.OpenVR.System.IsTrackedDeviceConnected(Id);
        ProvidesBatteryStatus = OVRHelper.GetBoolTrackedDeviceProperty(Id, ETrackedDeviceProperty.Prop_DeviceProvidesBatteryStatus_Bool);
        IsCharging = OVRHelper.GetBoolTrackedDeviceProperty(Id, ETrackedDeviceProperty.Prop_DeviceIsCharging_Bool);
        BatteryPercentage = OVRHelper.GetFloatTrackedDeviceProperty(Id, ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float);
    }
}
