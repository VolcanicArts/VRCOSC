// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Valve.VR;

namespace VRCOSC.OpenVR;

[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public static class OVRHelper
{
    public static float FPS => 1 / (getFrameTimeMilli() / 1000f);

    public static Action<string>? OnError;

    private static void error(string methodName, ETrackedDeviceProperty property, ETrackedPropertyError error, uint index)
    {
        var name = GetStringTrackedDeviceProperty(index, ETrackedDeviceProperty.Prop_RenderModelName_String);
        OnError?.Invoke($"{methodName} encountered error {error} on device {name} when getting property {property}");
    }

    private static readonly StringBuilder trackeddeviceproperty_stringbuilder = new(Constants.MAX_STRING_PROPERTY_SIZE);

    internal static bool InitialiseOpenVR(EVRApplicationType applicationType)
    {
        var err = new EVRInitError();
        var state = Valve.VR.OpenVR.InitInternal(ref err, applicationType);
        return err == EVRInitError.None && state != 0;
    }

    private static float getFrameTimeMilli()
    {
        var frameTiming = new Compositor_FrameTiming
        {
            m_nSize = Constants.COMPOSITOR_FRAMETIMING_SIZE
        };
        Valve.VR.OpenVR.Compositor.GetFrameTiming(ref frameTiming, 0);
        return frameTiming.m_flTotalRenderGpuMs;
    }

    internal static InputAnalogActionData_t GetAnalogueInput(ulong identifier)
    {
        var data = new InputAnalogActionData_t();
        Valve.VR.OpenVR.Input.GetAnalogActionData(identifier, ref data, Constants.INPUTANALOGACTIONDATA_T_SIZE, Constants.INVALID_INPUT_HANDLE);
        return data;
    }

    internal static InputDigitalActionData_t GetDigitalInput(ulong identifier)
    {
        var data = new InputDigitalActionData_t();
        Valve.VR.OpenVR.Input.GetDigitalActionData(identifier, ref data, Constants.INPUTDIGITALACTIONDATA_T_SIZE, Constants.INVALID_INPUT_HANDLE);
        return data;
    }

    internal static bool GetBoolTrackedDeviceProperty(uint id, ETrackedDeviceProperty property)
    {
        var error = new ETrackedPropertyError();
        var value = Valve.VR.OpenVR.System.GetBoolTrackedDeviceProperty(id, property, ref error);

        if (error == ETrackedPropertyError.TrackedProp_Success) return value;

        OVRHelper.error(nameof(GetBoolTrackedDeviceProperty), property, error, id);
        return false;
    }

    internal static int GetInt32TrackedDeviceProperty(uint id, ETrackedDeviceProperty property)
    {
        var error = new ETrackedPropertyError();
        var value = Valve.VR.OpenVR.System.GetInt32TrackedDeviceProperty(id, property, ref error);

        if (error == ETrackedPropertyError.TrackedProp_Success) return value;

        OVRHelper.error(nameof(GetInt32TrackedDeviceProperty), property, error, id);
        return 0;
    }

    internal static float GetFloatTrackedDeviceProperty(uint id, ETrackedDeviceProperty property)
    {
        var error = new ETrackedPropertyError();
        var value = Valve.VR.OpenVR.System.GetFloatTrackedDeviceProperty(id, property, ref error);

        if (error == ETrackedPropertyError.TrackedProp_Success) return value;

        OVRHelper.error(nameof(GetFloatTrackedDeviceProperty), property, error, id);
        return 0f;
    }

    internal static string GetStringTrackedDeviceProperty(uint id, ETrackedDeviceProperty property)
    {
        var error = new ETrackedPropertyError();
        trackeddeviceproperty_stringbuilder.Clear();
        Valve.VR.OpenVR.System.GetStringTrackedDeviceProperty(id, property, trackeddeviceproperty_stringbuilder, Constants.MAX_STRING_PROPERTY_SIZE, ref error);

        if (error == ETrackedPropertyError.TrackedProp_Success) return trackeddeviceproperty_stringbuilder.ToString();

        OVRHelper.error(nameof(GetStringTrackedDeviceProperty), property, error, id);
        return string.Empty;
    }
}
