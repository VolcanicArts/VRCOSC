// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NAudio.CoreAudioApi;
using PInvoke;

namespace VRCOSC.App.Utils;

public static class EnumerableExtensions
{
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (var item in enumerable)
            action(item);
    }

    /// <summary>
    /// Checks to see if source contains the same contents as other without having to sort and call SequenceEquals
    /// </summary>
    public static bool ContainsSame<T>(this IEnumerable<T> source, IEnumerable<T> other)
    {
        var sourceList = source.ToList();
        var otherList = other.ToList();

        return sourceList.Count == otherList.Count && sourceList.All(otherList.Contains);
    }
}

public static class CollectionExtensions
{
    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
            collection.Add(item);
    }

    /// <summary>
    /// Removes elements based on a predicate
    /// </summary>
    public static ICollection<T> RemoveIf<T>(this ICollection<T> collection, Func<T, bool> predicate)
    {
        var itemsToRemove = collection.Where(predicate.Invoke).ToList();

        foreach (var itemToRemove in itemsToRemove)
        {
            collection.Remove(itemToRemove);
        }

        return collection;
    }
}

public static class ArrayExtensions
{
    public static T[] NewCopy<T>(this T[] source, int length)
    {
        var destination = new T[length];
        Array.Copy(source, destination, length);
        return destination;
    }
}

public static class UriExtensions
{
    public static void OpenExternally(this Uri uri) => Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
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

        _ = User32.GetWindowThreadProcessId(foregroundWindowHandle, out int processId);

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
