// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using VRCOSC.Game.Modules.Frameworks;

namespace VRCOSC.Game.Modules.Modules.Media;

public class MediaModule : IntegrationModule
{
    public override string Title => "Media";
    public override string Description => "Integration with Windows OS Media";
    public override string Author => "VolcanicArts";
    public override Colour4 Colour => Color4Extensions.FromHex(@"ffb900").Darken(0.25f);
    public override ModuleType Type => ModuleType.Integrations;
    public override string ReturnProcess => string.Empty;

    public override void CreateAttributes()
    {
        RegisterInputParameter(MediaInputParameters.MediaPlayPause, typeof(bool));
        RegisterInputParameter(MediaInputParameters.MediaNext, typeof(bool));
        RegisterInputParameter(MediaInputParameters.MediaPrevious, typeof(bool));

        RegisterKeyCombination(MediaInputParameters.MediaPlayPause, WindowsVKey.VK_MEDIA_PLAY_PAUSE);
        RegisterKeyCombination(MediaInputParameters.MediaNext, WindowsVKey.VK_MEDIA_NEXT_TRACK);
        RegisterKeyCombination(MediaInputParameters.MediaPrevious, WindowsVKey.VK_MEDIA_PREV_TRACK);
    }

    protected override void OnBoolParameterReceived(Enum key, bool value)
    {
        if (!value) return;

        Terminal.Log($"Received input of {key}");

        ExecuteShortcut(key);
    }
}

public enum MediaInputParameters
{
    MediaPlayPause,
    MediaNext,
    MediaPrevious
}
