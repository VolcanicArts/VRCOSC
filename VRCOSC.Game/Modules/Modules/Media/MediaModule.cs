// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using VRCOSC.Game.Modules.Util;

namespace VRCOSC.Game.Modules.Modules.Media;

public sealed class MediaModule : IntegrationModule
{
    public override string Title => "Media";
    public override string Description => "Integration with Windows OS Media";
    public override string Author => "VolcanicArts";
    public override string Prefab => "VRCOSC-Media";
    public override ModuleType ModuleType => ModuleType.Integrations;
    protected override string ReturnProcess => string.Empty;

    protected override void CreateAttributes()
    {
        RegisterButtonInput(MediaIncomingParameter.MediaPlayPause);
        RegisterButtonInput(MediaIncomingParameter.MediaNext);
        RegisterButtonInput(MediaIncomingParameter.MediaPrevious);

        RegisterKeyCombination(MediaIncomingParameter.MediaPlayPause, WindowsVKey.VK_MEDIA_PLAY_PAUSE);
        RegisterKeyCombination(MediaIncomingParameter.MediaNext, WindowsVKey.VK_MEDIA_NEXT_TRACK);
        RegisterKeyCombination(MediaIncomingParameter.MediaPrevious, WindowsVKey.VK_MEDIA_PREV_TRACK);
    }

    protected override void OnButtonPressed(Enum key)
    {
        ExecuteKeyCombination(key);
    }

    private enum MediaIncomingParameter
    {
        MediaPlayPause,
        MediaNext,
        MediaPrevious
    }
}
