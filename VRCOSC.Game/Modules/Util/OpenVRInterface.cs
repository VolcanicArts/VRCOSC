// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Valve.VR;

namespace VRCOSC.Game.Modules.Util;

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
                            Console.WriteLine($"Setting {touchedButton} state to true");
                        }

                        break;

                    case EVREventType.VREvent_ButtonUntouch:
                        var untouchedButton = (EVRButtonId)evenT.data.controller.button;

                        if (touchTracker.ContainsKey(untouchedButton))
                        {
                            touchTracker[untouchedButton] = false;
                            Console.WriteLine($"Setting {untouchedButton} state to false");
                        }

                        break;
                }
            } while (hasEvents);
        }
        catch (NullReferenceException) { }
    }

    public float? GetHMDBatteryPercentage()
    {
        try
        {
            var error = new ETrackedPropertyError();

            var id = getIndexesForTrackedDeviceClass(ETrackedDeviceClass.HMD)[0];
            var canProvideBattery = OpenVR.System.GetBoolTrackedDeviceProperty(id, ETrackedDeviceProperty.Prop_DeviceProvidesBatteryStatus_Bool, ref error);

            if (!canProvideBattery) return null;

            return getFloatTrackedDeviceProperty(id, ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float);
        }
        catch (NullReferenceException)
        {
            return null;
        }
    }

    public float GetLeftControllerBatteryPercentage()
    {
        var id = getControllerUint("left");
        return id == uint.MaxValue ? 0 : getFloatTrackedDeviceProperty(id, ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float);
    }

    public float GetRightControllerBatteryPercentage()
    {
        var id = getControllerUint("right");
        return id == uint.MaxValue ? 0 : getFloatTrackedDeviceProperty(id, ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float);
    }

    public IEnumerable<float> GetTrackersBatteryPercentages()
    {
        try
        {
            var ids = getIndexesForTrackedDeviceClass(ETrackedDeviceClass.GenericTracker);
            return ids.Select(id => getFloatTrackedDeviceProperty(id, ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float));
        }
        catch (NullReferenceException)
        {
            return Array.Empty<float>();
        }
    }

    private uint getControllerUint(string identifier)
    {
        try
        {
            var ids = getIndexesForTrackedDeviceClass(ETrackedDeviceClass.Controller);

            foreach (var id in ids)
            {
                var name = getStringTrackedDeviceProperty(id, ETrackedDeviceProperty.Prop_RenderModelName_String);
                if (name.Contains(identifier)) return id;
            }

            return uint.MaxValue;
        }
        catch (NullReferenceException)
        {
            return uint.MaxValue;
        }
    }

    private string getStringTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        var error = new ETrackedPropertyError();
        StringBuilder sb = new StringBuilder((int)OpenVR.k_unMaxPropertyStringSize);
        OpenVR.System.GetStringTrackedDeviceProperty(index, property, sb, OpenVR.k_unMaxPropertyStringSize, ref error);
        return sb.ToString();
    }

    private float getFloatTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        var error = new ETrackedPropertyError();
        return OpenVR.System.GetFloatTrackedDeviceProperty(index, property, ref error);
    }

    private List<uint> getIndexesForTrackedDeviceClass(ETrackedDeviceClass klass)
    {
        var result = new List<uint>();

        for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
        {
            if (OpenVR.System.GetTrackedDeviceClass(i) == klass) result.Add(i);
        }

        return result;
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
