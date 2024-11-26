﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Valve.VR;

namespace VRCOSC.App.SDK.OVR;

internal static class OVRHelper
{
    internal static Action<string>? OnError;

    private static void error(string methodName, ETrackedDeviceProperty property, ETrackedPropertyError error, uint index)
    {
        if (error == ETrackedPropertyError.TrackedProp_UnknownProperty) return;

        var name = GetStringTrackedDeviceProperty(index, ETrackedDeviceProperty.Prop_RenderModelName_String);
        OnError?.Invoke($"{methodName} encountered error {error} on device {name} when getting property {property}");
    }

    private static readonly uint compositor_frametiming_size = (uint)Unsafe.SizeOf<Compositor_FrameTiming>();
    private static readonly uint inputanalogactiondata_t_size = (uint)Unsafe.SizeOf<InputAnalogActionData_t>();
    private static readonly uint inputdigitalactiondata_t_size = (uint)Unsafe.SizeOf<InputDigitalActionData_t>();

    // YEUSEPE
    internal static bool InitialiseOpenVR(EVRApplicationType applicationType)
    {
        var err = EVRInitError.None;
        OpenVR.Init(ref err, applicationType);

        if (err != EVRInitError.None)
        {
            OnError?.Invoke($"OpenVR.Init failed with error: {err}");
            return false;
        }

        return true;
    }


    internal static float GetFrameTimeMilli()
    {
        var frameTiming = new Compositor_FrameTiming
        {
            m_nSize = compositor_frametiming_size
        };

        OpenVR.Compositor.GetFrameTiming(ref frameTiming, 0);
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

    // GetTrackedDeviceIndexForControllerRole doesn't work when a tracker thinks it's a controller and assumes that role
    // We can forcibly find the correct indexes by using the model name
    public static IEnumerable<uint> GetAllControllersFromHint(string controllerHint)
    {
        return GetIndexesForTrackedDeviceClass(ETrackedDeviceClass.Controller)
            .Where(index => GetStringTrackedDeviceProperty(index, ETrackedDeviceProperty.Prop_RenderModelName_String).Contains(controllerHint, StringComparison.InvariantCultureIgnoreCase));
    }

    internal static uint GetIndexForTrackedDeviceClass(ETrackedDeviceClass klass)
    {
        var indexes = GetIndexesForTrackedDeviceClass(klass).ToArray();
        return indexes.Length != 0 ? indexes[0] : OpenVR.k_unTrackedDeviceIndexInvalid;
    }


    // YEUSEPE: Added a safety check try catch to this, as devices can be nullified. 
    internal static IEnumerable<uint> GetIndexesForTrackedDeviceClass(ETrackedDeviceClass klass)
    {
        if (!IsSystemAvailable()) yield break;


        for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
        {
            if (SafeCheckDeviceClass(i, klass))
            {
                yield return i;
            }
        }
    }

    private static bool SafeCheckDeviceClass(uint index, ETrackedDeviceClass klass)
    {
        try
        {
            return OpenVR.System.GetTrackedDeviceClass(index) == klass;
        }
        catch (Exception ex)
        {
            // Handle exception, e.g., log it
            // Console.WriteLine($"Error at index {index}: {ex.Message}, {ex.InnerException}, {ex.StackTrace}");
            return false;
        }
    }



    internal static bool GetBoolTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        if (index == OpenVR.k_unTrackedDeviceIndexInvalid) return default;

        var error = ETrackedPropertyError.TrackedProp_Success;
        var value = OpenVR.System.GetBoolTrackedDeviceProperty(index, property, ref error);

        if (error == ETrackedPropertyError.TrackedProp_Success) return value;

        OVRHelper.error(nameof(GetBoolTrackedDeviceProperty), property, error, index);
        return false;
    }

    internal static int GetInt32TrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        if (index == OpenVR.k_unTrackedDeviceIndexInvalid) return default;

        var error = ETrackedPropertyError.TrackedProp_Success;
        var value = OpenVR.System.GetInt32TrackedDeviceProperty(index, property, ref error);

        if (error == ETrackedPropertyError.TrackedProp_Success) return value;

        OVRHelper.error(nameof(GetInt32TrackedDeviceProperty), property, error, index);
        return 0;
    }

    internal static float GetFloatTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        if (index == OpenVR.k_unTrackedDeviceIndexInvalid) return default;

        var error = ETrackedPropertyError.TrackedProp_Success;
        var value = OpenVR.System.GetFloatTrackedDeviceProperty(index, property, ref error);

        if (error == ETrackedPropertyError.TrackedProp_Success) return value;

        OVRHelper.error(nameof(GetFloatTrackedDeviceProperty), property, error, index);
        return 0f;
    }

    private static readonly StringBuilder sb = new((int)OpenVR.k_unMaxPropertyStringSize);

    internal static string GetStringTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        if (index == OpenVR.k_unTrackedDeviceIndexInvalid) return string.Empty;

        var error = ETrackedPropertyError.TrackedProp_Success;
        sb.Clear();
        OpenVR.System.GetStringTrackedDeviceProperty(index, property, sb, OpenVR.k_unMaxPropertyStringSize, ref error);

        if (error == ETrackedPropertyError.TrackedProp_Success) return sb.ToString();

        OVRHelper.error(nameof(GetStringTrackedDeviceProperty), property, error, index);
        return string.Empty;
    }

    internal static void TriggerHaptic(ulong action, uint device, float durationSeconds, float frequency, float amplitude)
    {
        if (action == OpenVR.k_ulInvalidActionHandle) return;
        if (device == OpenVR.k_unTrackedDeviceIndexInvalid) return;

        OpenVR.Input.TriggerHapticVibrationAction(action, 0, durationSeconds, frequency, amplitude, device);
    }

    // YEUSEPE: Added a function to check that the system is correctly intitialized. 
    private static bool IsSystemAvailable()
    {
        if (OpenVR.System == null)
        {
            // OnError?.Invoke("OpenVR.System is not initialized or has been shut down.");
            return false;
        }
        return true;
    }

}
