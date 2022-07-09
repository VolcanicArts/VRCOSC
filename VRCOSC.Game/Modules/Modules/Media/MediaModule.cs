// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Colour;
using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Modules.Modules.Media;

public class MediaModule : IntegrationModule
{
    public override string Title => "Media";
    public override string Description => "Integration with Windows OS Media";
    public override string Author => "VolcanicArts";
    public override string Prefab => "VRCOSC-Media";
    public override ColourInfo Colour => Color4Extensions.FromHex(@"ffb900");
    public override ModuleType ModuleType => ModuleType.Integrations;
    protected override string ReturnProcess => string.Empty;

    protected override void CreateAttributes()
    {
        RegisterButtonInput(MediaInputParameter.MediaPlayPause);
        RegisterButtonInput(MediaInputParameter.MediaNext);
        RegisterButtonInput(MediaInputParameter.MediaPrevious);

        RegisterKeyCombination(MediaInputParameter.MediaPlayPause, WindowsVKey.VK_MEDIA_PLAY_PAUSE);
        RegisterKeyCombination(MediaInputParameter.MediaNext, WindowsVKey.VK_MEDIA_NEXT_TRACK);
        RegisterKeyCombination(MediaInputParameter.MediaPrevious, WindowsVKey.VK_MEDIA_PREV_TRACK);
    }

    protected override void OnButtonPressed(Enum key)
    {
        ExecuteKeyCombination(key);
    }

    private enum MediaInputParameter
    {
        MediaPlayPause,
        MediaNext,
        MediaPrevious
    }
}
