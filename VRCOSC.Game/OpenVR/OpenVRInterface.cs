// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using osu.Framework.Logging;
using osu.Framework.Platform;
using Valve.VR;

namespace VRCOSC.Game.OpenVR;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class OpenVRInterface
{
    private static readonly uint vrevent_t_size = (uint)Unsafe.SizeOf<VREvent_t>();
    private static readonly uint vractiveactonset_t_size = (uint)Unsafe.SizeOf<VRActiveActionSet_t>();
    private static readonly uint inputanalogactiondata_t_size = (uint)Unsafe.SizeOf<InputAnalogActionData_t>();
    private static readonly uint inputdigitalactiondata_t_size = (uint)Unsafe.SizeOf<InputDigitalActionData_t>();

    private ulong actionSetHandle;
    private readonly ulong[] leftControllerActions = new ulong[8];
    private readonly ulong[] rightControllerActions = new ulong[8];

    private readonly Storage storage;
    public Action? OnOpenVRShutdown;

    public bool HasInitialised { get; private set; }
    public OpenVRHMD? HMD { get; private set; }
    public OpenVRController? LeftController { get; private set; }
    public OpenVRController? RightController { get; private set; }
    public OpenVRTrackers? Trackers { get; private set; }

    public OpenVRInterface(Storage storage)
    {
        this.storage = storage.GetStorageForDirectory(@"openvr");
    }

    #region Boilerplate

    public void Init()
    {
        if (HasInitialised) return;

        HasInitialised = initialiseOpenVR();

        if (!HasInitialised) return;

        Valve.VR.OpenVR.Applications.AddApplicationManifest(storage.GetFullPath(@"app.vrmanifest"), false);
        Valve.VR.OpenVR.Input.SetActionManifestPath(storage.GetFullPath(@"action_manifest.json"));
        getActionHandles();

        HMD = new OpenVRHMD(this);
        LeftController = new OpenVRController(this);
        RightController = new OpenVRController(this);
        Trackers = new OpenVRTrackers(this);
    }

    private bool initialiseOpenVR()
    {
        var err = new EVRInitError();
        var state = Valve.VR.OpenVR.InitInternal(ref err, EVRApplicationType.VRApplication_Background);
        return err == EVRInitError.None && state != 0;
    }

    private void getActionHandles()
    {
        Valve.VR.OpenVR.Input.GetActionHandle(@"/actions/main/in/lefta", ref leftControllerActions[0]);
        Valve.VR.OpenVR.Input.GetActionHandle(@"/actions/main/in/leftb", ref leftControllerActions[1]);
        Valve.VR.OpenVR.Input.GetActionHandle(@"/actions/main/in/leftpad", ref leftControllerActions[2]);
        Valve.VR.OpenVR.Input.GetActionHandle(@"/actions/main/in/leftstick", ref leftControllerActions[3]);
        Valve.VR.OpenVR.Input.GetActionHandle(@"/actions/main/in/leftfingerindex", ref leftControllerActions[4]);
        Valve.VR.OpenVR.Input.GetActionHandle(@"/actions/main/in/leftfingermiddle", ref leftControllerActions[5]);
        Valve.VR.OpenVR.Input.GetActionHandle(@"/actions/main/in/leftfingerring", ref leftControllerActions[6]);
        Valve.VR.OpenVR.Input.GetActionHandle(@"/actions/main/in/leftfingerpinky", ref leftControllerActions[7]);

        Valve.VR.OpenVR.Input.GetActionHandle(@"/actions/main/in/righta", ref rightControllerActions[0]);
        Valve.VR.OpenVR.Input.GetActionHandle(@"/actions/main/in/rightb", ref rightControllerActions[1]);
        Valve.VR.OpenVR.Input.GetActionHandle(@"/actions/main/in/rightpad", ref rightControllerActions[2]);
        Valve.VR.OpenVR.Input.GetActionHandle(@"/actions/main/in/rightstick", ref rightControllerActions[3]);
        Valve.VR.OpenVR.Input.GetActionHandle(@"/actions/main/in/rightfingerindex", ref rightControllerActions[4]);
        Valve.VR.OpenVR.Input.GetActionHandle(@"/actions/main/in/rightfingermiddle", ref rightControllerActions[5]);
        Valve.VR.OpenVR.Input.GetActionHandle(@"/actions/main/in/rightfingerring", ref rightControllerActions[6]);
        Valve.VR.OpenVR.Input.GetActionHandle(@"/actions/main/in/rightfingerpinky", ref rightControllerActions[7]);

        Valve.VR.OpenVR.Input.GetActionSetHandle(@"/actions/main", ref actionSetHandle);
    }

