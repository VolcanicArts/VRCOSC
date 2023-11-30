// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Valve.VR;

namespace VRCOSC.Game.OVR;

[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public static class OVRHelper
{
    public static Action<string>? OnError;

    private static void error(string methodName, ETrackedDeviceProperty property, ETrackedPropertyError error, uint index)
    {
        if (error == ETrackedPropertyError.TrackedProp_UnknownProperty) return;

        var name = GetStringTrackedDeviceProperty(index, ETrackedDeviceProperty.Prop_RenderModelName_String);
        OnError?.Invoke($"{methodName} encountered error {error} on device {name} when getting property {property}");
    }

    private static readonly uint compositor_frametiming_size = (uint)Unsafe.SizeOf<Compositor_FrameTiming>();
    private static readonly uint inputanalogactiondata_t_size = (uint)Unsafe.SizeOf<InputAnalogActionData_t>();
    private static readonly uint inputdigitalactiondata_t_size = (uint)Unsafe.SizeOf<InputDigitalActionData_t>();

    internal static bool InitialiseOpenVR(EVRApplicationType applicationType)
    {
        var err = new EVRInitError();
        OpenVR.Init(ref err, applicationType);
        return err == EVRInitError.None;
    }

    internal static float GetFrameTimeMilli()
    {
        var frameTiming = new Compositor_FrameTiming
        {
            m_nSize = compositor_frametiming_size
        };
        OpenVR.Compositor.GetFrameTiming(ref frameTiming, 60);
        return frameTiming.m_flTotalRenderGpuMs;
    }

    internal static InputAnalogActionData_t GetAnalogueInput(ulong identifier)
    {
        var data = new InputAnalogActionData_t();
        OpenVR.Input.GetAnalogActionData(identifier, ref data, inputanalogactiondata_t_size, OpenVR.k_ulInvalidInputValueHandle);
        return data;
    }

    internal static InputDigitalActionData_t GetDigitalInput(ulong identifier)
    {
        var data = new InputDigitalActionData_t();
        OpenVR.Input.GetDigitalActionData(identifier, ref data, inputdigitalactiondata_t_size, OpenVR.k_ulInvalidInputValueHandle);
        return data;
    }

    internal static uint GetLeftControllerId()
    {
        var id = getControllerIdFromHint("left");
        return id != OpenVR.k_unTrackedDeviceIndexInvalid ? id : OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
    }

    internal static uint GetRightControllerId()
    {
        var id = getControllerIdFromHint("right");
        return id != OpenVR.k_unTrackedDeviceIndexInvalid ? id : OpenVR.System.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
    }

    // GetTrackedDeviceIndexForControllerRole doesn't work when a tracker thinks it's a controller and assumes that role
    // We can forcibly find the correct indexes by using the model name
    private static uint getControllerIdFromHint(string controllerHint)
    {
        var controllerIds = getAllControllersFromHint(controllerHint).ToList();

        if (!controllerIds.Any()) return OpenVR.k_unTrackedDeviceIndexInvalid;

        // prioritise the latest connected controller
        var connectedId = controllerIds.Where(index => OpenVR.System.IsTrackedDeviceConnected(index))
                                       .LastOrDefault(OpenVR.k_unTrackedDeviceIndexInvalid);

        return connectedId != OpenVR.k_unTrackedDeviceIndexInvalid ? connectedId : controllerIds.Last();
    }

    private static IEnumerable<uint> getAllControllersFromHint(string controllerHint)
    {
        return GetIndexesForTrackedDeviceClass(ETrackedDeviceClass.Controller)
            .Where(index => GetStringTrackedDeviceProperty(index, ETrackedDeviceProperty.Prop_RenderModelName_String).Contains(controllerHint, StringComparison.InvariantCultureIgnoreCase));
    }

    internal static uint GetIndexForTrackedDeviceClass(ETrackedDeviceClass klass)
    {
        var indexes = GetIndexesForTrackedDeviceClass(klass).ToArray();
        return indexes.Any() ? indexes[0] : OpenVR.k_unTrackedDeviceIndexInvalid;
    }

    internal static IEnumerable<uint> GetIndexesForTrackedDeviceClass(ETrackedDeviceClass klass)
    {
        for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
        {
            if (OpenVR.System.GetTrackedDeviceClass(i) == klass) yield return i;
        }
    }

    internal static bool GetBoolTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        var error = new ETrackedPropertyError();
        var value = OpenVR.System.GetBoolTrackedDeviceProperty(index, property, ref error);

        if (error == ETrackedPropertyError.TrackedProp_Success) return value;

        OVRHelper.error(nameof(GetBoolTrackedDeviceProperty), property, error, index);
        return false;
    }

    internal static int GetInt32TrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        var error = new ETrackedPropertyError();
        var value = OpenVR.System.GetInt32TrackedDeviceProperty(index, property, ref error);

        if (error == ETrackedPropertyError.TrackedProp_Success) return value;

        OVRHelper.error(nameof(GetInt32TrackedDeviceProperty), property, error, index);
        return 0;
    }

    internal static float GetFloatTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        var error = new ETrackedPropertyError();
        var value = OpenVR.System.GetFloatTrackedDeviceProperty(index, property, ref error);

        if (error == ETrackedPropertyError.TrackedProp_Success) return value;

        OVRHelper.error(nameof(GetFloatTrackedDeviceProperty), property, error, index);
        return 0f;
    }

    private static readonly StringBuilder sb = new((int)OpenVR.k_unMaxPropertyStringSize);

    internal static string GetStringTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        var error = new ETrackedPropertyError();
        sb.Clear();
        OpenVR.System.GetStringTrackedDeviceProperty(index, property, sb, OpenVR.k_unMaxPropertyStringSize, ref error);

        if (error == ETrackedPropertyError.TrackedProp_Success) return sb.ToString();

        OVRHelper.error(nameof(GetStringTrackedDeviceProperty), property, error, index);
        return string.Empty;
    }

    public static void TriggerHaptic(ulong action, uint device, float durationSeconds, float frequency, float amplitude)
    {
        OpenVR.Input.TriggerHapticVibrationAction(action, 0, durationSeconds, frequency, amplitude, device);
    }
}
