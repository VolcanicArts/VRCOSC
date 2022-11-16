// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Valve.VR;

namespace VRCOSC.Game.Modules.Util;

public static class OpenVrInterface
{
    public static bool Init()
    {
        var err = new EVRInitError();
        OpenVR.Init(ref err, EVRApplicationType.VRApplication_Background);
        return err == EVRInitError.None;
    }

    public static float? GetHMDBatteryPercentage()
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

    public static float GetLeftControllerBatteryPercentage()
    {
        var id = getControllerUint("left");
        return id == uint.MaxValue ? 0 : getFloatTrackedDeviceProperty(id, ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float);
    }

    public static float GetRightControllerBatteryPercentage()
    {
        var id = getControllerUint("right");
        return id == uint.MaxValue ? 0 : getFloatTrackedDeviceProperty(id, ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float);
    }

    public static IEnumerable<float> GetTrackersBatteryPercentages()
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

    private static uint getControllerUint(string identifier)
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

    private static string getStringTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        var error = new ETrackedPropertyError();
        StringBuilder sb = new StringBuilder((int)OpenVR.k_unMaxPropertyStringSize);
        OpenVR.System.GetStringTrackedDeviceProperty(index, property, sb, OpenVR.k_unMaxPropertyStringSize, ref error);
        return sb.ToString();
    }

    private static float getFloatTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        var error = new ETrackedPropertyError();
        return OpenVR.System.GetFloatTrackedDeviceProperty(index, property, ref error);
    }

    private static List<uint> getIndexesForTrackedDeviceClass(ETrackedDeviceClass klass)
    {
        var result = new List<uint>();

        for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
        {
            if (OpenVR.System.GetTrackedDeviceClass(i) == klass) result.Add(i);
        }

        return result;
    }
}
