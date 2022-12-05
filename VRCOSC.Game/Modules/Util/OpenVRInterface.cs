// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using osu.Framework.Logging;
using osu.Framework.Platform;
using Valve.VR;

// ReSharper disable MemberCanBeMadeStatic.Global

namespace VRCOSC.Game.Modules.Util;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class OpenVRInterface
{
    private static readonly uint vrevent_t_size = (uint)Unsafe.SizeOf<VREvent_t>();
    private static readonly uint vractiveactonset_t_size = (uint)Unsafe.SizeOf<VRActiveActionSet_t>();
    private static readonly uint inputanalogactiondata_t_size = (uint)Unsafe.SizeOf<InputAnalogActionData_t>();
    private static readonly uint inputdigitalactiondata_t_size = (uint)Unsafe.SizeOf<InputDigitalActionData_t>();

    public readonly ControllerData LeftController = new();
    public readonly ControllerData RightController = new();

    private ulong actionSetHandle;
    private readonly ulong[] leftController = new ulong[8];
    private readonly ulong[] rightController = new ulong[8];

    private readonly Storage storage;
    public bool HasInitialised { get; private set; }

    public OpenVRInterface(Storage storage)
    {
        this.storage = storage.GetStorageForDirectory("openvr");
    }

    public void Init()
    {
        if (HasInitialised) return;

        var err = new EVRInitError();
        var state = OpenVR.InitInternal(ref err, EVRApplicationType.VRApplication_Background);

        if (err != EVRInitError.None || state <= 0)
        {
            HasInitialised = false;
            return;
        }

        HasInitialised = true;

        OpenVR.Input.SetActionManifestPath(storage.GetFullPath("action_manifest.json"));

        OpenVR.Input.GetActionHandle("/actions/main/in/lefta", ref leftController[0]);
        OpenVR.Input.GetActionHandle("/actions/main/in/leftb", ref leftController[1]);
        OpenVR.Input.GetActionHandle("/actions/main/in/leftpad", ref leftController[2]);
        OpenVR.Input.GetActionHandle("/actions/main/in/leftstick", ref leftController[3]);
        OpenVR.Input.GetActionHandle("/actions/main/in/leftfingerindex", ref leftController[4]);
        OpenVR.Input.GetActionHandle("/actions/main/in/leftfingermiddle", ref leftController[5]);
        OpenVR.Input.GetActionHandle("/actions/main/in/leftfingerring", ref leftController[6]);
        OpenVR.Input.GetActionHandle("/actions/main/in/leftfingerpinky", ref leftController[7]);

        OpenVR.Input.GetActionHandle("/actions/main/in/righta", ref rightController[0]);
        OpenVR.Input.GetActionHandle("/actions/main/in/rightb", ref rightController[1]);
        OpenVR.Input.GetActionHandle("/actions/main/in/rightpad", ref rightController[2]);
        OpenVR.Input.GetActionHandle("/actions/main/in/rightstick", ref rightController[3]);
        OpenVR.Input.GetActionHandle("/actions/main/in/rightfingerindex", ref rightController[4]);
        OpenVR.Input.GetActionHandle("/actions/main/in/rightfingermiddle", ref rightController[5]);
        OpenVR.Input.GetActionHandle("/actions/main/in/rightfingerring", ref rightController[6]);
        OpenVR.Input.GetActionHandle("/actions/main/in/rightfingerpinky", ref rightController[7]);

        OpenVR.Input.GetActionSetHandle("/actions/main", ref actionSetHandle);
    }

    public bool IsHmdConnected() => getIndexForTrackedDeviceClass(ETrackedDeviceClass.HMD) != uint.MaxValue && OpenVR.IsHmdPresent();
    public bool IsLeftControllerConnected() => getLeftControllerIndex() != uint.MaxValue && OpenVR.System.IsTrackedDeviceConnected(getLeftControllerIndex());
    public bool IsRightControllerConnected() => getRightControllerIndex() != uint.MaxValue && OpenVR.System.IsTrackedDeviceConnected(getRightControllerIndex());
    public bool IsTrackerConnected(uint trackerIndex) => trackerIndex != uint.MaxValue && OpenVR.System.IsTrackedDeviceConnected(trackerIndex);

