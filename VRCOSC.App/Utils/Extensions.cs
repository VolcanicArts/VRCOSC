// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;
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

public static class ObservableCollectionExtensions
{
    /// <summary>
    /// Binds a callback to collection changed with the option of running once immediately.
    /// Callback is (NewItems, OldItems)
    /// </summary>
    /// <remarks>When run once immediately is true, NewItems will contain the collection</remarks>
    public static void OnCollectionChanged<T>(this ObservableCollection<T> collection, Action<IEnumerable<T>, IEnumerable<T>> callback, bool runOnceImmediately = false)
    {
        collection.CollectionChanged += (_, e) =>
        {
            callback.Invoke(e.NewItems?.Cast<T>() ?? Array.Empty<T>(), e.OldItems?.Cast<T>() ?? Array.Empty<T>());
        };

        if (runOnceImmediately)
        {
            callback.Invoke(collection, Array.Empty<T>());
        }
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

public static class KeyExtensions
{
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int ToUnicodeEx(
        uint wVirtKey,
        uint wScanCode,
        byte[] lpKeyState,
        [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff,
        int cchBuff,
        uint wFlags,
        IntPtr dwhkl);

    [DllImport("user32.dll")]
    private static extern uint MapVirtualKey(uint uCode, uint uMapType);

    [DllImport("user32.dll")]
    private static extern IntPtr GetKeyboardLayout(uint idThread);

    public static string ToReadableString(this Key key)
    {
        // Handle special cases for non-character keys
        switch (key)
        {
            case Key.Tab:
                return "Tab";

            case Key.Enter:
                return "Enter";

            case Key.Escape:
                return "Escape";

            case Key.Space:
                return "Space";

            case Key.Back:
                return "Backspace";

            case Key.Insert:
                return "Insert";

            case Key.Delete:
                return "Delete";

            case Key.Home:
                return "Home";

            case Key.End:
                return "End";

            case Key.PageUp:
                return "Page Up";

            case Key.PageDown:
                return "Page Down";

            case Key.Left:
                return "Left Arrow";

            case Key.Right:
                return "Right Arrow";

            case Key.Up:
                return "Up Arrow";

            case Key.Down:
                return "Down Arrow";

            case Key.LeftCtrl:
                return "Left Ctrl";

            case Key.RightCtrl:
                return "Right Ctrl";

            case Key.LeftShift:
                return "Left Shift";

            case Key.RightShift:
                return "Right Shift";

            case Key.LeftAlt:
                return "Left Alt";

            case Key.RightAlt:
                return "Right Alt";
        }

        var virtualKey = (uint)KeyInterop.VirtualKeyFromKey(key);
        var keyboardState = new byte[256];

        // Only take the current key into account to stop modifiers from ruining the representation
        keyboardState[virtualKey] = 0x80;

        var scanCode = MapVirtualKey(virtualKey, 0x02);
        var sb = new StringBuilder(10);
        var keyboardLayout = GetKeyboardLayout(0);

        var result = ToUnicodeEx(virtualKey, scanCode, keyboardState, sb, sb.Capacity, 0, keyboardLayout);
        return result > 0 ? sb.Length == 1 ? sb.ToString().ToUpperInvariant() : sb.ToString() : key.ToString();
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

    public static bool HasConstructorThatAccepts(this Type targetType, params Type[] parameterTypes)
    {
        return targetType.GetConstructors().Any(constructorInfo =>
        {
            var parameters = constructorInfo.GetParameters();
            return parameters.Length == parameterTypes.Length && !parameters.Where((parameterInfo, i) => !parameterInfo.ParameterType.IsAssignableTo(parameterTypes[i])).Any();
        });
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

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    private const int sw_minimize = 6;
    private const int sw_restore = 9;

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    public static void SetWindowVisibility(this Process process, bool visible)
    {
        if (process == null) throw new ArgumentNullException(nameof(process));

        var windowHandle = findWindowByProcessId(process.Id);
        if (windowHandle == IntPtr.Zero) throw new InvalidOperationException("The process does not have a visible main window");

        ShowWindow(windowHandle, visible ? sw_restore : sw_minimize);
    }

    private static IntPtr findWindowByProcessId(int processId)
    {
        IntPtr windowHandle = IntPtr.Zero;

        EnumWindows((hWnd, _) =>
        {
            GetWindowThreadProcessId(hWnd, out uint windowProcessId);

            if (windowProcessId == processId)
            {
                windowHandle = hWnd;
                return false;
            }

            return true;
        }, IntPtr.Zero);

        return windowHandle;
    }
}