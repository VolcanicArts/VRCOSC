// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Valve.VR;
using VRCOSC.App.OVR;
using VRCOSC.App.Utils;

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

    internal static bool InitialiseOpenVR(EVRApplicationType applicationType)
    {
        var err = EVRInitError.None;
        OpenVR.Init(ref err, applicationType);
        return err == EVRInitError.None;
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

    internal static Transform GetTrackedPose(string serialNumber)
    {
        var system = OpenVR.System;

        if (system == null)
            return Transform.Identity;

        for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
        {
            if (!system.IsTrackedDeviceConnected(i))
                continue;

            var deviceSerial = GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_SerialNumber_String);

            if (string.IsNullOrEmpty(deviceSerial))
                continue;

            if (deviceSerial == serialNumber)
            {
                var poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
                OpenVR.System.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, poses);

                var pose = poses[i];

                if (!pose.bPoseIsValid)
                    return Transform.Identity;

                var mat = pose.mDeviceToAbsoluteTracking;

                var position = new Vector3(mat.m3, mat.m7, mat.m11);

                var matrix = new Matrix4x4(
                    mat.m0, mat.m1, mat.m2, 0,
                    mat.m4, mat.m5, mat.m6, 0,
                    mat.m8, mat.m9, mat.m10, 0,
                    0, 0, 0, 1
                );

                var rotation = Quaternion.CreateFromRotationMatrix(matrix);

                var role = OVRDeviceManager.GetInstance().GetTrackedDevice(serialNumber)?.Role;

                // Apply role-based rotation correction (Z-up to Z-forward)
                if (role is DeviceRole.Waist or DeviceRole.Chest or DeviceRole.LeftFoot or DeviceRole.RightFoot or DeviceRole.LeftKnee or DeviceRole.RightKnee or DeviceRole.LeftElbow or DeviceRole.RightElbow)
                {
                    rotation = Quaternion.Multiply(rotation, Quaternion.CreateFromAxisAngle(Vector3.UnitX, -MathF.PI / 2));
                }

                return new Transform(position, rotation);
            }
        }

        return Transform.Identity;
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

    internal static IEnumerable<uint> GetIndexesForTrackedDeviceClass(ETrackedDeviceClass klass)
    {
        for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
        {
            if (OpenVR.System.GetTrackedDeviceClass(i) == klass) yield return i;
        }
    }

    internal static bool GetBoolTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        if (index == OpenVR.k_unTrackedDeviceIndexInvalid) return false;

        var error = ETrackedPropertyError.TrackedProp_Success;
        var value = OpenVR.System.GetBoolTrackedDeviceProperty(index, property, ref error);

        if (error == ETrackedPropertyError.TrackedProp_Success) return value;

        OVRHelper.error(nameof(GetBoolTrackedDeviceProperty), property, error, index);
        return false;
    }

    internal static int GetInt32TrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        if (index == OpenVR.k_unTrackedDeviceIndexInvalid) return 0;

        var error = ETrackedPropertyError.TrackedProp_Success;
        var value = OpenVR.System.GetInt32TrackedDeviceProperty(index, property, ref error);

        if (error == ETrackedPropertyError.TrackedProp_Success) return value;

        OVRHelper.error(nameof(GetInt32TrackedDeviceProperty), property, error, index);
        return 0;
    }

    internal static float GetFloatTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        if (index == OpenVR.k_unTrackedDeviceIndexInvalid) return 0f;

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
}