    public bool IsHmdCharging() => CanHmdProvideBatteryData() && getBoolTrackedDeviceProperty(getHmdIndex(), ETrackedDeviceProperty.Prop_DeviceIsCharging_Bool);
    public bool IsLeftControllerCharging() => getBoolTrackedDeviceProperty(getLeftControllerIndex(), ETrackedDeviceProperty.Prop_DeviceIsCharging_Bool);
    public bool IsRightControllerCharging() => getBoolTrackedDeviceProperty(getRightControllerIndex(), ETrackedDeviceProperty.Prop_DeviceIsCharging_Bool);
    public bool IsTrackerCharging(uint trackerIndex) => getBoolTrackedDeviceProperty(trackerIndex, ETrackedDeviceProperty.Prop_DeviceIsCharging_Bool);

    public float GetHmdBatteryPercentage() => getFloatTrackedDeviceProperty(getHmdIndex(), ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float);
    public float GetLeftControllerBatteryPercentage() => getFloatTrackedDeviceProperty(getLeftControllerIndex(), ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float);
    public float GetRightControllerBatteryPercentage() => getFloatTrackedDeviceProperty(getRightControllerIndex(), ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float);
    public float GetTrackerBatteryPercentage(uint trackerIndex) => getFloatTrackedDeviceProperty(trackerIndex, ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float);

    public bool CanHmdProvideBatteryData()
    {
        var error = new ETrackedPropertyError();
        var canProvideBattery = OpenVR.System.GetBoolTrackedDeviceProperty(getHmdIndex(), ETrackedDeviceProperty.Prop_DeviceProvidesBatteryStatus_Bool, ref error);
        return error == ETrackedPropertyError.TrackedProp_Success && canProvideBattery;
    }

    #region Events

    public void Poll()
    {
        var evenT = new VREvent_t();

        while (OpenVR.System.PollNextEvent(ref evenT, vrevent_t_size))
        {
            var eventType = (EVREventType)evenT.eventType;

            if (eventType == EVREventType.VREvent_Quit)
            {
                OpenVR.System.AcknowledgeQuit_Exiting();
                HasInitialised = false;
                return;
            }
        }

        var activeActionSet = new VRActiveActionSet_t[] { new() };
        activeActionSet[0].ulActionSet = actionSetHandle;
        activeActionSet[0].ulRestrictedToDevice = OpenVR.k_ulInvalidInputValueHandle;
        activeActionSet[0].nPriority = 0;
        OpenVR.Input.UpdateActionState(activeActionSet, vractiveactonset_t_size);
        extractControllerData();
    }

    private void extractControllerData()
    {
        LeftController.ATouched = getDigitalInput(leftController[0]).bState;
        LeftController.BTouched = getDigitalInput(leftController[1]).bState;
        LeftController.PadTouched = getDigitalInput(leftController[2]).bState;
        LeftController.StickTouched = getDigitalInput(leftController[3]).bState;
        LeftController.IndexFinger = getAnalogueInput(leftController[4]).x;
        LeftController.MiddleFinger = getAnalogueInput(leftController[5]).x;
        LeftController.RingFinger = getAnalogueInput(leftController[6]).x;
        LeftController.PinkyFinger = getAnalogueInput(leftController[7]).x;

        RightController.ATouched = getDigitalInput(rightController[0]).bState;
        RightController.BTouched = getDigitalInput(rightController[1]).bState;
        RightController.PadTouched = getDigitalInput(rightController[2]).bState;
        RightController.StickTouched = getDigitalInput(rightController[3]).bState;
        RightController.IndexFinger = getAnalogueInput(rightController[4]).x;
        RightController.MiddleFinger = getAnalogueInput(rightController[5]).x;
        RightController.RingFinger = getAnalogueInput(rightController[6]).x;
        RightController.PinkyFinger = getAnalogueInput(rightController[7]).x;
    }

