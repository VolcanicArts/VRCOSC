// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
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
    /// <remarks>
    /// When run once immediately is true, NewItems will contain the entire collection.
    /// Returns an IDisposable that, when disposed, unregisters the event handler.
    /// </remarks>
    public static IDisposable OnCollectionChanged<T>(this ObservableCollection<T> collection, Action<IEnumerable<T>, IEnumerable<T>> callback, bool runOnceImmediately = false)
    {
        NotifyCollectionChangedEventHandler handler = (_, e) =>
        {
            callback.Invoke(
                e.NewItems?.Cast<T>() ?? Array.Empty<T>(),
                e.OldItems?.Cast<T>() ?? Array.Empty<T>());
        };

        collection.CollectionChanged += handler;

        if (runOnceImmediately)
        {
            callback.Invoke(collection, Array.Empty<T>());
        }

        return new DisposableAction(() => collection.CollectionChanged -= handler);
    }

    private class DisposableAction : IDisposable
    {
        private readonly Action _disposeAction;
        private bool _disposed;

        public DisposableAction(Action disposeAction)
        {
            _disposeAction = disposeAction;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposeAction();
                _disposed = true;
            }
        }
    }
}

public static class TaskExtensions
{
    public static void Forget(this Task task)
    {
        if (task.IsCompleted)
        {
            if (task.IsFaulted) ExceptionHandler.Handle(task.Exception!.GetBaseException(), "A forgotten task has thrown an exception");
            return;
        }

        _ = task.ContinueWith(t => ExceptionHandler.Handle(t.Exception!.GetBaseException(), "A forgotten task has thrown an exception"),
            TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
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

public static class ParameterInfoExtensions
{
    public static bool TryGetCustomAttribute<T>(this ParameterInfo info, [NotNullWhen(true)] out T? attribute) where T : Attribute
    {
        attribute = info.GetCustomAttribute<T>();
        return attribute is not null;
    }

    public static bool HasCustomAttribute<T>(this ParameterInfo info) where T : Attribute => info.GetCustomAttribute<T>() is not null;
}

public static class TypeExtensions
{
    public static object? CreateDefault(this Type type) => type.IsValueType && !type.IsAssignableTo(typeof(Nullable<>)) ? Activator.CreateInstance(type) : null;

    public static Type? GetConstructedGenericBase(this Type typeToCheck, Type genericDef)
    {
        if (!genericDef.IsGenericTypeDefinition)
            throw new ArgumentException("Must be an open generic, e.g. typeof(MyBase<>)", nameof(genericDef));

        var cur = typeToCheck;

        while (cur != null && cur != typeof(object))
        {
            var bt = cur.BaseType;

            if (bt is { IsGenericType: true } &&
                bt.GetGenericTypeDefinition() == genericDef)
            {
                return bt;
            }

            cur = bt;
        }

        return null;
    }

    public static bool IsSubclassOfRawGeneric(this Type typeToCheck, Type genericTypeDefinition)
    {
        if (!genericTypeDefinition.IsGenericTypeDefinition)
            throw new ArgumentException("Must be a generic type definition, e.g. typeof(B<>)", nameof(genericTypeDefinition));

        if (typeToCheck.IsGenericType && typeToCheck.GetGenericTypeDefinition() == genericTypeDefinition) return true;
        if (typeToCheck.GetInterfaces().Any(iface => iface.IsGenericType && iface.GetGenericTypeDefinition() == genericTypeDefinition)) return true;

        var baseType = typeToCheck.BaseType;
        return baseType != null && IsSubclassOfRawGeneric(baseType, genericTypeDefinition);
    }

    public static string GetFriendlyName(this ParameterInfo pi)
    {
        var ctx = new NullabilityInfoContext();
        var nullInfo = ctx.Create(pi);
        return pi.ParameterType.GetFriendlyName(false, nullInfo);
    }

    public static string GetFriendlyName(this PropertyInfo pi)
    {
        var ctx = new NullabilityInfoContext();
        var nullInfo = ctx.Create(pi);
        return pi.PropertyType.GetFriendlyName(false, nullInfo);
    }

    /// <summary>
    /// Core formatter: walks the Type (and its nested generic args), consulting the parallel nullInfo tree.
    /// </summary>
    public static string GetFriendlyName(this Type t, bool includeNamespace = false, NullabilityInfo? nullInfo = null)
    {
        // 1) Handle Nullable<T> on value types
        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            // unwrap T and its nullability info
            var innerType = t.GetGenericArguments()[0];
            return innerType.GetFriendlyName(includeNamespace) + "?";
        }

        // 2) Handle generic types (e.g. Dictionary<,>, IList<> on interfaces, etc.)
        string baseName;

        if (t.IsGenericParameter)
        {
            baseName = includeNamespace ? t.FullName! : t.Name;
        }
        else if (t.IsGenericType)
        {
            // strip the `1, `2, etc.
            var name = includeNamespace ? t.FullName! : t.Name;
            var idx = name.IndexOf('`');
            if (idx >= 0) name = name[..idx];

            // recurse into each argument, carrying along its nullInfo
            var args = t.GetGenericArguments();

            var argNames = args
                           .Select((argType, i) => argType.GetFriendlyName(includeNamespace, nullInfo?.GenericTypeArguments[i]))
                           .ToArray();

            baseName = $"{name}<{string.Join(", ", argNames)}>";
        }
        else
        {
            // simple non‐generic
            baseName = includeNamespace ? t.FullName! : t.toReadableName();
        }

        // 3) If this is a reference type (class, interface, delegate, array, etc.)
        //    and the metadata says it's nullable, append “?”
        if (!t.IsValueType && nullInfo?.ReadState == NullabilityState.Nullable)
        {
            baseName += "?";
        }

        return baseName;
    }

    private static string toReadableName(this Type type)
    {
        if (type.IsEnum) return type.Name;
        if (type == typeof(object)) return "object";

        return Type.GetTypeCode(type) switch
        {
            TypeCode.Empty => "null",
            TypeCode.Boolean => "bool",
            TypeCode.Char => "char",
            TypeCode.Byte => "byte",
            TypeCode.SByte => "sbyte",
            TypeCode.Int16 => "short",
            TypeCode.UInt16 => "ushort",
            TypeCode.Int32 => "int",
            TypeCode.UInt32 => "uint",
            TypeCode.Int64 => "long",
            TypeCode.UInt64 => "ulong",
            TypeCode.Single => "float",
            TypeCode.Double => "double",
            TypeCode.Decimal => "decimal",
            TypeCode.String => "string",
            _ => type.Name
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