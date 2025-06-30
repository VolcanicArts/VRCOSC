// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Valve.VR;
using VRCOSC.App.SDK.OVR;
using VRCOSC.App.SDK.OVR.Device;
using VRCOSC.App.Utils;

namespace VRCOSC.App.OVR;

public class OVRDeviceManager
{
    private static readonly string openvrpathsfilelocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "openvr", "openvrpaths.vrpath");

    private static OVRDeviceManager? instance;
    internal static OVRDeviceManager GetInstance() => instance ??= new OVRDeviceManager();

    // qualified serial number : device role
    internal ConcurrentDictionary<string, DeviceRole> Roles { get; } = new();

    // serial number : tracked device
    internal ConcurrentDictionary<string, TrackedDevice> TrackedDevices { get; } = new();

    public string? RuntimePath { get; private set; }
    public string? LighthouseConsole => Path.Join(RuntimePath, "tools", "lighthouse", "bin", "win64", "lighthouse_console.exe");

    private readonly object deviceRolesLock = new();

    public TrackedDevice? GetTrackedDevice(string serialNumber)
    {
        lock (deviceRolesLock)
        {
            return TrackedDevices.Values.Where(trackedDevice => trackedDevice.SerialNumber == serialNumber && trackedDevice.IsConnected).MaxBy(trackedDevice => trackedDevice.Index);
        }
    }

    public TrackedDevice? GetTrackedDevice(DeviceRole role)
    {
        lock (deviceRolesLock)
        {
            return TrackedDevices.Values.Where(trackedDevice => trackedDevice.Role == role && trackedDevice.IsConnected).MaxBy(trackedDevice => trackedDevice.Index);
        }
    }

    private void addOrUpdateDeviceRole(string serialNumber, DeviceRole deviceRole, uint index)
    {
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

        var dongleId = OVRHelper.GetStringTrackedDeviceProperty(index, ETrackedDeviceProperty.Prop_ConnectedWirelessDongle_String);

        switch (deviceRole)
        {
            case DeviceRole.Head:
                TrackedDevices[serialNumber] = new HMD(serialNumber)
                {
                    Index = index,
                    DongleId = dongleId,
                    Role = deviceRole
                };
                return;

            case DeviceRole.LeftHand or DeviceRole.RightHand:
                TrackedDevices[serialNumber] = new Controller(serialNumber)
                {
                    Index = index,
                    DongleId = dongleId,
                    Role = deviceRole
                };
                return;

            default:
                TrackedDevices[serialNumber] = new TrackedDevice(serialNumber)
                {
                    Index = index,
                    DongleId = dongleId,
                    Role = deviceRole
                };
                break;
        }
    }

    public void Update()
    {
        getRolesFromSteamVR();
        auditHMD();
        auditLeftController();
        auditRightController();
        auditGenericTrackedDevices();
    }

    private void getRolesFromSteamVR()
    {
        Roles.Clear();

        if (!File.Exists(openvrpathsfilelocation)) return;

        var openVRPaths = JsonConvert.DeserializeObject<OpenVRPaths>(File.ReadAllText(openvrpathsfilelocation));
        if (openVRPaths is null) return;

        RuntimePath = openVRPaths.Runtime[0];

        var steamVRSettingsFilePath = Path.Join(openVRPaths.Config[0], "steamvr.vrsettings");
        if (!File.Exists(steamVRSettingsFilePath)) return;

        var steamVRSettings = JsonConvert.DeserializeObject<SteamVRSettings>(File.ReadAllText(steamVRSettingsFilePath));
        if (steamVRSettings is null) return;

        var trackers = steamVRSettings.Trackers;

        foreach (var (key, value) in trackers)
        {
            var serialNumber = key.Split('/').Last();
            var role = Enum.Parse<DeviceRole>(value.Split('_').Last());
            Roles.TryAdd(serialNumber, role);
        }
    }

    private void auditHMD()
    {
        var connectedHMDIndex = OVRHelper.GetIndexForTrackedDeviceClass(ETrackedDeviceClass.HMD);
        if (connectedHMDIndex == OpenVR.k_unTrackedDeviceIndexInvalid) return;

        var connectedHMDSerial = OVRHelper.GetStringTrackedDeviceProperty(connectedHMDIndex, ETrackedDeviceProperty.Prop_SerialNumber_String);
        addOrUpdateDeviceRole(connectedHMDSerial, DeviceRole.Head, connectedHMDIndex);
    }

    private void auditLeftController()
    {
        var connectedLeftControllersIndexes = OVRHelper.GetAllControllersFromHint("left").ToList();

        if (connectedLeftControllersIndexes.Count == 0)
        {
            var connectedLeftControllerIndex = OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
            if (connectedLeftControllerIndex == OpenVR.k_unTrackedDeviceIndexInvalid) return;

            var connectedLeftControllerSerial = OVRHelper.GetStringTrackedDeviceProperty(connectedLeftControllerIndex, ETrackedDeviceProperty.Prop_SerialNumber_String);
            addOrUpdateDeviceRole(connectedLeftControllerSerial, DeviceRole.LeftHand, connectedLeftControllerIndex);
        }
        else
        {
            foreach (var connectedLeftControllerIndex in connectedLeftControllersIndexes)
            {
                var connectedLeftControllerSerial = OVRHelper.GetStringTrackedDeviceProperty(connectedLeftControllerIndex, ETrackedDeviceProperty.Prop_SerialNumber_String);

                addOrUpdateDeviceRole(connectedLeftControllerSerial, DeviceRole.LeftHand, connectedLeftControllerIndex);
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

            addOrUpdateDeviceRole(connectedRightControllerSerial, DeviceRole.RightHand, connectedRightControllerIndex);
        }
        else
        {
            foreach (var connectedRightControllerIndex in connectedRightControllersIndexes)
            {
                var connectedRightControllerSerial = OVRHelper.GetStringTrackedDeviceProperty(connectedRightControllerIndex, ETrackedDeviceProperty.Prop_SerialNumber_String);

                addOrUpdateDeviceRole(connectedRightControllerSerial, DeviceRole.RightHand, connectedRightControllerIndex);
            }
        }
    }

    private void auditGenericTrackedDevices()
    {
        var connectedGenericDeviceIndexes = OVRHelper.GetIndexesForTrackedDeviceClass(ETrackedDeviceClass.GenericTracker);

        foreach (var connectedGenericDeviceIndex in connectedGenericDeviceIndexes)
        {
            var connectedGenericDeviceSerial = OVRHelper.GetStringTrackedDeviceProperty(connectedGenericDeviceIndex, ETrackedDeviceProperty.Prop_SerialNumber_String);

            if (Roles.Any(pair => pair.Key.EndsWith(connectedGenericDeviceSerial)))
            {
                var deviceRole = Roles.Single(pair => pair.Key.EndsWith(connectedGenericDeviceSerial)).Value;
                addOrUpdateDeviceRole(connectedGenericDeviceSerial, deviceRole, connectedGenericDeviceIndex);
            }
            else
            {
                addOrUpdateDeviceRole(connectedGenericDeviceSerial, DeviceRole.Unset, connectedGenericDeviceIndex);
            }
        }
    }
}