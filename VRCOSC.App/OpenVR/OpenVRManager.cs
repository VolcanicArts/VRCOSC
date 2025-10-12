// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using Newtonsoft.Json;
using Valve.VR;
using VRCOSC.App.OpenVR.Device;
using VRCOSC.App.OpenVR.Metadata;
using VRCOSC.App.Settings;
using VRCOSC.App.Utils;

#if !DEBUG
using Velopack.Locators;
#endif

namespace VRCOSC.App.OpenVR;

public class OpenVRManager
{
    public static readonly string VRPATH_FILE = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "openvr", "openvrpaths.vrpath");
    private static readonly uint vrevent_t_size = (uint)Unsafe.SizeOf<VREvent_t>();

    private Storage storage => AppManager.GetInstance().Storage;

    private const int fast_ups = 30;

    private readonly OpenVRInput input;
    private readonly OVRMetadata metadata;
    private readonly Repeater attemptInitialisationTask;
    private readonly Repeater fastUpdate;

    private int updateCounter;

    public bool Initialised { get; private set; }

    public float FPS { get; private set; }
    public bool IsDashboardVisible { get; private set; }
    public bool IsUserPresent { get; private set; }

    private ConcurrentDictionary<uint, TrackedDevice> devices { get; } = [];

    public IReadOnlyList<TrackedDevice> Devices => devices.Values.ToImmutableList();

    internal OpenVRManager()
    {
        copyResources();

        input = new OpenVRInput(this);

        metadata = new OVRMetadata
        {
            ApplicationType = EVRApplicationType.VRApplication_Background,
            ApplicationManifest = storage.GetFullPath("runtime/openvr/app.vrmanifest"),
            ActionManifest = storage.GetFullPath("runtime/openvr/action_manifest.json")
        };

        attemptInitialisationTask = new Repeater($"{nameof(OpenVRManager)}-{nameof(attemptInitialisation)}", attemptInitialisation);
        attemptInitialisationTask.Start(TimeSpan.FromSeconds(2d), true);

        fastUpdate = new Repeater($"{nameof(OpenVRManager)}-{nameof(onFastUpdate)}", onFastUpdate);
        fastUpdate.Start(TimeSpan.FromSeconds(1d / fast_ups), true);
    }

    public TrackedDevice? GetTrackedDevice(string serialNumber) => devices.Values.SingleOrDefault(d => d.SerialNumber == serialNumber);
    public TrackedDevice? GetTrackedDevice(DeviceRole role) => role == DeviceRole.Unset ? null : devices.Values.SingleOrDefault(d => d.Role == role);
    public HMD? GetHMD() => (HMD?)GetTrackedDevice(DeviceRole.Head);
    public Controller? GetLeftController() => (Controller?)GetTrackedDevice(DeviceRole.LeftHand);
    public Controller? GetRightController() => (Controller?)GetTrackedDevice(DeviceRole.RightHand);

    public void TriggerHaptic(TrackedDevice trackedDevice, float durationSeconds, float frequency, float amplitude)
    {
        if (!Initialised) return;
        if (trackedDevice is null || !trackedDevice.IsConnected) return;

        OpenVRHelper.TriggerHaptic(input.GetHapticActionHandle(trackedDevice.Role), trackedDevice.Index, durationSeconds, frequency, amplitude);
    }

    private Task attemptInitialisation()
    {
        if (Initialised || !File.Exists(VRPATH_FILE)) return Task.CompletedTask;
        if (!OpenVRHelper.InitialiseOpenVR(metadata.ApplicationType)) return Task.CompletedTask;

        manageManifests();
        input.Init();
        Initialised = true;

        return Task.CompletedTask;
    }

    private void deinitialise()
    {
        Initialised = false;
        FPS = 0f;
        IsUserPresent = false;
        IsDashboardVisible = false;
        devices.Clear();
    }

    private async Task onFastUpdate()
    {
        if (!Initialised) return;

        await pollEvents();

        if (!Initialised) return;

        updateFPS();
        updateIsUserPresent();
        updateIsDashboardVisible();

        updateCounter++;

        if (updateCounter == fast_ups)
        {
            onSlowUpdate();
            updateCounter = 0;
        }

        updateTransforms();
        input.Update();
    }

    private void updateFPS()
    {
        if (!Initialised) return;

        FPS = 1000f / OpenVRHelper.GetFrameTimeMilli();
    }

    private void updateIsUserPresent()
    {
        if (!Initialised) return;

        var hmd = (HMD?)GetTrackedDevice(DeviceRole.Head);

        if (hmd is null || !hmd.IsConnected)
        {
            IsUserPresent = false;
            return;
        }

        IsUserPresent = Valve.VR.OpenVR.System.GetTrackedDeviceActivityLevel(hmd.Index) == EDeviceActivityLevel.k_EDeviceActivityLevel_UserInteraction;
    }

    private void updateIsDashboardVisible()
    {
        if (!Initialised) return;

        IsDashboardVisible = Valve.VR.OpenVR.Overlay.IsDashboardVisible();
    }

    private void updateTransforms()
    {
        if (!Initialised) return;

        var poses = OpenVRHelper.GetPoses();

        foreach (var device in devices.Values)
        {
            if (!device.IsConnected)
            {
                device.Transform = Transform.Identity;
                continue;
            }

            device.Transform = poses[device.Index];

            if (device.Role is DeviceRole.Head or DeviceRole.LeftHand or DeviceRole.RightHand) continue;

            device.Transform = device.Transform with { Rotation = Quaternion.Multiply(device.Transform.Rotation, Quaternion.CreateFromAxisAngle(Vector3.UnitX, -MathF.PI / 2)) };
        }
    }

    private void onSlowUpdate()
    {
        if (!Initialised) return;

        Valve.VR.OpenVR.Applications.SetApplicationAutoLaunch("volcanicarts.vrcosc", SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.OVRAutoOpen));

        foreach (var role in Enum.GetValues<DeviceRole>())
        {
            if (role == DeviceRole.Unset) continue;

            var index = OpenVRHelper.GetDeviceIndexForRole(role);
            if (index == Valve.VR.OpenVR.k_unTrackedDeviceIndexInvalid) continue;

            var serial = OpenVRHelper.GetStringTrackedDeviceProperty(index, ETrackedDeviceProperty.Prop_SerialNumber_String);
            var dongle = OpenVRHelper.GetStringTrackedDeviceProperty(index, ETrackedDeviceProperty.Prop_ConnectedWirelessDongle_String);

            if (devices.TryGetValue(index, out var existingDevice))
            {
                if (existingDevice.SerialNumber == serial && existingDevice.Role == role && isDeviceTypeCorrect(existingDevice, role))
                    continue;

                devices.Remove(index, out _);
            }

            var roleDevice = GetTrackedDevice(role);

            if (roleDevice is not null && devices.TryGetValue(roleDevice.Index, out var oldDevice))
            {
                if (oldDevice.SerialNumber != serial)
                    devices.Remove(roleDevice.Index, out _);
            }

            var device = createDevice(index, serial, dongle, role);
            devices.TryAdd(index, device);
        }

        foreach (var index in OpenVRHelper.GetAllDeviceIndexes())
        {
            if (devices.ContainsKey(index)) continue;

            var serial = OpenVRHelper.GetStringTrackedDeviceProperty(index, ETrackedDeviceProperty.Prop_SerialNumber_String);
            var dongle = OpenVRHelper.GetStringTrackedDeviceProperty(index, ETrackedDeviceProperty.Prop_ConnectedWirelessDongle_String);

            var device = createDevice(index, serial, dongle, DeviceRole.Unset);
            devices.TryAdd(index, device);
        }

        foreach (var device in devices.Values)
        {
            updateDeviceInfo(device);
        }
    }

    private bool isDeviceTypeCorrect(TrackedDevice device, DeviceRole role) => role switch
    {
        DeviceRole.Head => device is HMD,
        DeviceRole.LeftHand or DeviceRole.RightHand => device is Controller,
        _ => device.GetType() == typeof(TrackedDevice),
    };

    private TrackedDevice createDevice(uint index, string serial, string dongle, DeviceRole role)
    {
        switch (role)
        {
            case DeviceRole.Head:
                return new HMD(index, serial, dongle, role);

            case DeviceRole.LeftHand:
            case DeviceRole.RightHand:
                return new Controller(index, serial, dongle, role);

            case DeviceRole.Chest:
            case DeviceRole.Waist:
            case DeviceRole.LeftElbow:
            case DeviceRole.RightElbow:
            case DeviceRole.LeftFoot:
            case DeviceRole.RightFoot:
            case DeviceRole.LeftKnee:
            case DeviceRole.RightKnee:
            case DeviceRole.Unset:
                return new TrackedDevice(index, serial, dongle, role);

            default:
                throw new ArgumentOutOfRangeException(nameof(role), role, null);
        }
    }

    private void updateDeviceInfo(TrackedDevice device)
    {
        device.IsConnected = Valve.VR.OpenVR.System.IsTrackedDeviceConnected(device.Index);
        device.ProvidesBatteryStatus = device.IsConnected && OpenVRHelper.GetBoolTrackedDeviceProperty(device.Index, ETrackedDeviceProperty.Prop_DeviceProvidesBatteryStatus_Bool);
        device.IsCharging = device.IsConnected && OpenVRHelper.GetBoolTrackedDeviceProperty(device.Index, ETrackedDeviceProperty.Prop_DeviceIsCharging_Bool);
        device.BatteryPercentage = device.IsConnected ? MathF.Max(0f, OpenVRHelper.GetFloatTrackedDeviceProperty(device.Index, ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float)) : 0f;
    }

    private async Task pollEvents()
    {
        var @event = new VREvent_t();

        while (Initialised && Valve.VR.OpenVR.System.PollNextEvent(ref @event, vrevent_t_size))
        {
            var eventType = (EVREventType)@event.eventType;

            switch (eventType)
            {
                case EVREventType.VREvent_Quit:
                    Valve.VR.OpenVR.System.AcknowledgeQuit_Exiting();
                    await shutdown();
                    return;
            }
        }
    }

    private async Task shutdown()
    {
        deinitialise();
        Valve.VR.OpenVR.Shutdown();
        await AppManager.GetInstance().StopAsync();

        if (SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.OVRAutoClose))
            Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
    }

    private void copyResources()
    {
        var runtimeOVRStorage = storage.GetStorageForDirectory("runtime/openvr");
        var runtimeOVRPath = runtimeOVRStorage.GetFullPath(string.Empty);

        var ovrFiles = Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(file => file.Contains("OpenVR"));

        foreach (var file in ovrFiles)
        {
            File.WriteAllBytes(Path.Combine(runtimeOVRPath, ResourceHelper.GetOriginalFileName(file)), ResourceHelper.GetResourceBytes(file));
        }

        var manifest = new OVRManifest();
#if DEBUG
        manifest.Applications[0].BinaryPathWindows = Environment.ProcessPath!;
#else
        manifest.Applications[0].BinaryPathWindows = Path.Join(VelopackLocator.CreateDefaultForPlatform().RootAppDir, "current", "VRCOSC.exe");
#endif
        manifest.Applications[0].ActionManifestPath = runtimeOVRStorage.GetFullPath("action_manifest.json");
        manifest.Applications[0].ImagePath = runtimeOVRStorage.GetFullPath("SteamImage.png");

        File.WriteAllText(Path.Join(runtimeOVRPath, "app.vrmanifest"), JsonConvert.SerializeObject(manifest, Formatting.Indented));
    }

    private void manageManifests()
    {
        // Remove V1 manifest
        Valve.VR.OpenVR.Applications.RemoveApplicationManifest(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VRCOSC", "openvr", "app.vrmanifest"));
        Valve.VR.OpenVR.Applications.AddApplicationManifest(metadata.ApplicationManifest, false);
        Valve.VR.OpenVR.Input.SetActionManifestPath(metadata.ActionManifest);
    }
}