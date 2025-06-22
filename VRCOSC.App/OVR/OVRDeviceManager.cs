// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Linq;
using Valve.VR;
using VRCOSC.App.OVR.Serialisation;
using VRCOSC.App.SDK.OVR;
using VRCOSC.App.SDK.OVR.Device;
using VRCOSC.App.Serialisation;
using VRCOSC.App.Utils;

namespace VRCOSC.App.OVR;

public class OVRDeviceManager
{
    private static OVRDeviceManager? instance;
    internal static OVRDeviceManager GetInstance() => instance ??= new OVRDeviceManager();

    // serial number : tracked device
    internal ObservableDictionary<string, TrackedDevice> TrackedDevices { get; } = new();

    private readonly object deviceRolesLock = new();

    private readonly SerialisationManager serialisationManager = new();

    internal OVRDeviceManager()
    {
        serialisationManager.RegisterSerialiser(1, new OVRDeviceManagerSerialiser(AppManager.GetInstance().Storage, this));
    }

    public void Deserialise() => serialisationManager.Deserialise();
    public void Serialise() => serialisationManager.Serialise();

    public TrackedDevice? GetTrackedDevice(DeviceRole role)
    {
        lock (deviceRolesLock)
        {
            return TrackedDevices.Values.Where(trackedDevice => trackedDevice.Role == role && trackedDevice.IsConnected).MaxBy(trackedDevice => trackedDevice.Index);
        }
    }

    public void AddOrUpdateDeviceRole(string serialNumber, DeviceRole deviceRole)
    {
        var index = OpenVR.k_unTrackedDeviceIndexInvalid;

        if (TrackedDevices.TryGetValue(serialNumber, out var device))
        {
            index = device.Index;
            if (device.Role == deviceRole) return;

            Logger.Log($"Updating {serialNumber} to role {deviceRole}");
        }
        else
        {
            Logger.Log($"Adding {serialNumber} as role {deviceRole}");
        }

        switch (deviceRole)
        {
            case DeviceRole.Head:
                TrackedDevices[serialNumber] = new HMD(serialNumber)
                {
                    Index = index,
                    Role = deviceRole
                };
                return;

            case DeviceRole.LeftHand or DeviceRole.RightHand:
                TrackedDevices[serialNumber] = new Controller(serialNumber)
                {
                    Index = index,
                    Role = deviceRole
                };
                return;

            default:
                TrackedDevices[serialNumber] = new TrackedDevice(serialNumber)
                {
                    Index = index,
                    Role = deviceRole
                };
                break;
        }
    }

    public void Update()
    {
        auditHMD();
        auditLeftController();
        auditRightController();
        auditGenericTrackedDevices();

        foreach (TrackedDevice trackedDevice in TrackedDevices.Values)
        {
            trackedDevice.Update();
        }
    }

    private void addTrackedDevice<T>(string serialNumber, DeviceRole deviceRole, uint index) where T : TrackedDevice
    {
        if (TrackedDevices.TryGetValue(serialNumber, out var existingDevice))
        {
            existingDevice.Index = index;
            return;
        }

        Logger.Log($"Auditing new tracked device {serialNumber} as {deviceRole}");
        TrackedDevice newDevice = (TrackedDevice)Activator.CreateInstance(typeof(T), args: [serialNumber])!;
        newDevice.Index = index;
        newDevice.Role = deviceRole;
        TrackedDevices.Add(serialNumber, newDevice);
    }

    private void auditHMD()
    {
        var connectedHMDIndex = OVRHelper.GetIndexForTrackedDeviceClass(ETrackedDeviceClass.HMD);
        if (connectedHMDIndex == OpenVR.k_unTrackedDeviceIndexInvalid) return;

        var connectedHMDSerial = OVRHelper.GetStringTrackedDeviceProperty(connectedHMDIndex, ETrackedDeviceProperty.Prop_SerialNumber_String);

        addTrackedDevice<HMD>(connectedHMDSerial, DeviceRole.Head, connectedHMDIndex);
    }

    private void auditLeftController()
    {
        var connectedLeftControllersIndexes = OVRHelper.GetAllControllersFromHint("left").ToList();

        if (connectedLeftControllersIndexes.Count == 0)
        {
            var connectedLeftControllerIndex = OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
            if (connectedLeftControllerIndex == OpenVR.k_unTrackedDeviceIndexInvalid) return;

            var connectedLeftControllerSerial = OVRHelper.GetStringTrackedDeviceProperty(connectedLeftControllerIndex, ETrackedDeviceProperty.Prop_SerialNumber_String);

            addTrackedDevice<Controller>(connectedLeftControllerSerial, DeviceRole.LeftHand, connectedLeftControllerIndex);
        }
        else
        {
            foreach (var connectedLeftControllerIndex in connectedLeftControllersIndexes)
            {
                var connectedLeftControllerSerial = OVRHelper.GetStringTrackedDeviceProperty(connectedLeftControllerIndex, ETrackedDeviceProperty.Prop_SerialNumber_String);

                addTrackedDevice<Controller>(connectedLeftControllerSerial, DeviceRole.LeftHand, connectedLeftControllerIndex);
            }
        }
    }

    private void auditRightController()
    {
        var connectedRightControllersIndexes = OVRHelper.GetAllControllersFromHint("right").ToList();

        if (connectedRightControllersIndexes.Count == 0)
        {
            var connectedRightControllerIndex = OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
            if (connectedRightControllerIndex == OpenVR.k_unTrackedDeviceIndexInvalid) return;

            var connectedRightControllerSerial = OVRHelper.GetStringTrackedDeviceProperty(connectedRightControllerIndex, ETrackedDeviceProperty.Prop_SerialNumber_String);

            addTrackedDevice<Controller>(connectedRightControllerSerial, DeviceRole.RightHand, connectedRightControllerIndex);
        }
        else
        {
            foreach (var connectedRightControllerIndex in connectedRightControllersIndexes)
            {
                var connectedRightControllerSerial = OVRHelper.GetStringTrackedDeviceProperty(connectedRightControllerIndex, ETrackedDeviceProperty.Prop_SerialNumber_String);

                addTrackedDevice<Controller>(connectedRightControllerSerial, DeviceRole.RightHand, connectedRightControllerIndex);
            }
        }
    }

    private void auditGenericTrackedDevices()
    {
        var connectedGenericDeviceIndexes = OVRHelper.GetIndexesForTrackedDeviceClass(ETrackedDeviceClass.GenericTracker);

        foreach (var connectedGenericDeviceIndex in connectedGenericDeviceIndexes)
        {
            var connectedGenericDeviceSerial = OVRHelper.GetStringTrackedDeviceProperty(connectedGenericDeviceIndex, ETrackedDeviceProperty.Prop_SerialNumber_String);

            addTrackedDevice<TrackedDevice>(connectedGenericDeviceSerial, DeviceRole.Unset, connectedGenericDeviceIndex);
        }
    }
}