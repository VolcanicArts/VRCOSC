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
            Logger.Log($"Updating {serialNumber} to role {deviceRole}");
        }
        else
        {
            Logger.Log($"Adding {serialNumber} as role {deviceRole}");
        }

        switch (deviceRole)
        {
            case DeviceRole.Head:
                TrackedDevices[serialNumber] = new HMD
                {
                    SerialNumber = serialNumber,
                    Index = index,
                    Role = deviceRole
                };
                return;

            case DeviceRole.LeftHand or DeviceRole.RightHand:
                TrackedDevices[serialNumber] = new Controller
                {
                    SerialNumber = serialNumber,
                    Index = index,
                    Role = deviceRole
                };
                return;

            default:
                TrackedDevices[serialNumber] = new TrackedDevice
                {
                    SerialNumber = serialNumber,
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
        TrackedDevice newDevice = Activator.CreateInstance<T>();
        newDevice.SerialNumber = serialNumber;
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

    // YEUSEPE: The code has been rewritten to make it easier to work with and avoid repeating the same steps for left and right controllers.
    // Here's what was changed:
    // 1. A single method called `AuditController` now handles both left and right controllers. 
    //    - You tell it whether to check the left or right controller by passing the role (e.g., LeftHand or RightHand) and a hint ("left" or "right").
    // 2. The method `HandleSingleController` is used when no controllers are found. It checks for a single connected controller and adds it if valid.
    // 3. The method `TryAddTrackedDevice` is used to add a controller. 
    //    - It checks if the controller is valid (not null or empty) before adding it, making the code safer and preventing crashes.
    // 4. The methods for checking left and right controllers (`AuditLeftController` and `AuditRightController`) now just call `AuditController` with the correct settings for left or right.
    // This makes the code easier to understand, avoids repeating the same logic, and ensures it works correctly for both controllers.


    private void AuditController(ETrackedControllerRole role, DeviceRole deviceRole, string hint)
    {
        var controllerIndexes = OVRHelper.GetAllControllersFromHint(hint)?.ToList();

        if (controllerIndexes == null || controllerIndexes.Count == 0)
        {
            HandleSingleController(role, deviceRole);
        }
        else
        {
            foreach (var index in controllerIndexes)
            {
                TryAddTrackedDevice(index, deviceRole);
            }
        }
    }

    private void HandleSingleController(ETrackedControllerRole role, DeviceRole deviceRole)
    {
        if (OpenVR.System == null) return;

        var controllerIndex = OpenVR.System.GetTrackedDeviceIndexForControllerRole(role);
        if (controllerIndex == OpenVR.k_unTrackedDeviceIndexInvalid) return;

        TryAddTrackedDevice(controllerIndex, deviceRole);
    }

    private void TryAddTrackedDevice(uint controllerIndex, DeviceRole role)
    {
        if (OpenVR.System == null) return;

        var serialNumber = OVRHelper.GetStringTrackedDeviceProperty(controllerIndex, ETrackedDeviceProperty.Prop_SerialNumber_String);

        if (!string.IsNullOrEmpty(serialNumber))
        {
            addTrackedDevice<Controller>(serialNumber, role, controllerIndex);
        }
    }

    // Usage examples:
    private void auditLeftController()
    {
        AuditController(ETrackedControllerRole.LeftHand, DeviceRole.LeftHand, "left");
    }

    private void auditRightController()
    {
        AuditController(ETrackedControllerRole.RightHand, DeviceRole.RightHand, "right");
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
