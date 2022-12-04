// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using osu.Framework.Logging;
using Valve.VR;

// ReSharper disable MemberCanBeMadeStatic.Global

namespace VRCOSC.Game.Modules.Util;

[SuppressMessage("Performance", "CA1822:Mark members as static")]
public class OpenVrInterface
{
    private readonly Dictionary<EVRButtonId, bool> touchTracker = new()
    {
        { EVRButtonId.k_EButton_IndexController_A, false },
        { EVRButtonId.k_EButton_IndexController_B, false },
        { EVRButtonId.k_EButton_SteamVR_Touchpad, false },
        { EVRButtonId.k_EButton_IndexController_JoyStick, false }
    };

    public bool Init()
    {
        var err = new EVRInitError();
        OpenVR.Init(ref err, EVRApplicationType.VRApplication_Utility);
        return err == EVRInitError.None;
    }

    public bool IsHmdPresent() => OpenVR.IsHmdPresent();
    public bool IsLeftControllerPresent() => getLeftControllerIndex() != uint.MaxValue && OpenVR.System.IsTrackedDeviceConnected(getLeftControllerIndex());
    public bool IsRightControllerPresent() => getRightControllerIndex() != uint.MaxValue && OpenVR.System.IsTrackedDeviceConnected(getRightControllerIndex());
    public bool IsTrackerPresent(int trackerNum) => trackerNum < getTrackers().Count() && OpenVR.System.IsTrackedDeviceConnected(getTrackerIndex(trackerNum));

    public bool IsHmdCharging() => CanHmdProvideBatteryData() && getBoolTrackedDeviceProperty(getHmdIndex(), ETrackedDeviceProperty.Prop_DeviceIsCharging_Bool);
    public bool IsLeftControllerCharging() => getBoolTrackedDeviceProperty(getLeftControllerIndex(), ETrackedDeviceProperty.Prop_DeviceIsCharging_Bool);
    public bool IsRightControllerCharging() => getBoolTrackedDeviceProperty(getRightControllerIndex(), ETrackedDeviceProperty.Prop_DeviceIsCharging_Bool);
    public bool IsTrackerCharging(int trackerNum) => getBoolTrackedDeviceProperty(getTrackerIndex(trackerNum), ETrackedDeviceProperty.Prop_DeviceIsCharging_Bool);

    public float GetHmdBatteryPercentage() => getFloatTrackedDeviceProperty(getHmdIndex(), ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float);
    public float GetLeftControllerBatteryPercentage() => getFloatTrackedDeviceProperty(getLeftControllerIndex(), ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float);
    public float GetRightControllerBatteryPercentage() => getFloatTrackedDeviceProperty(getRightControllerIndex(), ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float);
    public float GetTrackerBatteryPercentage(int trackerNum) => getFloatTrackedDeviceProperty(getTrackerIndex(trackerNum), ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float);

    public bool CanHmdProvideBatteryData()
    {
        var error = new ETrackedPropertyError();
        var canProvideBattery = OpenVR.System.GetBoolTrackedDeviceProperty(getHmdIndex(), ETrackedDeviceProperty.Prop_DeviceProvidesBatteryStatus_Bool, ref error);
        return error == ETrackedPropertyError.TrackedProp_Success && canProvideBattery;
    }

    #region Events

    public unsafe void Poll()
    {
        try
        {
            bool hasEvents;

            do
            {
                var evenT = new VREvent_t();
                hasEvents = OpenVR.System.PollNextEvent(ref evenT, (uint)sizeof(VREvent_t));

                switch ((EVREventType)evenT.eventType)
                {
                    case EVREventType.VREvent_ButtonTouch:
                        var touchedButton = (EVRButtonId)evenT.data.controller.button;

                        if (touchTracker.ContainsKey(touchedButton))
                        {
                            touchTracker[touchedButton] = true;
                        }

                        break;

                    case EVREventType.VREvent_ButtonUntouch:
                        var untouchedButton = (EVRButtonId)evenT.data.controller.button;

                        if (touchTracker.ContainsKey(untouchedButton))
                        {
                            touchTracker[untouchedButton] = false;
                        }

                        break;
                }
            } while (hasEvents);
        }
        catch (NullReferenceException) { }
    }

    #endregion

    #region OpenVR Abstraction

    private uint getHmdIndex() => getIndexForTrackedDeviceClass(ETrackedDeviceClass.HMD);
    private uint getLeftControllerIndex() => getController("left");
    private uint getRightControllerIndex() => getController("right");
    private uint getTrackerIndex(int trackerNum) => getTrackers().ToArray()[trackerNum];

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

    private IEnumerable<uint> getTrackers()
    {
        var controllers = getIndexesForTrackedDeviceClass(ETrackedDeviceClass.Controller);
        var trackers = getIndexesForTrackedDeviceClass(ETrackedDeviceClass.GenericTracker);

        foreach (var controller in controllers)
        {
            var controllerRole = (ETrackedControllerRole)getInt32TrackedDeviceProperty(controller, ETrackedDeviceProperty.Prop_ControllerRoleHint_Int32);
            // An invalid controller seems to always be a tracker that took a controller's place when a controller disconnected/reconnected
            if (controllerRole == ETrackedControllerRole.Invalid) yield return controller;
        }

        foreach (var tracker in trackers)
        {
            yield return tracker;
        }
    }

    private uint getIndexForTrackedDeviceClass(ETrackedDeviceClass klass) => getIndexesForTrackedDeviceClass(klass).ToArray()[0];

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

public enum OpenVRButton
{
    None,
    A,
    B,
    Stick,
    Pad
}
