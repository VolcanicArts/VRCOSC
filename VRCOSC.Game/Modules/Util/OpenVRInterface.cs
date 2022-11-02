// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Collections.Generic;
using System.Linq;
using System.Text;
using osu.Framework.Logging;
using Valve.VR;

namespace VRCOSC.Game.Modules.Util;

public static class OpenVrInterface
{
    public static void Init()
    {
        var err = new EVRInitError();
        OpenVR.Init(ref err, EVRApplicationType.VRApplication_Background);
        Logger.Log($"OpenVR: {err}");
    }

    public static float? GetHMDBatteryPercentage()
    {
        var error = new ETrackedPropertyError();

        var id = getIndexesForTrackedDeviceClass(ETrackedDeviceClass.HMD)[0];
        var canProvideBattery = OpenVR.System.GetBoolTrackedDeviceProperty(id, ETrackedDeviceProperty.Prop_DeviceProvidesBatteryStatus_Bool, ref error);
        Logger.Log($"OpenVR: {error}");

        if (!canProvideBattery) return null;

        var battery = getFloatTrackedDeviceProperty(id, ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float);
        return battery;
    }

    public static float? GetLeftControllerBatteryPercentage()
    {
        var id = getControllerUint("left");
        return id == uint.MaxValue ? null : getFloatTrackedDeviceProperty(id, ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float);
    }

    public static float? GetRightControllerBatteryPercentage()
    {
        var id = getControllerUint("right");
        return id == uint.MaxValue ? null : getFloatTrackedDeviceProperty(id, ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float);
    }

    public static IEnumerable<float> GetTrackersBatteryPercentages()
    {
        var ids = getIndexesForTrackedDeviceClass(ETrackedDeviceClass.GenericTracker);
        return ids.Select(id => getFloatTrackedDeviceProperty(id, ETrackedDeviceProperty.Prop_DeviceBatteryPercentage_Float));
    }

    private static uint getControllerUint(string identifier)
    {
        var ids = getIndexesForTrackedDeviceClass(ETrackedDeviceClass.Controller);

        foreach (var id in ids)
        {
            var name = getStringTrackedDeviceProperty(id, ETrackedDeviceProperty.Prop_RenderModelName_String);
            if (name.Contains(identifier)) return id;
        }

        return uint.MaxValue;
    }

    private static string getStringTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        var error = new ETrackedPropertyError();
        StringBuilder sb = new StringBuilder((int)OpenVR.k_unMaxPropertyStringSize);
        OpenVR.System.GetStringTrackedDeviceProperty(index, property, sb, OpenVR.k_unMaxPropertyStringSize, ref error);
        Logger.Log($"OpenVR: {error}");
        return sb.ToString();
    }

    private static float getFloatTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        var error = new ETrackedPropertyError();
        var result = OpenVR.System.GetFloatTrackedDeviceProperty(index, property, ref error);
        Logger.Log($"OpenVR: {error}");
        return result;
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
