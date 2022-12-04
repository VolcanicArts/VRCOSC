// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using osu.Framework.Lists;
using osu.Framework.Logging;
using osu.Framework.Platform;
using Valve.VR;

// ReSharper disable MemberCanBeMadeStatic.Global

namespace VRCOSC.Game.Modules.Util;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class OpenVRInterface
{
    public IndexControllerData LeftIndexControllerData = new();
    public IndexControllerData RightIndexControllerData = new();

    private ulong actionSetHandle;
    private readonly ulong[] leftController = new ulong[8];
    private readonly ulong[] rightController = new ulong[8];

    private Storage storage;
    public bool HasSession { get; private set; }

    public OpenVRInterface(Storage storage)
    {
        this.storage = storage.GetStorageForDirectory("temp");
    }

    public bool Init()
    {
        var err = new EVRInitError();
        OpenVR.Init(ref err, EVRApplicationType.VRApplication_Utility);

        if (err != EVRInitError.None) return false;

        HasSession = true;

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

        return true;
    }

    public bool IsHmdPresent() => getIndexForTrackedDeviceClass(ETrackedDeviceClass.HMD) != uint.MaxValue && OpenVR.IsHmdPresent();
    public bool IsLeftControllerPresent() => getLeftControllerIndex() != uint.MaxValue && OpenVR.System.IsTrackedDeviceConnected(getLeftControllerIndex());
    public bool IsRightControllerPresent() => getRightControllerIndex() != uint.MaxValue && OpenVR.System.IsTrackedDeviceConnected(getRightControllerIndex());
    public bool IsTrackerPresent(uint trackerIndex) => trackerIndex != uint.MaxValue && OpenVR.System.IsTrackedDeviceConnected(trackerIndex);

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

    public IndexControllerData GetLeftIndexControllerData() => LeftIndexControllerData;
    public IndexControllerData GetRightIndexControllerData() => RightIndexControllerData;

    #region Events

    public unsafe void Poll()
    {
        if (!HasSession) return;

        try
        {
            bool hasEvents;

            do
            {
                var evenT = new VREvent_t();

                hasEvents = OpenVR.System.PollNextEvent(ref evenT, (uint)sizeof(VREvent_t));

                if (!HasSession) break;

                switch ((EVREventType)evenT.eventType)
                {
                    case EVREventType.VREvent_Quit:
                        OpenVR.System.AcknowledgeQuit_Exiting();
                        OpenVR.Shutdown();
                        HasSession = false;
                        break;
                }

                var activeActionSet = new VRActiveActionSet_t[] { new() };
                activeActionSet[0].ulActionSet = actionSetHandle;
                activeActionSet[0].ulRestrictedToDevice = OpenVR.k_ulInvalidInputValueHandle;
                activeActionSet[0].nPriority = 0;
                OpenVR.Input.UpdateActionState(activeActionSet, (uint)sizeof(VRActiveActionSet_t));
            } while (hasEvents);
        }
        catch (NullReferenceException) { }

        if (HasSession) extractIndexControllerData();
    }

    private void extractIndexControllerData()
    {
        LeftIndexControllerData.ATouched = getDigitalInput(leftController[0]).bState;
        LeftIndexControllerData.BTouched = getDigitalInput(leftController[1]).bState;
        LeftIndexControllerData.PadTouched = getDigitalInput(leftController[2]).bState;
        LeftIndexControllerData.StickTouched = getDigitalInput(leftController[3]).bState;
        LeftIndexControllerData.IndexFinger = getAnalogueInput(leftController[4]).x;
        LeftIndexControllerData.MiddleFinger = getAnalogueInput(leftController[5]).x;
        LeftIndexControllerData.RingFinger = getAnalogueInput(leftController[6]).x;
        LeftIndexControllerData.PinkyFinger = getAnalogueInput(leftController[7]).x;

        RightIndexControllerData.ATouched = getDigitalInput(rightController[0]).bState;
        RightIndexControllerData.BTouched = getDigitalInput(rightController[1]).bState;
        RightIndexControllerData.PadTouched = getDigitalInput(rightController[2]).bState;
        RightIndexControllerData.StickTouched = getDigitalInput(rightController[3]).bState;
        RightIndexControllerData.IndexFinger = getAnalogueInput(rightController[4]).x;
        RightIndexControllerData.MiddleFinger = getAnalogueInput(rightController[5]).x;
        RightIndexControllerData.RingFinger = getAnalogueInput(rightController[6]).x;
        RightIndexControllerData.PinkyFinger = getAnalogueInput(rightController[7]).x;
    }

    private unsafe InputAnalogActionData_t getAnalogueInput(ulong identifier)
    {
        var data = new InputAnalogActionData_t();
        OpenVR.Input.GetAnalogActionData(identifier, ref data, (uint)sizeof(InputAnalogActionData_t), OpenVR.k_ulInvalidInputValueHandle);
        return data;
    }

    private unsafe InputDigitalActionData_t getDigitalInput(ulong identifier)
    {
        var data = new InputDigitalActionData_t();
        OpenVR.Input.GetDigitalActionData(identifier, ref data, (uint)sizeof(InputDigitalActionData_t), OpenVR.k_ulInvalidInputValueHandle);
        return data;
    }

    #endregion

    #region OpenVR Abstraction

    private uint getHmdIndex() => getIndexForTrackedDeviceClass(ETrackedDeviceClass.HMD);
    private uint getLeftControllerIndex() => getController("left");
    private uint getRightControllerIndex() => getController("right");

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

    public IEnumerable<uint> GetTrackers()
    {
        var trackersList = new SortedList<uint>();

        var controllers = getIndexesForTrackedDeviceClass(ETrackedDeviceClass.Controller);

        foreach (var controller in controllers)
        {
            var controllerRole = (ETrackedControllerRole)getInt32TrackedDeviceProperty(controller, ETrackedDeviceProperty.Prop_ControllerRoleHint_Int32);
            // An invalid controller seems to always be a tracker that took a controller's place when a controller disconnected/reconnected
            if (controllerRole == ETrackedControllerRole.Invalid) trackersList.Add(controller);
        }

        trackersList.AddRange(getIndexesForTrackedDeviceClass(ETrackedDeviceClass.GenericTracker));
        trackersList.Sort();
        return trackersList;
    }

    private uint getIndexForTrackedDeviceClass(ETrackedDeviceClass klass)
    {
        var indexes = getIndexesForTrackedDeviceClass(klass).ToArray();
        return indexes.Any() ? indexes[0] : uint.MaxValue;
    }

    private IEnumerable<uint> getIndexesForTrackedDeviceClass(ETrackedDeviceClass klass)
    {
        for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
        {
            if (OpenVR.System.GetTrackedDeviceClass(i) == klass) yield return i;
        }
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

    private string getStringTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        var error = new ETrackedPropertyError();
        var sb = new StringBuilder((int)OpenVR.k_unMaxPropertyStringSize);
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

public class IndexControllerData
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
