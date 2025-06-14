// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using Valve.VR;
using VRCOSC.App.Utils;

namespace VRCOSC.App.SDK.OVR.Device;

public class TrackedDevice
{
    public bool IsValid => Index != OpenVR.k_unTrackedDeviceIndexInvalid;

    /// <summary>
    /// The index of the device. This changes between sessions. Do not user this as a reference between sessions
    /// </summary>
    public uint Index { get; internal set; } = OpenVR.k_unTrackedDeviceIndexInvalid;

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

    /// <summary>
    /// The role of this device as set by the user
    /// </summary>
    public DeviceRole Role { get; internal set; }

    public Transform Transform { get; internal set; }

    internal void Update()
    {
        if (!IsValid) return;

        IsConnected = OpenVR.System.IsTrackedDeviceConnected(Index);
        ProvidesBatteryStatus = IsConnected && OVRHelper.GetBoolTrackedDeviceProperty(Index, ETrackedDeviceProperty.Prop_DeviceProvidesBatteryStatus_Bool);
        IsCharging = IsConnected && OVRHelper.GetBoolTrackedDeviceProperty(Index, ETrackedDeviceProperty.Prop_DeviceIsCharging_Bool);
        BatteryPercentage = MathF.Max(0f, IsConnected ? OVRHelper.GetFloatTrackedDeviceProperty(Index, ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float) : 0f);
        Transform = OVRHelper.GetTrackedPose(Index);
    }
}