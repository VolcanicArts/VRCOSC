// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Modules.Modules.DiscordVoice;

public class DiscordVoiceModule : Module
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

    private const int keyeventf_keyup = 0x0002;

    private const int vk_ctrl = 0x11;
    private const int vk_l_shift = 0xA0;
    private const int vk_m = 0x4D;

    public override string Title => "Discord Voice";
    public override string Description => "Allows for toggling of your Discord microphone";
    public override string Author => "VolcanicArts";
    public override Colour4 Colour => Color4Extensions.FromHex(@"454FBF");
    public override ModuleType Type => ModuleType.Integrations;

    public override IReadOnlyCollection<Enum> InputParameters => new List<Enum>
    {
        DiscordVoiceInputParameters.DiscordVoice
    };

    protected override void OnParameterReceived(Enum key, object value)
    {
        var buttonPressed = (bool)value;
        if (!buttonPressed) return;

        Terminal.Log("Toggling Discord voice");

        Task.Run(async () =>
        {
            Process? discordProcess = Process.GetProcessesByName("discord").FirstOrDefault();
            Process? vrchatProcess = Process.GetProcessesByName("vrchat").FirstOrDefault();

            if (discordProcess == null)
            {
                Terminal.Log("Cannot toggle microphone. Discord is not running");
                return;
            }

            if (vrchatProcess == null)
            {
                Terminal.Log("How are you even using this. VRChat isn't running");
                return;
            }

            ProcessHelper.ShowMainWindow(discordProcess, ShowWindowEnum.Restore);
            ProcessHelper.ShowMainWindow(discordProcess, ShowWindowEnum.ShowMaximized);
            await Task.Delay(10);
            ProcessHelper.SetMainWindowForeground(discordProcess);
            await Task.Delay(10);
            await executeMute();
            await Task.Delay(10);
            ProcessHelper.ShowMainWindow(vrchatProcess, ShowWindowEnum.Restore);
            ProcessHelper.ShowMainWindow(vrchatProcess, ShowWindowEnum.ShowMaximized);
            await Task.Delay(10);
            ProcessHelper.SetMainWindowForeground(vrchatProcess);
        }).ConfigureAwait(false);
    }

    private static async Task executeMute()
    {
        holdKey(vk_ctrl);
        holdKey(vk_l_shift);
        holdKey(vk_m);
        await Task.Delay(10);
        releaseKey(vk_ctrl);
        releaseKey(vk_l_shift);
        releaseKey(vk_m);
    }

    private static void holdKey(int key) => keybd_event((byte)key, (byte)key, 0, 0);
    private static void releaseKey(int key) => keybd_event((byte)key, (byte)key, keyeventf_keyup, 0);
}

public enum DiscordVoiceInputParameters
{
    DiscordVoice
}
