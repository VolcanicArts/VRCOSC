// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.CompilerServices;
using Valve.VR;
using VRCOSC.OpenVR.Device;
using VRCOSC.OpenVR.Metadata;

namespace VRCOSC.OpenVR;

public class OVRClient
{
    private static readonly uint vrevent_t_size = (uint)Unsafe.SizeOf<VREvent_t>();

    public Action? OnShutdown;

    public bool HasInitialised { get; private set; }

    public HMD? HMD => system.HMD;
    public Controller? LeftController => system.LeftController;
    public Controller? RightController => system.RightController;
    public IEnumerable<GenericTracker> Trackers => system.Trackers;

    internal readonly OVRMetadata Metadata;

    private readonly OVRSystem system;
    private readonly OVRInput input;

    public OVRClient(OVRMetadata metadata)
    {
        Metadata = metadata;
        system = new OVRSystem();
        input = new OVRInput(this);
    }

    public void Init()
    {
        if (HasInitialised) return;

        HasInitialised = OVRHelper.InitialiseOpenVR(Metadata.ApplicationType);

        if (!HasInitialised) return;

        Valve.VR.OpenVR.Applications.AddApplicationManifest(Metadata.ApplicationManifest, false);
        system.Init();
        input.Init();
    }

    public void Update()
    {
        if (!HasInitialised) return;

        pollEvents();

        if (!HasInitialised) return;

        input.Update();
    }

    private void pollEvents()
    {
        var evenT = new VREvent_t();

        while (Valve.VR.OpenVR.System.PollNextEvent(ref evenT, vrevent_t_size))
        {
            var eventType = (EVREventType)evenT.eventType;

            switch (eventType)
            {
                case EVREventType.VREvent_Quit:
                    Valve.VR.OpenVR.System.AcknowledgeQuit_Exiting();
                    shutdown();
                    return;

                case EVREventType.VREvent_TrackedDeviceActivated: // registration or connection
                case EVREventType.VREvent_TrackedDeviceDeactivated: // disconnection but not a deregistration
                case EVREventType.VREvent_TrackedDeviceUpdated: // anything else about the device could've been updated
                    system.HandleDevice(evenT.trackedDeviceIndex);
                    break;
            }
        }
    }

    private void shutdown()
    {
        Valve.VR.OpenVR.Shutdown();
        HasInitialised = false;
        OnShutdown?.Invoke();
    }
}
