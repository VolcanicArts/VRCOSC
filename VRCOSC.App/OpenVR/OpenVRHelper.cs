// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using Valve.VR;
using VRCOSC.App.OpenVR.Device;
using VRCOSC.App.Utils;

namespace VRCOSC.App.OpenVR;

internal static class OpenVRHelper
{
    private static void error(string methodName, ETrackedDeviceProperty property, ETrackedPropertyError error, uint index)
    {
        if (error == ETrackedPropertyError.TrackedProp_UnknownProperty) return;

        var name = GetStringTrackedDeviceProperty(index, ETrackedDeviceProperty.Prop_RenderModelName_String);
        Logger.Error(new Exception(), $"{methodName} encountered error {error} on device {name} when getting property {property}");
    }

    private static readonly uint compositor_frametiming_size = (uint)Unsafe.SizeOf<Compositor_FrameTiming>();
    private static readonly uint inputanalogactiondata_t_size = (uint)Unsafe.SizeOf<InputAnalogActionData_t>();
    private static readonly uint inputdigitalactiondata_t_size = (uint)Unsafe.SizeOf<InputDigitalActionData_t>();

    internal static bool InitialiseOpenVR(EVRApplicationType applicationType)
    {
        var err = EVRInitError.None;
        Valve.VR.OpenVR.Init(ref err, applicationType);
        return err == EVRInitError.None;
    }

    internal static float GetFrameTimeMilli()
    {
        var frameTiming = new Compositor_FrameTiming
        {
            m_nSize = compositor_frametiming_size
        };

        Valve.VR.OpenVR.Compositor.GetFrameTiming(ref frameTiming, 0);
        return frameTiming.m_flTotalRenderGpuMs;
    }

    internal static Transform[] GetPoses()
    {
        var transforms = new Transform[Valve.VR.OpenVR.k_unMaxTrackedDeviceCount];

        var poses = new TrackedDevicePose_t[Valve.VR.OpenVR.k_unMaxTrackedDeviceCount];
        Valve.VR.OpenVR.System.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, poses);

        for (var i = 0; i < poses.Length; i++)
        {
            var pose = poses[i];

            if (!pose.bPoseIsValid)
            {
                transforms[i] = Transform.Identity;
                continue;
            }

            var mat = pose.mDeviceToAbsoluteTracking;

            var position = new Vector3(mat.m3, mat.m7, mat.m11);

            var matrix = new Matrix4x4(
                mat.m0, mat.m1, mat.m2, 0,
                mat.m4, mat.m5, mat.m6, 0,
                mat.m8, mat.m9, mat.m10, 0,
                0, 0, 0, 1
            );

            var rotation = Quaternion.CreateFromRotationMatrix(matrix);

            transforms[i] = new Transform(position, rotation);
        }

