// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using NAudio.CoreAudioApi;
using PInvoke;

namespace VRCOSC.App.Utils;

public static class EnumerableExtensions
{
    public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach (var item in collection)
            action(item);
    }
}

public static class CollectionExtensions
{
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
            collection.Add(item);
    }

    public static void RemoveIf<T>(this ICollection<T> collection, Func<T, bool> callback)
    {
        var itemsToRemove = collection.Where(callback.Invoke).ToList();

        foreach (var itemToRemove in itemsToRemove)
        {
            collection.Remove(itemToRemove);
        }
    }
}

public static class StringExtensions
{
    public static string Pluralise(this string str) => str + (str.EndsWith("s") ? "'" : "'s");
}

public static class IntegerExtensions
{
    public static int Modulo(this int x, int m)
    {
        var r = x % m;
        return r < 0 ? r + m : r;
    }
}

public static class EnumExtensions
{
    public static string ToLookup(this Enum @enum) => @enum.ToString().ToLowerInvariant();

    public static Array GetEnumValues(this Type enumType) => Enum.GetValues(enumType);

    public static Array GetValues<T>(this T @enum) where T : Enum => Enum.GetValues(@enum.GetType());

    /// <summary>
    /// A fast alternative functionally equivalent to <see cref="Enum.HasFlag"/>, eliminating boxing in all scenarios.
    /// </summary>
    /// <param name="enumValue">The enum to check.</param>
    /// <param name="flag">The flag to check for.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool HasFlagFast<T>(this T enumValue, T flag) where T : unmanaged, Enum
    {
        // Note: Using a switch statement would eliminate inlining.

        if (sizeof(T) == 1)
        {
            byte value1 = Unsafe.As<T, byte>(ref enumValue);
            byte value2 = Unsafe.As<T, byte>(ref flag);
            return (value1 & value2) == value2;
        }

        if (sizeof(T) == 2)
        {
            short value1 = Unsafe.As<T, short>(ref enumValue);
            short value2 = Unsafe.As<T, short>(ref flag);
            return (value1 & value2) == value2;
        }

        if (sizeof(T) == 4)
        {
            int value1 = Unsafe.As<T, int>(ref enumValue);
            int value2 = Unsafe.As<T, int>(ref flag);
            return (value1 & value2) == value2;
        }

        if (sizeof(T) == 8)
        {
            long value1 = Unsafe.As<T, long>(ref enumValue);
            long value2 = Unsafe.As<T, long>(ref flag);
            return (value1 & value2) == value2;
        }

        throw new ArgumentException($"Invalid enum type provided: {typeof(T)}.");
    }
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
