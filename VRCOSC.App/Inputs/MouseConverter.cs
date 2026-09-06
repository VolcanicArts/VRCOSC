// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using SharpHook.Data;

namespace VRCOSC.App.Inputs;

public static class MouseConverter
{
    public static MouseButton ToSharpHook(this System.Windows.Input.MouseButton button) => button switch
    {
        System.Windows.Input.MouseButton.Left => MouseButton.Button1,
        System.Windows.Input.MouseButton.Middle => MouseButton.Button3,
        System.Windows.Input.MouseButton.Right => MouseButton.Button2,
        System.Windows.Input.MouseButton.XButton1 => MouseButton.Button4,
        System.Windows.Input.MouseButton.XButton2 => MouseButton.Button5,
        _ => throw new ArgumentOutOfRangeException(nameof(button), button, null)
    };

    public static System.Windows.Input.MouseButton ToWpf(this MouseButton button) => button switch
    {
        MouseButton.NoButton => System.Windows.Input.MouseButton.Left,
        MouseButton.Button1 => System.Windows.Input.MouseButton.Left,
        MouseButton.Button2 => System.Windows.Input.MouseButton.Right,
        MouseButton.Button3 => System.Windows.Input.MouseButton.Middle,
        MouseButton.Button4 => System.Windows.Input.MouseButton.XButton1,
        MouseButton.Button5 => System.Windows.Input.MouseButton.XButton2,
        _ => throw new ArgumentOutOfRangeException(nameof(button), button, null)
    };
}
