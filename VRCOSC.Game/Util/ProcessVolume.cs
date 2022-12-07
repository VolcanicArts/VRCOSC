// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Runtime.InteropServices;

// ReSharper disable SuspiciousTypeConversion.Global
// ReSharper disable InconsistentNaming

namespace VRCOSC.Game.Util;

// https://stackoverflow.com/questions/20938934/controlling-applications-volume-by-process-id
internal static class ProcessVolume
{
    internal static float? GetApplicationVolume(string processName)
    {
        ISimpleAudioVolume? volume = getVolumeObject(processName);
        if (volume == null)
            return null;

        volume.GetMasterVolume(out var percentage);
        Marshal.ReleaseComObject(volume);
        return percentage;
    }

    internal static bool? GetApplicationMute(string processName)
    {
        ISimpleAudioVolume? volume = getVolumeObject(processName);
        if (volume == null)
            return null;

        volume.GetMute(out var mute);
        Marshal.ReleaseComObject(volume);
        return mute;
    }

    internal static void SetApplicationVolume(string processName, float percentage)
    {
        ISimpleAudioVolume? volume = getVolumeObject(processName);
        if (volume == null)
            return;

        Guid guid = Guid.Empty;
        volume.SetMasterVolume(percentage, ref guid);
        Marshal.ReleaseComObject(volume);
    }

    internal static void SetApplicationMute(string processName, bool mute)
    {
        ISimpleAudioVolume? volume = getVolumeObject(processName);
        if (volume == null)
            return;

        Guid guid = Guid.Empty;
        volume.SetMute(mute, ref guid);
        Marshal.ReleaseComObject(volume);
    }

    private static ISimpleAudioVolume? getVolumeObject(string processName)
    {
        // get the speakers (1st render + multimedia) device
        IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
        deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out var speakers);

        // activate the session manager. we need the enumerator
        Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
        speakers.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out var o);
        IAudioSessionManager2 mgr = (IAudioSessionManager2)o;

        // enumerate sessions for on this device
        mgr.GetSessionEnumerator(out var sessionEnumerator);
        sessionEnumerator.GetCount(out var count);

        // search for an audio session with the required name
        // NOTE: we could also use the process id instead of the app name (with IAudioSessionControl2)
        ISimpleAudioVolume? volumeControl = null;

        for (int i = 0; i < count; i++)
        {
            sessionEnumerator.GetSession(i, out var ctl);
            ctl.GetSessionIdentifier(out var identifier);

            if (identifier.Contains(processName, StringComparison.InvariantCultureIgnoreCase))
            {
                volumeControl = ctl as ISimpleAudioVolume;
                break;
            }

            Marshal.ReleaseComObject(ctl);
        }

        Marshal.ReleaseComObject(sessionEnumerator);
        Marshal.ReleaseComObject(mgr);
        Marshal.ReleaseComObject(speakers);
        Marshal.ReleaseComObject(deviceEnumerator);
        return volumeControl;
    }
}

[ComImport]
[Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
internal class MMDeviceEnumerator
{
}

internal enum EDataFlow
{
    eRender,
    eCapture,
    eAll,
    EDataFlow_enum_count
}

internal enum ERole
{
    eConsole,
    eMultimedia,
    eCommunications,
    ERole_enum_count
}

[Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMMDeviceEnumerator
{
    int NotImpl1();

    [PreserveSig]
    int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppDevice);

    // the rest is not implemented
}

[Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMMDevice
{
    [PreserveSig]
    int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);

    // the rest is not implemented
}

[Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IAudioSessionManager2
{
    int NotImpl1();
    int NotImpl2();

    [PreserveSig]
    int GetSessionEnumerator(out IAudioSessionEnumerator SessionEnum);

    // the rest is not implemented
}

[Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IAudioSessionEnumerator
{
    [PreserveSig]
    int GetCount(out int SessionCount);

    [PreserveSig]
    int GetSession(int SessionCount, out IAudioSessionControl2 Session);
}

[Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface ISimpleAudioVolume
{
    [PreserveSig]
    int SetMasterVolume(float fLevel, ref Guid EventContext);

    [PreserveSig]
    int GetMasterVolume(out float pfLevel);

    [PreserveSig]
    int SetMute(bool bMute, ref Guid EventContext);

    [PreserveSig]
    int GetMute(out bool pbMute);
}

[Guid("bfb7ff88-7239-4fc9-8fa2-07c950be9c6d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IAudioSessionControl2
{
    // IAudioSessionControl
    [PreserveSig]
    int NotImpl0();

    [PreserveSig]
    int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

    [PreserveSig]
    int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string Value, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);

    [PreserveSig]
    int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

    [PreserveSig]
    int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string Value, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);

    [PreserveSig]
    int GetGroupingParam(out Guid pRetVal);

    [PreserveSig]
    int SetGroupingParam([MarshalAs(UnmanagedType.LPStruct)] Guid Override, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);

    [PreserveSig]
    int NotImpl1();

    [PreserveSig]
    int NotImpl2();

    // IAudioSessionControl2
    [PreserveSig]
    int GetSessionIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

    [PreserveSig]
    int GetSessionInstanceIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

    [PreserveSig]
    int GetProcessId(out int pRetVal);

    [PreserveSig]
    int IsSystemSoundsSession();

    [PreserveSig]
    int SetDuckingPreference(bool optOut);
}
