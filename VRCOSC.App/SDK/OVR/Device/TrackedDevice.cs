// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using Valve.VR;

namespace VRCOSC.App.SDK.OVR.Device;

public class TrackedDevice
{
    /// <summary>
    /// The index of the device. This changes between sessions. Do not user this as a reference between sessions
    /// </summary>
    public uint Index { get; internal init; } = OpenVR.k_unTrackedDeviceIndexInvalid;

    /// <summary>
    /// The serial number of the device. This is unchanging and can be used as a reference between sessions
    /// </summary>
    public string SerialNumber { get; internal set; } = string.Empty;

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

    internal void Update()
    {
        IsConnected = OpenVR.System.IsTrackedDeviceConnected(Index);
        ProvidesBatteryStatus = OVRHelper.GetBoolTrackedDeviceProperty(Index, ETrackedDeviceProperty.Prop_DeviceProvidesBatteryStatus_Bool);
        IsCharging = OVRHelper.GetBoolTrackedDeviceProperty(Index, ETrackedDeviceProperty.Prop_DeviceIsCharging_Bool);
        BatteryPercentage = OVRHelper.GetFloatTrackedDeviceProperty(Index, ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float);
    }
}
