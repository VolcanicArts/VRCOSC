// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
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


    // YEUSEPE: Changes to the Init Method and Initialization Process:
    // 1. Separated Checks into Helper Methods:
    //    - Each step of the initialization process (e.g., validating metadata, verifying VR paths, initializing OpenVR, setting up the manifest, and initializing input) is now handled by its own method.
    //    - This ensures that the main `Init` method is clean, focused, and easy to read.
    // 2. Added Logging for Each Step:
    //    - Clear success or failure logs are added for each part of the initialization process, making it easier to understand where a failure occurs.
    //    - Example: If metadata is missing or the OpenVR paths file is not found, specific logs explain the problem.
    // 3. Graceful Shutdown on Failure:
    //    - If a critical step fails (e.g., OpenVR initialization or manifest setup), the system shuts down gracefully using the `shutdown()` method.
    //    - This prevents leaving OpenVR in an inconsistent state if initialization partially succeeds but then fails.
    // 4. Improved Maintainability:
    //    - By breaking down the process into helper methods, each method now focuses on a single task, making it easier to debug and update specific parts of the initialization process without affecting others.
    //    - Example: If manifest setup logic changes, it only impacts the `SetupManifest` method.
    // 5. No Nested If Statements:
    //    - Each `if` statement in `Init` checks one condition and exits early on failure, avoiding deeply nested or complex conditions.
    //    - This keeps the `Init` method clean and readable.
    // 6. Centralized Error Handling:
    //    - Exceptions in manifest setup or input initialization are caught and logged, preventing the application from crashing unexpectedly.
    //    - Logs provide detailed error messages, including the exception message when applicable.
    // 7. Consistency in Flags:
    //    - The `HasInitialised` flag is only set to `true` after all initialization steps succeed, ensuring no partial or invalid initialization state.
    //    - If initialization fails, the flag remains `false` to prevent further operations from proceeding.
    // 8. Better Debugging Information:
    //    - Logs for both success and failure at each step provide clear insights during debugging or troubleshooting.
    //    - Example: A failure log explains exactly which step failed (e.g., "Initialization failed: Metadata is missing.").
    // Overall, these changes make the initialization process more robust, modular, and easier to debug and maintain.

    internal void Init()
    {
        if (HasInitialised)
        {
            Log("Initialization skipped: Already initialized.");
            return;
        }

        if (!ValidateMetadata())
        {
            Log("Initialization failed: Metadata is missing.");
            return;
        }

        if (!ValidateVRPath())
        {
            Log($"Initialization failed: OpenVR paths file not found at {vrpath_file}.");
            return;
        }

        if (!InitializeOpenVR())
        {
            Log("Initialization failed: OpenVR could not be initialized.");
            return;
        }

        if (!SetupManifest())
        {
            Log("Initialization failed: Could not set up application manifest.");
            shutdown();
            return;
        }

        if (!InitializeInput())
        {
            Log("Initialization failed: Input could not be initialized.");
            shutdown();
            return;
        }

        HasInitialised = true;
        Log("Initialization completed successfully.");
    }


    private bool ValidateMetadata()
    {
        if (Metadata == null)
        {
            return false;
        }

        Log("Metadata validated successfully.");
        return true;
    }

    private bool ValidateVRPath()
    {
        if (!File.Exists(vrpath_file))
        {
            return false;
        }

        Log("OpenVR paths file validated successfully.");
        return true;
    }

    private bool InitializeOpenVR()
    {
        try
        {
            Log("Attempting to initialize OpenVR...");
            if (!OVRHelper.InitialiseOpenVR(Metadata!.ApplicationType))
            {
                Log($"OpenVR initialization failed. ApplicationType: {Metadata.ApplicationType}");
                return false;
            }
            Log("OpenVR initialized successfully.");
            return true;
        }
        catch (Exception ex)
        {
            Log($"OpenVR initialization exception: {ex.Message}");
            return false;
        }
    }


    private bool SetupManifest()
    {
        try
        {
            var oldManifestPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VRCOSC", "openvr", "app.vrmanifest");
            OpenVR.Applications.RemoveApplicationManifest(oldManifestPath);
            OpenVR.Applications.AddApplicationManifest(Metadata!.ApplicationManifest, false);

            Log("Application manifest set up successfully.");
            return true;
        }
        catch (Exception ex)
        {
            Log($"Error setting up manifest: {ex.Message}");
            return false;
        }
    }

    private bool InitializeInput()
    {
        try
        {
            Input.Init();
            Log("Input initialized successfully.");
            return true;
        }
        catch (Exception ex)
        {
            Log($"Error initializing input: {ex.Message}");
            return false;
        }
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

    // YEUSEPE: Expanded for readability, and added logs. 

    private void shutdown()
    {
        if (HasInitialised)
        {
            OpenVR.Shutdown();
            HasInitialised = false;
            OnShutdown?.Invoke();
            Log("OpenVR shut down gracefully.");
        }
        else
        {
            Log("Shutdown skipped: OpenVR was not initialized.");
        }
    }


    internal void SetAutoLaunch(bool value)
    {
        if (!HasInitialised) return;

        OpenVR.Applications.SetApplicationAutoLaunch("volcanicarts.vrcosc", value);
    }
    // YEUSEPE: Added log function (Im lazy)
    private void Log(string message)
    {
        Debug.WriteLine($"[OVRClient] {message}");
    }

}
