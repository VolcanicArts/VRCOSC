// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.App.Utils;

namespace VRCOSC.App.OpenVR.Device;

public record TrackedDevice
{
    /// <summary>
    /// The index of the device. This changes between sessions. Do not use this as a reference
    /// </summary>
    public uint Index { get; }

    /// <summary>
    /// The serial number of this device. This is constant between sessions. Use this as a reference
    /// </summary>
    public string SerialNumber { get; }

    /// <summary>
    /// The ID of the dongle that this device is connected through
    /// </summary>
    public string DongleId { get; }

    /// <summary>
    /// The role of this device
    /// </summary>
    public DeviceRole Role { get; internal set; }

    /// <summary>
    /// Whether the device is currently connected to this OpenVR session
    /// </summary>
    public bool IsConnected { get; internal set; }

    /// <summary>
    /// Whether the device can provide battery information
    /// </summary>
    public bool ProvidesBatteryStatus { get; internal set; }

    /// <summary>
    /// Whether the device is currently charging
    /// </summary>
    public bool IsCharging { get; internal set; }

    /// <summary>
    /// The device's battery between 0 and 1
    /// </summary>
    public float BatteryPercentage { get; internal set; }

    /// <summary>
    /// The transform of this device
    /// </summary>
    public Transform Transform { get; internal set; } = Transform.Identity;

    public TrackedDevice(uint index, string serialNumber, string dongleId, DeviceRole role)
    {
        Index = index;
        SerialNumber = serialNumber;
        DongleId = dongleId;
        Role = role;
    }
}

public record HMD : TrackedDevice
{
    public HMD(uint index, string serialNumber, string dongleId, DeviceRole role)
        : base(index, serialNumber, dongleId, role)
    {
    }
}

public record Controller : TrackedDevice
{
    public InputState Input { get; internal set; }

    public Controller(uint index, string serialNumber, string dongleId, DeviceRole role)
        : base(index, serialNumber, dongleId, role)
    {
    }
}