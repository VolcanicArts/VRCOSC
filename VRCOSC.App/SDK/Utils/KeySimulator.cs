// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json;
using WindowsInput;
using WindowsInput.Events;

namespace VRCOSC.App.SDK.Utils;

public static class KeySimulator
{
    public static async Task ExecuteKeybind(Keybind keybind, KeybindMode mode = KeybindMode.Press)
    {
        var keys = keybind.Modifiers.Concat(keybind.Keys).Select(key => (KeyCode)KeyInterop.VirtualKeyFromKey(key));

        switch (mode)
        {
            case KeybindMode.Press:
                await Simulate.Events().ClickChord(keys).Wait(50).Invoke();
                break;

            case KeybindMode.Hold:
                await Simulate.Events().Hold(keys).Invoke();
                break;

            case KeybindMode.Release:
                await Simulate.Events().Release(keys).Invoke();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }
}

[JsonObject(MemberSerialization.OptIn)]
public class Keybind
{
    [JsonProperty("modifiers")]
    public List<Key> Modifiers { get; set; } = [];

    [JsonProperty("keys")]
    public List<Key> Keys { get; set; } = [];
}

public enum KeybindMode
{
    Press,
    Hold,
    Release
}