    private InputAnalogActionData_t getAnalogueInput(ulong identifier)
    {
        var data = new InputAnalogActionData_t();
        OpenVR.Input.GetAnalogActionData(identifier, ref data, inputanalogactiondata_t_size, OpenVR.k_ulInvalidInputValueHandle);
        return data;
    }

    private InputDigitalActionData_t getDigitalInput(ulong identifier)
    {
        var data = new InputDigitalActionData_t();
        OpenVR.Input.GetDigitalActionData(identifier, ref data, inputdigitalactiondata_t_size, OpenVR.k_ulInvalidInputValueHandle);
        return data;
    }

    #endregion

    #region OpenVR Abstraction

    private uint getHmdIndex() => getIndexForTrackedDeviceClass(ETrackedDeviceClass.HMD);
    private uint getLeftControllerIndex() => getController("left");
    private uint getRightControllerIndex() => getController("right");
    public uint[] GetTrackers() => getIndexesForTrackedDeviceClass(ETrackedDeviceClass.GenericTracker);

    // GetTrackedDeviceIndexForControllerRole doesn't work when a tracker thinks it's a controller and assumes that role
    // We can forcibly find the correct indexes by using the model name
    private uint getController(string controllerHint)
    {
        var indexes = getIndexesForTrackedDeviceClass(ETrackedDeviceClass.Controller);

        foreach (var index in indexes)
        {
            var renderModelName = getStringTrackedDeviceProperty(index, ETrackedDeviceProperty.Prop_RenderModelName_String);
            if (renderModelName.Contains(controllerHint, StringComparison.InvariantCultureIgnoreCase)) return index;
        }

        return uint.MaxValue;
    }

    private uint getIndexForTrackedDeviceClass(ETrackedDeviceClass klass)
    {
        var indexes = getIndexesForTrackedDeviceClass(klass);
        return indexes.Any() ? indexes[0] : uint.MaxValue;
    }

    private uint[] getIndexesForTrackedDeviceClass(ETrackedDeviceClass klass)
    {
        var data = new uint[OpenVR.k_unMaxTrackedDeviceCount];
        OpenVR.System.GetSortedTrackedDeviceIndicesOfClass(klass, data, OpenVR.k_unTrackedDeviceIndexInvalid);
        return data;
    }

    private bool getBoolTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        var error = new ETrackedPropertyError();
        var value = OpenVR.System.GetBoolTrackedDeviceProperty(index, property, ref error);

        if (error != ETrackedPropertyError.TrackedProp_Success)
        {
            log($"GetBoolTrackedDeviceProperty has given an error: {error}");
            return false;
        }

        return value;
    }

    private int getInt32TrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        var error = new ETrackedPropertyError();
        var value = OpenVR.System.GetInt32TrackedDeviceProperty(index, property, ref error);

        if (error != ETrackedPropertyError.TrackedProp_Success)
        {
            log($"GetInt32TrackedDeviceProperty has given an error: {error}");
            return 0;
        }

        return value;
    }

    private float getFloatTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        var error = new ETrackedPropertyError();
        var value = OpenVR.System.GetFloatTrackedDeviceProperty(index, property, ref error);

        if (error != ETrackedPropertyError.TrackedProp_Success)
        {
            log($"GetFloatTrackedDeviceProperty has given an error: {error}");
            return 0f;
        }

        return value;
    }

    private readonly StringBuilder sb = new((int)OpenVR.k_unMaxPropertyStringSize);

    private string getStringTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        var error = new ETrackedPropertyError();
        sb.Clear();
        OpenVR.System.GetStringTrackedDeviceProperty(index, property, sb, OpenVR.k_unMaxPropertyStringSize, ref error);

        if (error != ETrackedPropertyError.TrackedProp_Success)
        {
            log($"GetStringTrackedDeviceProperty has given an error: {error}");
            return string.Empty;
        }

        return sb.ToString();
    }

    #endregion

    private static void log(string message)
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
    public float IndexFinger;
    public float MiddleFinger;
    public float RingFinger;
    public float PinkyFinger;
}
