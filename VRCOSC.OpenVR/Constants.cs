// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.Runtime.CompilerServices;
using Valve.VR;

namespace VRCOSC.OpenVR;

public static class Constants
{
    public const ulong INVALID_INPUT_HANDLE = Valve.VR.OpenVR.k_ulInvalidInputValueHandle;
    public const int MAX_DEVICE_COUNT = (int)Valve.VR.OpenVR.k_unMaxTrackedDeviceCount;
    public const int MAX_STRING_PROPERTY_SIZE = (int)Valve.VR.OpenVR.k_unMaxPropertyStringSize;

    public static readonly uint COMPOSITOR_FRAMETIMING_SIZE = (uint)Unsafe.SizeOf<Compositor_FrameTiming>();
    public static readonly uint INPUTANALOGACTIONDATA_T_SIZE = (uint)Unsafe.SizeOf<InputAnalogActionData_t>();
    public static readonly uint INPUTDIGITALACTIONDATA_T_SIZE = (uint)Unsafe.SizeOf<InputDigitalActionData_t>();
    public static readonly uint VRACTIVEACTONSET_T_SIZE = (uint)Unsafe.SizeOf<VRActiveActionSet_t>();
}
