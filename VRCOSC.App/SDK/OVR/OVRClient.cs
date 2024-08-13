// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Valve.VR;
using VRCOSC.App.OVR;
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
    public bool RefreshManifest { get; set; }
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
        // TODO: Change this to contact OpenVR to find out the values for the manifest.
        // If the manifest exists but the values have been updated in the app, remove and add the manifest
        Debug.Assert(Metadata is not null);

        if (!RefreshManifest) return;

        OpenVR.Applications.RemoveApplicationManifest(Metadata.ApplicationManifest);
        OpenVR.Applications.AddApplicationManifest(Metadata.ApplicationManifest, false);

        RefreshManifest = false;
    }

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

    public void TriggerHaptic(DeviceRole device, float durationSeconds, float frequency, float amplitude) =>
        OVRHelper.TriggerHaptic(Input.GetHapticActionHandle(device), OVRDeviceManager.GetTrackedDevice(device).Index, durationSeconds, frequency, amplitude);

    /// <summary>
    ///     Checks to see if the user is wearing their headset
    /// </summary>
    public bool IsUserPresent()
    {
        if (!HasInitialised) return false;

        var hmd = OVRDeviceManager.GetTrackedDevice(DeviceRole.Head);
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
