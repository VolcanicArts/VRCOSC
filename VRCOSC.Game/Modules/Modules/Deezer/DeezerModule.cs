// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using VRCOSC.Game.Modules.Frameworks;

namespace VRCOSC.Game.Modules.Modules.Spotify;

public class DeezerModule : IntegrationModule
{
    public override string Title => "Deezer";
    public override string Description => "Integration with the Deezer desktop app";
    public override string Author => "VolcanicArts";
    public override Colour4 Colour => Color4Extensions.FromHex(@"14141B");
    public override ModuleType Type => ModuleType.Integrations;

    public override IReadOnlyCollection<Enum> InputParameters => new List<Enum>
    {
        DeezerInputParameters.DeezerPlayPause,
        DeezerInputParameters.DeezerNext,
        DeezerInputParameters.DeezerPrevious,
        DeezerInputParameters.DeezerVolumeUp,
        DeezerInputParameters.DeezerVolumeDown
    };

    protected override string TargetProcess => "deezer";

    protected override IReadOnlyDictionary<Enum, WindowsVKey[]> KeyCombinations => new Dictionary<Enum, WindowsVKey[]>()
    {
        { DeezerInputParameters.DeezerPlayPause, new[] { WindowsVKey.VK_SPACE } },
        { DeezerInputParameters.DeezerNext, new[] { WindowsVKey.VK_LSHIFT, WindowsVKey.VK_RIGHT } },
        { DeezerInputParameters.DeezerPrevious, new[] { WindowsVKey.VK_LSHIFT, WindowsVKey.VK_LEFT } },
        { DeezerInputParameters.DeezerVolumeUp, new[] { WindowsVKey.VK_LSHIFT, WindowsVKey.VK_UP } },
        { DeezerInputParameters.DeezerVolumeDown, new[] { WindowsVKey.VK_LSHIFT, WindowsVKey.VK_DOWN } }
    };

    protected override void OnParameterReceived(Enum key, object value)
    {
        var buttonPressed = (bool)value;
        if (!buttonPressed) return;

        Terminal.Log($"Received input of {key}");

        ExecuteTask(key);
    }
}

public enum DeezerInputParameters
{
    DeezerPlayPause,
    DeezerNext,
    DeezerPrevious,
    DeezerVolumeUp,
    DeezerVolumeDown
}
