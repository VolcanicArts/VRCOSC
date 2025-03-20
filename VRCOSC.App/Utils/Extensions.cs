// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;
using Windows.Win32.UI.WindowsAndMessaging;
using NAudio.CoreAudioApi;
using NAudio.CoreAudioApi.Interfaces;

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
    public static string Pluralise(this string str) => str + (str.EndsWith('s') ? "'" : "'s");
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
}

public static class KeyExtensions
{
    public static unsafe string ToReadableString(this Key key)
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

        var scanCode = PInvoke.MapVirtualKey(virtualKey, MAP_VIRTUAL_KEY_TYPE.MAPVK_VK_TO_CHAR);
        var output = new char[10];

        fixed (byte* keyboardStatePtr = keyboardState)
        {
            fixed (char* outputPtr = output)
            {
                var keyboardLayout = PInvoke.GetKeyboardLayout(0);

                var result = PInvoke.ToUnicodeEx(virtualKey, scanCode, keyboardStatePtr, new PWSTR(outputPtr), output.Length, 0, keyboardLayout);
                return result > 0 ? output[0] != 0x0 && output[1] == 0x0 ? output[0].ToString().ToUpperInvariant() : new string(output) : key.ToString();
            }
        }
    }
}

public static class MemberInfoExtensions
{
    public static bool TryGetCustomAttribute<T>(this MemberInfo info, [NotNullWhen(true)] out T? attribute) where T : Attribute
    {
        attribute = info.GetCustomAttribute<T>();
        return attribute is not null;
    }

    public static bool HasCustomAttribute<T>(this MemberInfo info) where T : Attribute => info.GetCustomAttribute<T>() is not null;
}

public static class TypeExtensions
{
    public static string ToReadableName(this Type type)
    {
        if (type.IsSubclassOf(typeof(Enum))) return "Enum";

        return Type.GetTypeCode(type) switch
        {
            TypeCode.Empty => "Null",
            TypeCode.Object => type.Name,
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
    public static unsafe string? GetActiveWindowTitle()
    {
        var foregroundWindowHandle = PInvoke.GetForegroundWindow();
        if (foregroundWindowHandle == IntPtr.Zero) return null;

        var processId = 0u;
        var result = PInvoke.GetWindowThreadProcessId(foregroundWindowHandle, &processId);
        if (result == 0u || processId == 0u) return null;

        try
        {
            return Process.GetProcessById((int)processId).ProcessName;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static SimpleAudioVolume? getProcessAudioVolume(string? processName)
    {
        if (processName is null) return null;

        var audioEndPoints = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

        foreach (var audioEndPoint in audioEndPoints)
        {
            for (var i = 0; i < audioEndPoint.AudioSessionManager.Sessions.Count; i++)
            {
                var session = audioEndPoint.AudioSessionManager.Sessions[i];

                var isValidSession = session.GetSessionIdentifier.Contains(processName, StringComparison.InvariantCultureIgnoreCase)
                                     && session.State == AudioSessionState.AudioSessionStateActive;

                if (isValidSession) return session.SimpleAudioVolume;
            }
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

    public static void SetWindowVisibility(this Process process, bool visible)
    {
        var windowHandle = findWindowByProcessId((uint)process.Id);
        if (windowHandle == IntPtr.Zero) throw new InvalidOperationException("The process does not have a visible main window");

        var result = PInvoke.ShowWindow(new HWND(windowHandle), visible ? SHOW_WINDOW_CMD.SW_RESTORE : SHOW_WINDOW_CMD.SW_MINIMIZE);
        logResult(result, $"Could not alter window state with ID {process.Id}");
    }

    private static unsafe IntPtr findWindowByProcessId(uint processId)
    {
        var windowHandle = IntPtr.Zero;

        var result = PInvoke.EnumWindows((hWnd, _) =>
        {
            var windowProcessId = 0u;

            var result = PInvoke.GetWindowThreadProcessId(hWnd, &windowProcessId);
            if (result == 0u || windowProcessId != processId) return true;

            windowHandle = hWnd;
            return false;
        }, IntPtr.Zero);

        logResult(result, $"Could not find window process with ID {processId}");

        return windowHandle;
    }

    private static void logResult(BOOL result, string message)
    {
        if (!result) Logger.Log($"{message}. Error code: {Marshal.GetLastWin32Error()}");
    }
}