// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using PInvoke;
using VRCOSC.Game.Modules;

namespace VRCOSC.Modules.Discord;

[ModuleTitle("Discord")]
[ModuleDescription("Basic integration with the Discord desktop app")]
[ModuleAuthor("VolcanicArts", "https://github.com/VolcanicArts", "https://avatars.githubusercontent.com/u/29819296?v=4")]
[ModuleGroup(ModuleType.Integrations)]
[ModulePrefab("VRCOSC-Discord", "https://github.com/VolcanicArts/VRCOSC/releases/download/latest/VRCOSC-Discord.unitypackage")]
public sealed class DiscordModule : IntegrationModule
{
    protected override string TargetProcess => "discord";

    protected override void CreateAttributes()
    {
        CreateParameter<bool>(DiscordParameter.Mic, ParameterMode.Read, "VRCOSC/Discord/Mic", "Mic", "Becomes true to toggle the mic");
        CreateParameter<bool>(DiscordParameter.Deafen, ParameterMode.Read, "VRCOSC/Discord/Deafen", "Deafen", "Becomes true to toggle deafen");

        RegisterKeyCombination(DiscordParameter.Mic, User32.VirtualKey.VK_LCONTROL, User32.VirtualKey.VK_LSHIFT, User32.VirtualKey.VK_M);
        RegisterKeyCombination(DiscordParameter.Deafen, User32.VirtualKey.VK_LCONTROL, User32.VirtualKey.VK_LSHIFT, User32.VirtualKey.VK_D);
    }

    protected override void OnBoolParameterReceived(Enum key, bool value)
    {
        if (value) ExecuteKeyCombination(key);
    }

    private enum DiscordParameter
    {
        Mic,
        Deafen
    }
}
