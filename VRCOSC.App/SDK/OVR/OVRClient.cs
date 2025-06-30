// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using Valve.VR;
using VRCOSC.App.OVR;
using VRCOSC.App.SDK.OVR.Device;
using VRCOSC.App.SDK.OVR.Metadata;
using VRCOSC.App.Settings;

namespace VRCOSC.App.SDK.OVR;

public class OVRClient
{
    private static readonly string vrpath_file = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "openvr", "openvrpaths.vrpath");
    private static readonly uint vrevent_t_size = (uint)Unsafe.SizeOf<VREvent_t>();
    public readonly OVRInput Input;

    internal OVRMetadata? Metadata;

    internal OVRClient()
    {
        Input = new OVRInput(this);
    }

    public bool HasInitialised { get; private set; }
    public float FPS { get; private set; }

    internal void SetMetadata(OVRMetadata metadata)
    {
        Metadata = metadata;
    }

    internal void Init()
    {
        if (HasInitialised || Metadata is null) return;
        if (!File.Exists(vrpath_file)) return;

        if (!OVRHelper.InitialiseOpenVR(Metadata.ApplicationType)) return;

        manageManifest();

        Input.Init();

        HasInitialised = true;
    }

    private void manageManifest()
    {
        Debug.Assert(Metadata is not null);

        // Remove V1 manifest
        OpenVR.Applications.RemoveApplicationManifest(Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VRCOSC", "openvr", "app.vrmanifest"));
        OpenVR.Applications.AddApplicationManifest(Metadata.ApplicationManifest, false);
    }

    public TrackedDevice? GetTrackedDevice(string serialNumber) => OVRDeviceManager.GetInstance().GetTrackedDevice(serialNumber);
    public TrackedDevice? GetTrackedDevice(DeviceRole deviceRole) => OVRDeviceManager.GetInstance().GetTrackedDevice(deviceRole);
    public HMD? GetHMD() => (HMD?)OVRDeviceManager.GetInstance().GetTrackedDevice(DeviceRole.Head);
    public Controller? GetLeftController() => (Controller?)OVRDeviceManager.GetInstance().GetTrackedDevice(DeviceRole.LeftHand);
    public Controller? GetRightController() => (Controller?)OVRDeviceManager.GetInstance().GetTrackedDevice(DeviceRole.RightHand);

    internal void Update()
    {
        if (!HasInitialised) return;

        pollEvents();

        if (!HasInitialised) return;

        FPS = 1000.0f / OVRHelper.GetFrameTimeMilli();

        Input.Update();

        foreach (TrackedDevice trackedDevice in OVRDeviceManager.GetInstance().TrackedDevices.Values)
        {
            trackedDevice.Update();
        }
    }

    private void pollEvents()
    {
        var evenT = new VREvent_t();

        while (OpenVR.System.PollNextEvent(ref evenT, vrevent_t_size))
        {
            var eventType = (EVREventType)evenT.eventType;

            switch (eventType)
            {
                case EVREventType.VREvent_Quit:
                    OpenVR.System.AcknowledgeQuit_Exiting();
                    shutdown();
                    return;
            }
        }
    }

    public void TriggerHaptic(DeviceRole deviceRole, float durationSeconds, float frequency, float amplitude)
    {
        if (!HasInitialised) return;

        var trackedDevice = OVRDeviceManager.GetInstance().GetTrackedDevice(deviceRole);
        if (trackedDevice is null || !trackedDevice.IsConnected || deviceRole == DeviceRole.Unset) return;

        OVRHelper.TriggerHaptic(Input.GetHapticActionHandle(deviceRole), trackedDevice.Index, durationSeconds, frequency, amplitude);
    }

    public void TriggerHaptic(TrackedDevice trackedDevice, float durationSeconds, float frequency, float amplitude)
    {
        if (!HasInitialised) return;

        if (trackedDevice is null || !trackedDevice.IsConnected || trackedDevice.Role == DeviceRole.Unset) return;

        OVRHelper.TriggerHaptic(Input.GetHapticActionHandle(trackedDevice.Role), trackedDevice.Index, durationSeconds, frequency, amplitude);
    }

    public void ShutdownDevice(TrackedDevice device)
    {
        if (!HasInitialised) return;

        var lighthouseConsole = OVRDeviceManager.GetInstance().LighthouseConsole;
        if (lighthouseConsole is null) return;

        var startInfo = new ProcessStartInfo(lighthouseConsole)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            Arguments = $"/serial {device.DongleId} poweroff"
        };

        Process.Start(startInfo);
    }

    /// <summary>
    ///     Checks to see if the user is wearing their headset
    /// </summary>
    public bool IsUserPresent()
    {
        if (!HasInitialised) return false;

        var hmd = GetHMD();
        if (hmd is null || !hmd.IsConnected) return false;

        return OpenVR.System.GetTrackedDeviceActivityLevel(hmd.Index) == EDeviceActivityLevel.k_EDeviceActivityLevel_UserInteraction;
    }

    /// <summary>
    ///     Checks to see if the dashboard is visible
    /// </summary>
    public bool IsDashboardVisible()
    {
        if (!HasInitialised) return false;

        return OpenVR.Overlay.IsDashboardVisible();
    }

    private async void shutdown()
    {
        await AppManager.GetInstance().StopAsync();

        OpenVR.Shutdown();
        HasInitialised = false;

        if (SettingsManager.GetInstance().GetValue<bool>(VRCOSCSetting.OVRAutoClose))
        {
            Application.Current.Shutdown();
        }
    }

    internal void SetAutoLaunch(bool value)
    {
        if (!HasInitialised) return;

        OpenVR.Applications.SetApplicationAutoLaunch("volcanicarts.vrcosc", value);
    }
}