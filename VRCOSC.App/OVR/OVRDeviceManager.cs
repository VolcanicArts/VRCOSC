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

    public void Reset()
    {
        TrackedDevices.Clear();
    }

    private void addTrackedDevice<T>(string serialNumber, uint index, DeviceRole initialRole) where T : TrackedDevice
    {
        if (TrackedDevices.TryGetValue(serialNumber, out var existingDevice))
        {
            var existingRole = existingDevice.Role;

            TrackedDevice replacementDevice = Activator.CreateInstance<T>();
            replacementDevice.SerialNumber = serialNumber;
            replacementDevice.Index = index;
            replacementDevice.Role = existingRole;

            TrackedDevices[serialNumber] = replacementDevice;
        }
        else
        {
            TrackedDevice newDevice = Activator.CreateInstance<T>();
            newDevice.SerialNumber = serialNumber;
            newDevice.Index = index;
            newDevice.Role = initialRole;

            TrackedDevices.Add(serialNumber, newDevice);
        }
    }

    private void auditHMD()
    {
        var connectedHMDIndex = OVRHelper.GetIndexForTrackedDeviceClass(ETrackedDeviceClass.HMD);
        if (connectedHMDIndex == OpenVR.k_unTrackedDeviceIndexInvalid) return;

        var connectedHMDSerial = OVRHelper.GetStringTrackedDeviceProperty(connectedHMDIndex, ETrackedDeviceProperty.Prop_SerialNumber_String);

        addTrackedDevice<HMD>(connectedHMDSerial, connectedHMDIndex, DeviceRole.Head);
    }

    private void auditLeftController()
    {
        var connectedLeftControllersIndexes = OVRHelper.GetAllControllersFromHint("left").ToList();

        if (connectedLeftControllersIndexes.Count == 0)
        {
            var connectedLeftControllerIndex = OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
            if (connectedLeftControllerIndex == OpenVR.k_unTrackedDeviceIndexInvalid) return;

            var connectedLeftControllerSerial = OVRHelper.GetStringTrackedDeviceProperty(connectedLeftControllerIndex, ETrackedDeviceProperty.Prop_SerialNumber_String);

            addTrackedDevice<Controller>(connectedLeftControllerSerial, connectedLeftControllerIndex, DeviceRole.LeftHand);
        }
        else
        {
            foreach (var connectedLeftControllerIndex in connectedLeftControllersIndexes)
            {
                var connectedLeftControllerSerial = OVRHelper.GetStringTrackedDeviceProperty(connectedLeftControllerIndex, ETrackedDeviceProperty.Prop_SerialNumber_String);

                addTrackedDevice<Controller>(connectedLeftControllerSerial, connectedLeftControllerIndex, DeviceRole.LeftHand);
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

            addTrackedDevice<Controller>(connectedRightControllerSerial, connectedRightControllerIndex, DeviceRole.RightHand);
        }
        else
        {
            foreach (var connectedRightControllerIndex in connectedRightControllersIndexes)
            {
                var connectedRightControllerSerial = OVRHelper.GetStringTrackedDeviceProperty(connectedRightControllerIndex, ETrackedDeviceProperty.Prop_SerialNumber_String);

                addTrackedDevice<Controller>(connectedRightControllerSerial, connectedRightControllerIndex, DeviceRole.RightHand);
            }
        }
    }

    private void auditGenericTrackedDevices()
    {
        var connectedGenericDeviceIndexes = OVRHelper.GetIndexesForTrackedDeviceClass(ETrackedDeviceClass.GenericTracker);

        foreach (var connectedGenericDeviceIndex in connectedGenericDeviceIndexes)
        {
            var connectedGenericDeviceSerial = OVRHelper.GetStringTrackedDeviceProperty(connectedGenericDeviceIndex, ETrackedDeviceProperty.Prop_SerialNumber_String);

            addTrackedDevice<TrackedDevice>(connectedGenericDeviceSerial, connectedGenericDeviceIndex, DeviceRole.Unset);
        }
    }
}
