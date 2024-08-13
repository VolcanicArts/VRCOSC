using System.Collections.Generic;
using System.Linq;
using Valve.VR;
using VRCOSC.App.SDK.OVR.Device;

namespace VRCOSC.App.SDK.OVR;

internal static class OVRDeviceManager
{
    private static readonly Dictionary<string, DeviceRole> device_roles = new();
    private static readonly Dictionary<string, TrackedDevice> tracked_devices = new();

    private static readonly object device_roles_lock = new();

    public static TrackedDevice? GetTrackedDevice(DeviceRole role)
    {
        lock (device_roles_lock)
        {
            return device_roles.Where(pair => pair.Value == role).Select(pair => tracked_devices[pair.Key]).Where(trackedDevice => trackedDevice.IsConnected).MaxBy(trackedDevice => trackedDevice.Index);
        }
    }

    public static void Update()
    {
        auditHMD();
        auditLeftController();
        auditRightController();
        auditGenericTrackedDevices();

        foreach (var trackedDevice in tracked_devices.Values)
        {
            trackedDevice.Update();
        }
    }

    public static void Reset()
    {
        tracked_devices.Clear();
    }

    private static void auditHMD()
    {
        var connectedHMDIndex = OVRHelper.GetIndexForTrackedDeviceClass(ETrackedDeviceClass.HMD);
        if (connectedHMDIndex == OpenVR.k_unTrackedDeviceIndexInvalid) return;

        var connectedHMDSerial = OVRHelper.GetStringTrackedDeviceProperty(connectedHMDIndex, ETrackedDeviceProperty.Prop_SerialNumber_String);

        tracked_devices.TryAdd(connectedHMDSerial, new HMD
        {
            Index = connectedHMDIndex,
            SerialNumber = connectedHMDSerial
        });

        lock (device_roles_lock)
        {
            device_roles.TryAdd(connectedHMDSerial, DeviceRole.Head);
        }
    }

    private static void auditLeftController()
    {
        var connectedLeftControllersIndexes = OVRHelper.GetAllControllersFromHint("left").ToList();

        if (connectedLeftControllersIndexes.Count == 0)
        {
            var connectedLeftControllerIndex = OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
            if (connectedLeftControllerIndex == OpenVR.k_unTrackedDeviceIndexInvalid) return;

            var connectedLeftControllerSerial = OVRHelper.GetStringTrackedDeviceProperty(connectedLeftControllerIndex, ETrackedDeviceProperty.Prop_SerialNumber_String);

            tracked_devices.TryAdd(connectedLeftControllerSerial, new Controller
            {
                Index = connectedLeftControllerIndex,
                SerialNumber = connectedLeftControllerSerial
            });

            lock (device_roles_lock)
            {
                device_roles.TryAdd(connectedLeftControllerSerial, DeviceRole.LeftHand);
            }
        }
        else
        {
            foreach (var connectedLeftControllerIndex in connectedLeftControllersIndexes)
            {
                var connectedLeftControllerSerial = OVRHelper.GetStringTrackedDeviceProperty(connectedLeftControllerIndex, ETrackedDeviceProperty.Prop_SerialNumber_String);

                tracked_devices.TryAdd(connectedLeftControllerSerial, new Controller
                {
                    Index = connectedLeftControllerIndex,
                    SerialNumber = connectedLeftControllerSerial
                });

                lock (device_roles_lock)
                {
                    device_roles.TryAdd(connectedLeftControllerSerial, DeviceRole.LeftHand);
                }
            }
        }
    }

    private static void auditRightController()
    {
        var connectedRightControllersIndexes = OVRHelper.GetAllControllersFromHint("right").ToList();

        if (connectedRightControllersIndexes.Count == 0)
        {
            var connectedRightControllerIndex = OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
            if (connectedRightControllerIndex == OpenVR.k_unTrackedDeviceIndexInvalid) return;

            var connectedRightControllerSerial = OVRHelper.GetStringTrackedDeviceProperty(connectedRightControllerIndex, ETrackedDeviceProperty.Prop_SerialNumber_String);

            tracked_devices.TryAdd(connectedRightControllerSerial, new Controller
            {
                Index = connectedRightControllerIndex,
                SerialNumber = connectedRightControllerSerial
            });

            lock (device_roles_lock)
            {
                device_roles.TryAdd(connectedRightControllerSerial, DeviceRole.RightHand);
            }
        }
        else
        {
            foreach (var connectedRightControllerIndex in connectedRightControllersIndexes)
            {
                var connectedRightControllerSerial = OVRHelper.GetStringTrackedDeviceProperty(connectedRightControllerIndex, ETrackedDeviceProperty.Prop_SerialNumber_String);

                tracked_devices.TryAdd(connectedRightControllerSerial, new Controller
                {
                    Index = connectedRightControllerIndex,
                    SerialNumber = connectedRightControllerSerial
                });

                lock (device_roles_lock)
                {
                    device_roles.TryAdd(connectedRightControllerSerial, DeviceRole.RightHand);
                }
            }
        }
    }

    private static void auditGenericTrackedDevices()
    {
        var connectedGenericDeviceIndexes = OVRHelper.GetIndexesForTrackedDeviceClass(ETrackedDeviceClass.GenericTracker);

        foreach (var connectedGenericDeviceIndex in connectedGenericDeviceIndexes)
        {
            var connectedGenericDeviceSerial = OVRHelper.GetStringTrackedDeviceProperty(connectedGenericDeviceIndex, ETrackedDeviceProperty.Prop_SerialNumber_String);

            tracked_devices.TryAdd(connectedGenericDeviceSerial, new TrackedDevice
            {
                Index = connectedGenericDeviceIndex,
                SerialNumber = connectedGenericDeviceSerial
            });
        }
    }
}
