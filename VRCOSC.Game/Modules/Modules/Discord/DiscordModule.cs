// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Modules.Modules.Discord;

public sealed class DiscordModule : IntegrationModule
{
    public override string Title => "Discord";
    public override string Description => "Integration with the Discord desktop app";
    public override string Author => "VolcanicArts";
    public override string Prefab => "VRCOSC-Discord";
    public override ModuleType ModuleType => ModuleType.Integrations;
    protected override string TargetProcess => "discord";

    protected override void CreateAttributes()
    {
        CreateButtonParameter(DiscordIncomingParameter.Mic, "VRCOSC/Discord/Mic", "Becomes true to toggle the mic");
        CreateButtonParameter(DiscordIncomingParameter.Deafen, "VRCOSC/Discord/Deafen", "Becomes true to toggle deafen");

        RegisterKeyCombination(DiscordIncomingParameter.Mic, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_LSHIFT, WindowsVKey.VK_M);
        RegisterKeyCombination(DiscordIncomingParameter.Deafen, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_LSHIFT, WindowsVKey.VK_D);
    }

    protected override void OnButtonPressed(Enum key)
    {
        ExecuteKeyCombination(key);
    }

    private enum DiscordIncomingParameter
    {
        Mic,
        Deafen
    }
}