    #endregion

    #region Events

    public void Update()
    {
        if (!HasInitialised) return;

        pollEvents();

        if (!HasInitialised) return;

        updateActionSet();
        updateDevices();
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
            }
        }
    }

    private void updateActionSet()
    {
        var activeActionSet = new VRActiveActionSet_t[] { new() };
        activeActionSet[0].ulActionSet = actionSetHandle;
        activeActionSet[0].ulRestrictedToDevice = Valve.VR.OpenVR.k_ulInvalidInputValueHandle;
        activeActionSet[0].nPriority = 0;
        Valve.VR.OpenVR.Input.UpdateActionState(activeActionSet, vractiveactonset_t_size);
    }

    private void updateDevices()
    {
        HMD!.Update(getHmdIndex());
        LeftController!.Update(getLeftControllerIndex());
        RightController!.Update(getRightControllerIndex());
        Trackers!.Update(getTrackerIndexes());

        LeftController.Data.ATouched = getDigitalInput(leftControllerActions[0]).bState;
        LeftController.Data.BTouched = getDigitalInput(leftControllerActions[1]).bState;
        LeftController.Data.PadTouched = getDigitalInput(leftControllerActions[2]).bState;
        LeftController.Data.StickTouched = getDigitalInput(leftControllerActions[3]).bState;
        LeftController.Data.IndexFinger = getAnalogueInput(leftControllerActions[4]).x;
        LeftController.Data.MiddleFinger = getAnalogueInput(leftControllerActions[5]).x;
        LeftController.Data.RingFinger = getAnalogueInput(leftControllerActions[6]).x;
        LeftController.Data.PinkyFinger = getAnalogueInput(leftControllerActions[7]).x;

        RightController.Data.ATouched = getDigitalInput(rightControllerActions[0]).bState;
        RightController.Data.BTouched = getDigitalInput(rightControllerActions[1]).bState;
        RightController.Data.PadTouched = getDigitalInput(rightControllerActions[2]).bState;
        RightController.Data.StickTouched = getDigitalInput(rightControllerActions[3]).bState;
        RightController.Data.IndexFinger = getAnalogueInput(rightControllerActions[4]).x;
        RightController.Data.MiddleFinger = getAnalogueInput(rightControllerActions[5]).x;
        RightController.Data.RingFinger = getAnalogueInput(rightControllerActions[6]).x;
        RightController.Data.PinkyFinger = getAnalogueInput(rightControllerActions[7]).x;
    }

    private void shutdown()
    {
        Valve.VR.OpenVR.Shutdown();
        HasInitialised = false;
        HMD = null;
        LeftController = null;
        RightController = null;
        Trackers = null;
        OnOpenVRShutdown?.Invoke();
    }

    #endregion

    #region Abstraction

    private uint getHmdIndex() => getIndexForTrackedDeviceClass(ETrackedDeviceClass.HMD);
    private uint getLeftControllerIndex() => getController(@"left");
    private uint getRightControllerIndex() => getController(@"right");
    private IEnumerable<uint> getTrackerIndexes() => getIndexesForTrackedDeviceClass(ETrackedDeviceClass.GenericTracker);

    private InputAnalogActionData_t getAnalogueInput(ulong identifier)
    {
        var data = new InputAnalogActionData_t();
        Valve.VR.OpenVR.Input.GetAnalogActionData(identifier, ref data, inputanalogactiondata_t_size, Valve.VR.OpenVR.k_ulInvalidInputValueHandle);
        return data;
    }

    private InputDigitalActionData_t getDigitalInput(ulong identifier)
    {
        var data = new InputDigitalActionData_t();
        Valve.VR.OpenVR.Input.GetDigitalActionData(identifier, ref data, inputdigitalactiondata_t_size, Valve.VR.OpenVR.k_ulInvalidInputValueHandle);
        return data;
    }

    // GetTrackedDeviceIndexForControllerRole doesn't work when a tracker thinks it's a controller and assumes that role
    // We can forcibly find the correct indexes by using the model name
    private uint getController(string controllerHint)
    {
        var indexes = getIndexesForTrackedDeviceClass(ETrackedDeviceClass.Controller);

        foreach (var index in indexes)
        {
            var renderModelName = GetStringTrackedDeviceProperty(index, ETrackedDeviceProperty.Prop_RenderModelName_String);
            if (renderModelName.Contains(controllerHint, StringComparison.InvariantCultureIgnoreCase)) return index;
        }

        return uint.MaxValue;
    }

    private uint getIndexForTrackedDeviceClass(ETrackedDeviceClass klass)
    {
        var indexes = getIndexesForTrackedDeviceClass(klass).ToArray();
        return indexes.Any() ? indexes[0] : uint.MaxValue;
    }

    private IEnumerable<uint> getIndexesForTrackedDeviceClass(ETrackedDeviceClass klass)
    {
        for (uint i = 0; i < Valve.VR.OpenVR.k_unMaxTrackedDeviceCount; i++)
        {
            if (Valve.VR.OpenVR.System.GetTrackedDeviceClass(i) == klass) yield return i;
        }
    }

    internal bool GetBoolTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        var error = new ETrackedPropertyError();
        var value = Valve.VR.OpenVR.System.GetBoolTrackedDeviceProperty(index, property, ref error);

        if (error == ETrackedPropertyError.TrackedProp_Success) return value;

        logError(nameof(GetBoolTrackedDeviceProperty), index, error);
        return false;
    }

    internal int GetInt32TrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        var error = new ETrackedPropertyError();
        var value = Valve.VR.OpenVR.System.GetInt32TrackedDeviceProperty(index, property, ref error);

        if (error == ETrackedPropertyError.TrackedProp_Success) return value;

        logError(nameof(GetInt32TrackedDeviceProperty), index, error);
        return 0;
    }

    internal float GetFloatTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        var error = new ETrackedPropertyError();
        var value = Valve.VR.OpenVR.System.GetFloatTrackedDeviceProperty(index, property, ref error);

        if (error == ETrackedPropertyError.TrackedProp_Success) return value;

        logError(nameof(GetFloatTrackedDeviceProperty), index, error);
        return 0f;
    }

    private readonly StringBuilder sb = new((int)Valve.VR.OpenVR.k_unMaxPropertyStringSize);
    private readonly object stringLock = new();

    internal string GetStringTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        string str;

        lock (stringLock)
        {
            var error = new ETrackedPropertyError();
            sb.Clear();
            Valve.VR.OpenVR.System.GetStringTrackedDeviceProperty(index, property, sb, Valve.VR.OpenVR.k_unMaxPropertyStringSize, ref error);

            if (error != ETrackedPropertyError.TrackedProp_Success)
            {
                logError(nameof(GetStringTrackedDeviceProperty), index, error);
                return string.Empty;
            }

            str = sb.ToString();
        }

        return str;
    }

    #endregion

    private void logError(string methodName, uint index, ETrackedPropertyError error)
    {
        log($"[Error] {methodName}: {GetStringTrackedDeviceProperty(index, ETrackedDeviceProperty.Prop_TrackingSystemName_String)}: {error}");
    }

    private void log(string message)
    {
        Logger.Log($"[OpenVR] {message}");
    }
}

public class ControllerData
{
    public bool ATouched;
    public bool BTouched;
    public bool PadTouched;
    public bool StickTouched;
    public bool ThumbDown => ATouched || BTouched || PadTouched || StickTouched;
    public float IndexFinger;
    public float MiddleFinger;
    public float RingFinger;
    public float PinkyFinger;
}
