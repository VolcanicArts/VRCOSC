// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using osu.Framework.Graphics;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Modules.Modules.Spotify;

public class SpotifyModule : Module
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

    private const int keyeventf_keyup = 0x0002;

    public override string Title => "Spotify";
    public override string Description => "Integration with the Spotify desktop app";
    public override string Author => "VolcanicArts";
    public override Colour4 Colour => Colour4.Green.Darken(0.5f);
    public override ModuleType Type => ModuleType.Integrations;

    public override IReadOnlyCollection<Enum> InputParameters => new List<Enum>
    {
        SpotifyInputParameters.SpotifyPlayPause,
        SpotifyInputParameters.SpotifyNext,
        SpotifyInputParameters.SpotifyPrevious,
        SpotifyInputParameters.SpotifyVolumeUp,
        SpotifyInputParameters.SpotifyVolumeDown
    };

    private readonly IReadOnlyDictionary<SpotifyInputParameters, int[]> keyCombinations = new Dictionary<SpotifyInputParameters, int[]>()
    {
        { SpotifyInputParameters.SpotifyPlayPause, new[] { 0x20 } },
        { SpotifyInputParameters.SpotifyNext, new[] { 0xA2, 0x27 } },
        { SpotifyInputParameters.SpotifyPrevious, new[] { 0xA2, 0x25 } },
        { SpotifyInputParameters.SpotifyVolumeUp, new[] { 0xA2, 0x26 } },
        { SpotifyInputParameters.SpotifyVolumeDown, new[] { 0xA2, 0x28 } }
    };

    protected override void OnParameterReceived(Enum key, object value)
    {
        var buttonPressed = (bool)value;
        if (!buttonPressed) return;

        Terminal.Log($"Received input of {key}");

        executeTask((SpotifyInputParameters)key).ConfigureAwait(false);
    }

    private async Task executeTask(SpotifyInputParameters action)
    {
        if (!retrieveSpotifyProcess(out var spotifyProcess)) return;
        if (!retrieveVrChatProcess(out var vrChatProcess)) return;

        await focusProcess(spotifyProcess!);
        await executeKeyCombination(action);
        await focusProcess(vrChatProcess!);
    }

    private bool retrieveSpotifyProcess(out Process? spotifyProcess)
    {
        spotifyProcess = Process.GetProcessesByName("spotify").FirstOrDefault();

        if (spotifyProcess != null) return true;

        Terminal.Log("Spotify is not running. Please run Spotify to use this Module");
        return false;
    }

    private bool retrieveVrChatProcess(out Process? vrChatProcess)
    {
        vrChatProcess = Process.GetProcessesByName("vrchat").FirstOrDefault();

        if (vrChatProcess != null) return true;

        Terminal.Log("VRChat is not running. How are you using this module");
        return false;
    }

    private static async Task focusProcess(Process process)
    {
        await Task.Delay(10);
        ProcessHelper.ShowMainWindow(process, ShowWindowEnum.Restore);
        await Task.Delay(10);
        ProcessHelper.ShowMainWindow(process, ShowWindowEnum.ShowMaximized);
        await Task.Delay(10);
        ProcessHelper.SetMainWindowForeground(process);
        await Task.Delay(10);
    }

    private async Task executeKeyCombination(SpotifyInputParameters parameter)
    {
        var keys = keyCombinations[parameter];

        foreach (var key in keys)
        {
            holdKey(key);
        }

        await Task.Delay(10);

        foreach (var key in keys)
        {
            releaseKey(key);
        }
    }

    private static void holdKey(int key) => keybd_event((byte)key, (byte)key, 0, 0);
    private static void releaseKey(int key) => keybd_event((byte)key, (byte)key, keyeventf_keyup, 0);
}

public enum SpotifyInputParameters
{
    SpotifyPlayPause,
    SpotifyNext,
    SpotifyPrevious,
    SpotifyVolumeUp,
    SpotifyVolumeDown
}
