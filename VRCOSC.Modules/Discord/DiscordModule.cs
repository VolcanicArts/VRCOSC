﻿// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using VRCOSC.Game.Modules;

namespace VRCOSC.Modules.Discord;

public sealed partial class DiscordModule : IntegrationModule
{
    public override string Title => "Discord";
    public override string Description => "Integration with the Discord desktop app";
    public override string Author => "VolcanicArts";
    public override string Prefab => "VRCOSC-Discord";
    public override ModuleType Type => ModuleType.Integrations;
    protected override string TargetProcess => "discord";

    protected override void CreateAttributes()
    {
        CreateParameter<bool>(DiscordParameter.Mic, ParameterMode.Read, "VRCOSC/Discord/Mic", "Becomes true to toggle the mic");
        CreateParameter<bool>(DiscordParameter.Deafen, ParameterMode.Read, "VRCOSC/Discord/Deafen", "Becomes true to toggle deafen");

        RegisterKeyCombination(DiscordParameter.Mic, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_LSHIFT, WindowsVKey.VK_M);
        RegisterKeyCombination(DiscordParameter.Deafen, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_LSHIFT, WindowsVKey.VK_D);
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