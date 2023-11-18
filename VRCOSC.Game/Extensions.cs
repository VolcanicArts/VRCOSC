// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using NAudio.CoreAudioApi;
using PInvoke;

namespace VRCOSC.Game;

public static class EnumExtensions
{
    public static string ToLookup(this Enum @enum) => @enum.ToString().ToLowerInvariant();
}

public static class TypeExtensions
{
    public static string ToReadableName(this Type type)
    {
        if (type.IsSubclassOf(typeof(Enum))) return "Enum";

        return Type.GetTypeCode(type) switch
        {
            TypeCode.Empty => "Null",
            TypeCode.Object => "Object",
            TypeCode.DBNull => "DBNull",
            TypeCode.Boolean => "Bool",
            TypeCode.Char => "Char",
            TypeCode.SByte => "Byte",
            TypeCode.Byte => "UByte",
            TypeCode.Int16 => "Short",
            TypeCode.UInt16 => "UShort",
            TypeCode.Int32 => "Int",
            TypeCode.UInt32 => "UInt",
            TypeCode.Int64 => "Long",
            TypeCode.UInt64 => "ULong",
            TypeCode.Single => "Float",
            TypeCode.Double => "Double",
            TypeCode.Decimal => "Decimal",
            TypeCode.DateTime => "DateTime",
            TypeCode.String => "String",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown type provided")
        };
    }
}

public static class ProcessExtensions
{
    public static string? GetActiveWindowTitle()
    {
        var foregroundWindowHandle = User32.GetForegroundWindow();
        if (foregroundWindowHandle == IntPtr.Zero) return null;

        User32.GetWindowThreadProcessId(foregroundWindowHandle, out int processId);

        if (processId <= 0) return null;

        try
        {
            return Process.GetProcessById(processId).ProcessName;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static SimpleAudioVolume? getProcessAudioVolume(string? processName)
    {
        if (processName is null) return null;

        var speakers = new MMDeviceEnumerator().GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

        for (var i = 0; i < speakers.AudioSessionManager.Sessions.Count; i++)
        {
            var session = speakers.AudioSessionManager.Sessions[i];
            if (session.GetSessionIdentifier.Contains(processName, StringComparison.InvariantCultureIgnoreCase)) return session.SimpleAudioVolume;
        }

        return null;
    }

    public static float RetrieveProcessVolume(string? processName) => getProcessAudioVolume(processName)?.Volume ?? 1f;

    public static void SetProcessVolume(string? processName, float percentage)
    {
        var processAudioVolume = getProcessAudioVolume(processName);
        if (processAudioVolume is null) return;

        processAudioVolume.Volume = percentage;
    }
}
