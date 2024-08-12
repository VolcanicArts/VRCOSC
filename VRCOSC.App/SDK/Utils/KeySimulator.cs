// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;

namespace VRCOSC.App.SDK.Utils;

public static class KeySimulator
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public int type;
        public InputUnion u;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public Mouseinput mi;

        [FieldOffset(0)]
        public Keybdinput ki;

        [FieldOffset(0)]
        public Hardwareinput hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Mouseinput
    {
        public int dx;
        public int dy;
        public int mouseData;
        public int dwFlags;
        public int time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Keybdinput
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Hardwareinput
    {
        public int uMsg;
        public short wParamL;
        public short wParamH;
    }

    private const int input_keyboard = 1;
    private const uint keyeventf_keyup = 0x0002;
    private const uint keyeventf_scancode = 0x0008;

    public static async Task ExecuteKeybind(Keybind keybind, int holdTimeMilli = 50)
    {
        var inputs = new Input[4];
        var inputIndex = 0;

        if (keybind.Modifiers != ModifierKeys.None)
        {
            foreach (ModifierKeys modifier in Enum.GetValues(typeof(ModifierKeys)))
            {
                if (modifier is ModifierKeys.None) continue;

                if (keybind.Modifiers.HasFlag(modifier))
                {
                    inputs[inputIndex].type = input_keyboard;

                    inputs[inputIndex].u.ki = new Keybdinput
                    {
                        wVk = (ushort)KeyInterop.VirtualKeyFromKey(getKeyFromModifier(modifier)),
                        dwFlags = 0,
                        dwExtraInfo = IntPtr.Zero
                    };
                    inputIndex++;
                }
            }
        }

        inputs[inputIndex].type = input_keyboard;

        inputs[inputIndex].u.ki = new Keybdinput
        {
            wVk = (ushort)KeyInterop.VirtualKeyFromKey(keybind.Key),
            dwFlags = 0,
            dwExtraInfo = IntPtr.Zero
        };
        inputIndex++;

        _ = SendInput((uint)inputIndex, inputs, Marshal.SizeOf(typeof(Input)));

        await Task.Delay(holdTimeMilli);

        inputs[inputIndex - 1].u.ki.dwFlags = keyeventf_keyup;
        _ = SendInput(1, new[] { inputs[inputIndex - 1] }, Marshal.SizeOf(typeof(Input)));

        for (var i = 0; i < inputIndex - 1; i++)
        {
            inputs[i].u.ki.dwFlags = keyeventf_keyup;
            _ = SendInput(1, new[] { inputs[i] }, Marshal.SizeOf(typeof(Input)));
        }
    }

    private static Key getKeyFromModifier(ModifierKeys modifier)
    {
        switch (modifier)
        {
            case ModifierKeys.Alt:
                return Key.LeftAlt;

            case ModifierKeys.Control:
                return Key.LeftCtrl;

            case ModifierKeys.Shift:
                return Key.LeftShift;

            case ModifierKeys.Windows:
                return Key.LWin;

            default:
                throw new ArgumentOutOfRangeException(nameof(modifier), modifier, null);
        }
    }
}

[JsonObject(MemberSerialization.OptIn)]
public class Keybind
{
    [JsonProperty("modifiers")]
    public ModifierKeys Modifiers { get; set; }

    [JsonProperty("key")]
    public Key Key { get; set; }
}
