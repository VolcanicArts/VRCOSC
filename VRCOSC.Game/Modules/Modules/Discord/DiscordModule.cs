// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Colour;
using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Modules.Modules.Discord;

public class DiscordModule : IntegrationModule
{
    public override string Title => "Discord";
    public override string Description => "Integration with the Discord desktop app";
    public override string Author => "VolcanicArts";
    public override string Prefab => "VRCOSC-Discord";
    public override ColourInfo Colour => Color4Extensions.FromHex(@"454FBF");
    public override ModuleType ModuleType => ModuleType.Integrations;
    protected override string TargetProcess => "discord";

    protected override void CreateAttributes()
    {
        RegisterButtonInput(DiscordInputParameter.DiscordMic);
        RegisterButtonInput(DiscordInputParameter.DiscordDeafen);

        RegisterKeyCombination(DiscordInputParameter.DiscordMic, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_LSHIFT, WindowsVKey.VK_M);
        RegisterKeyCombination(DiscordInputParameter.DiscordDeafen, WindowsVKey.VK_LCONTROL, WindowsVKey.VK_LSHIFT, WindowsVKey.VK_D);
    }

    protected override void OnButtonPressed(Enum key)
    {
        ExecuteKeyCombination(key);
    }

    private enum DiscordInputParameter
    {
        DiscordMic,
        DiscordDeafen
    }
}