        return transforms;
    }

    internal static InputAnalogActionData_t GetAnalogueInput(ulong identifier)
    {
        var data = new InputAnalogActionData_t();
        Valve.VR.OpenVR.Input.GetAnalogActionData(identifier, ref data, inputanalogactiondata_t_size, Valve.VR.OpenVR.k_ulInvalidInputValueHandle);
        return data;
    }

    internal static InputDigitalActionData_t GetDigitalInput(ulong identifier)
    {
        var data = new InputDigitalActionData_t();
        Valve.VR.OpenVR.Input.GetDigitalActionData(identifier, ref data, inputdigitalactiondata_t_size, Valve.VR.OpenVR.k_ulInvalidInputValueHandle);
        return data;
    }

    internal static uint GetDeviceIndexForRole(DeviceRole role)
    {
        var deviceHandle = Valve.VR.OpenVR.k_ulInvalidActionHandle;

        switch (role)
        {
            case DeviceRole.Head:
                Valve.VR.OpenVR.Input.GetInputSourceHandle(Valve.VR.OpenVR.k_pchPathUserHead, ref deviceHandle);
                break;

            case DeviceRole.Chest:
                Valve.VR.OpenVR.Input.GetInputSourceHandle(Valve.VR.OpenVR.k_pchPathUserChest, ref deviceHandle);
                break;

            case DeviceRole.Waist:
                Valve.VR.OpenVR.Input.GetInputSourceHandle(Valve.VR.OpenVR.k_pchPathUserWaist, ref deviceHandle);
                break;

            case DeviceRole.LeftHand:
                Valve.VR.OpenVR.Input.GetInputSourceHandle(Valve.VR.OpenVR.k_pchPathUserHandLeft, ref deviceHandle);
                break;

            case DeviceRole.RightHand:
                Valve.VR.OpenVR.Input.GetInputSourceHandle(Valve.VR.OpenVR.k_pchPathUserHandRight, ref deviceHandle);
                break;

            case DeviceRole.LeftElbow:
                Valve.VR.OpenVR.Input.GetInputSourceHandle(Valve.VR.OpenVR.k_pchPathUserElbowLeft, ref deviceHandle);
                break;

            case DeviceRole.RightElbow:
                Valve.VR.OpenVR.Input.GetInputSourceHandle(Valve.VR.OpenVR.k_pchPathUserElbowRight, ref deviceHandle);
                break;

            case DeviceRole.LeftFoot:
                Valve.VR.OpenVR.Input.GetInputSourceHandle(Valve.VR.OpenVR.k_pchPathUserFootLeft, ref deviceHandle);
                break;

            case DeviceRole.RightFoot:
                Valve.VR.OpenVR.Input.GetInputSourceHandle(Valve.VR.OpenVR.k_pchPathUserFootRight, ref deviceHandle);
                break;

            case DeviceRole.LeftKnee:
                Valve.VR.OpenVR.Input.GetInputSourceHandle(Valve.VR.OpenVR.k_pchPathUserKneeLeft, ref deviceHandle);
                break;

            case DeviceRole.RightKnee:
                Valve.VR.OpenVR.Input.GetInputSourceHandle(Valve.VR.OpenVR.k_pchPathUserKneeRight, ref deviceHandle);
                break;

            case DeviceRole.Unset:
                deviceHandle = Valve.VR.OpenVR.k_ulInvalidActionHandle;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(role), role, null);
        }

        if (deviceHandle == Valve.VR.OpenVR.k_ulInvalidActionHandle) return Valve.VR.OpenVR.k_unTrackedDeviceIndexInvalid;

        var originInfo = new InputOriginInfo_t();
        Valve.VR.OpenVR.Input.GetOriginTrackedDeviceInfo(deviceHandle, ref originInfo, (uint)Unsafe.SizeOf<InputOriginInfo_t>());
        return originInfo.trackedDeviceIndex;
    }

    internal static IEnumerable<uint> GetAllDeviceIndexes()
    {
        for (var i = 0u; i < Valve.VR.OpenVR.k_unMaxTrackedDeviceCount; i++)
        {
            if (Valve.VR.OpenVR.System.GetTrackedDeviceClass(i) is ETrackedDeviceClass.GenericTracker or ETrackedDeviceClass.Controller or ETrackedDeviceClass.HMD) yield return i;
        }
    }

    internal static bool GetBoolTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        if (index == Valve.VR.OpenVR.k_unTrackedDeviceIndexInvalid) return false;

        var error = ETrackedPropertyError.TrackedProp_Success;
        var value = Valve.VR.OpenVR.System.GetBoolTrackedDeviceProperty(index, property, ref error);

        if (error == ETrackedPropertyError.TrackedProp_Success) return value;

        OpenVRHelper.error(nameof(GetBoolTrackedDeviceProperty), property, error, index);
        return false;
    }

    internal static int GetInt32TrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        if (index == Valve.VR.OpenVR.k_unTrackedDeviceIndexInvalid) return 0;

        var error = ETrackedPropertyError.TrackedProp_Success;
        var value = Valve.VR.OpenVR.System.GetInt32TrackedDeviceProperty(index, property, ref error);

        if (error == ETrackedPropertyError.TrackedProp_Success) return value;

        OpenVRHelper.error(nameof(GetInt32TrackedDeviceProperty), property, error, index);
        return 0;
    }

    internal static float GetFloatTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        if (index == Valve.VR.OpenVR.k_unTrackedDeviceIndexInvalid) return 0f;

        var error = ETrackedPropertyError.TrackedProp_Success;
        var value = Valve.VR.OpenVR.System.GetFloatTrackedDeviceProperty(index, property, ref error);

        if (error == ETrackedPropertyError.TrackedProp_Success) return value;

        OpenVRHelper.error(nameof(GetFloatTrackedDeviceProperty), property, error, index);
        return 0f;
    }

    internal static string GetStringTrackedDeviceProperty(uint index, ETrackedDeviceProperty property)
    {
        if (index == Valve.VR.OpenVR.k_unTrackedDeviceIndexInvalid) return string.Empty;

        var error = ETrackedPropertyError.TrackedProp_Success;
        var sb = new StringBuilder((int)Valve.VR.OpenVR.k_unMaxPropertyStringSize);

        Valve.VR.OpenVR.System.GetStringTrackedDeviceProperty(index, property, sb, Valve.VR.OpenVR.k_unMaxPropertyStringSize, ref error);

        if (error == ETrackedPropertyError.TrackedProp_Success) return sb.ToString();

        OpenVRHelper.error(nameof(GetStringTrackedDeviceProperty), property, error, index);
        return string.Empty;
    }

    internal static void TriggerHaptic(ulong action, uint device, float durationSeconds, float frequency, float amplitude)
    {
        if (action == Valve.VR.OpenVR.k_ulInvalidActionHandle) return;
        if (device == Valve.VR.OpenVR.k_unTrackedDeviceIndexInvalid) return;

        Valve.VR.OpenVR.Input.TriggerHapticVibrationAction(action, 0, durationSeconds, frequency, amplitude, device);
    }
}