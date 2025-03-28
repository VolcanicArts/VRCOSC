﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Valve.VR;
using VRCOSC.App.OVR;
using VRCOSC.App.SDK.OVR.Device;
using VRCOSC.App.SDK.OVR.Metadata;

namespace VRCOSC.App.SDK.OVR;

public class OVRClient
{
    private static readonly string vrpath_file = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "openvr", "openvrpaths.vrpath");
    private static readonly uint vrevent_t_size = (uint)Unsafe.SizeOf<VREvent_t>();
    public readonly OVRInput Input;

    internal OVRMetadata? Metadata;

    internal Action? OnShutdown;

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

    public TrackedDevice GetTrackedDevice(DeviceRole deviceRole) => OVRDeviceManager.GetInstance().GetTrackedDevice(deviceRole) ?? new TrackedDevice();
    public HMD GetHMD() => (HMD)(OVRDeviceManager.GetInstance().GetTrackedDevice(DeviceRole.Head) ?? new HMD());
    public Controller GetLeftController() => (Controller)(OVRDeviceManager.GetInstance().GetTrackedDevice(DeviceRole.LeftHand) ?? new Controller());
    public Controller GetRightController() => (Controller)(OVRDeviceManager.GetInstance().GetTrackedDevice(DeviceRole.RightHand) ?? new Controller());

    internal void Update()
    {
        if (!HasInitialised) return;

        pollEvents();

        if (!HasInitialised) return;

        FPS = 1000.0f / OVRHelper.GetFrameTimeMilli();

        Input.Update();
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
        if (!HasInitialised || deviceRole == DeviceRole.Unset) return;

        var trackedDevice = OVRDeviceManager.GetInstance().GetTrackedDevice(deviceRole);
        if (trackedDevice is null || !trackedDevice.IsConnected) return;

        OVRHelper.TriggerHaptic(Input.GetHapticActionHandle(deviceRole), trackedDevice.Index, durationSeconds, frequency, amplitude);
    }

    /// <summary>
    ///     Checks to see if the user is wearing their headset
    /// </summary>
    public bool IsUserPresent()
    {
        if (!HasInitialised) return false;

        var hmd = GetHMD();
        if (!hmd.IsConnected) return false;

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

    private void shutdown()
    {
        OpenVR.Shutdown();
        HasInitialised = false;
        OnShutdown?.Invoke();
    }

    internal void SetAutoLaunch(bool value)
    {
        if (!HasInitialised) return;

        OpenVR.Applications.SetApplicationAutoLaunch("volcanicarts.vrcosc", value);
    }
}
