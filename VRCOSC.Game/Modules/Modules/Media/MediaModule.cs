// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using Windows.Media;

namespace VRCOSC.Game.Modules.Modules.Media;

public sealed class MediaModule : MediaIntegrationModule
{
    public override string Title => "Media";
    public override string Description => "Integration with Windows OS Media";
    public override string Author => "VolcanicArts";
    public override string Prefab => "VRCOSC-Media";
    public override ModuleType ModuleType => ModuleType.Integrations;

    private string currentTitle = string.Empty;

    protected override void CreateAttributes()
    {
        CreateSetting(MediaSetting.DisplayTitle, "Display Title", "If the title of the next track should be displayed in VRChat's ChatBox", false);
        CreateSetting(MediaSetting.TitleFormat, "Title Format", "How displaying the title should be formatted.\nAvailable values: %title%, %artist%.", "Now Playing: %artist% - %title%");
        CreateSetting(MediaSetting.DisplayTime, "Display Time", "How long should the title display for when overwriting the ChatBox (Milliseconds)", 5000);

        CreateOutgoingParameter(MediaOutgoingParameter.Repeat, "Repeat Mode", "The repeat mode of the current controller", "/avatar/parameters/VRCOSC/Media/Repeat");
        CreateOutgoingParameter(MediaOutgoingParameter.Shuffle, "Shuffle", "Whether shuffle is enabled in the current controller", "/avatar/parameters/VRCOSC/Media/Shuffle");
        CreateOutgoingParameter(MediaOutgoingParameter.Play, "Play", "Whether the song is currently playing or not", "/avatar/parameters/VRCOSC/Media/Play");

        RegisterButtonInput(MediaIncomingParameter.Next, "VRCOSC/Media/Next");
        RegisterButtonInput(MediaIncomingParameter.Previous, "VRCOSC/Media/Previous");
        RegisterIncomingParameter<bool>(MediaIncomingParameter.Play, "VRCOSC/Media/Play");
        RegisterIncomingParameter<int>(MediaIncomingParameter.Repeat, "VRCOSC/Media/Repeat");
        RegisterIncomingParameter<bool>(MediaIncomingParameter.Shuffle, "VRCOSC/Media/Shuffle");
    }

    protected override void OnStart()
    {
        currentTitle = string.Empty;
        StartMediaHook();
    }

    protected override void OnStop()
    {
        StopMediaHook();
    }

    protected override void OnAvatarChange()
    {
        execute();
    }

    protected override void OnBoolParameterReceived(Enum key, bool value)
    {
        switch (key)
        {
            case MediaIncomingParameter.Play:
                if (value)
                    MediaController.TryPlayAsync();
                else
                    MediaController.TryPauseAsync();
                break;

            case MediaIncomingParameter.Shuffle:
                MediaController.TryChangeShuffleActiveAsync(value);
                break;
        }
    }

    protected override void OnIntParameterReceived(Enum key, int value)
    {
        switch (key)
        {
            case MediaIncomingParameter.Repeat:
                MediaController.TryChangeAutoRepeatModeAsync((MediaPlaybackAutoRepeatMode)value);
                break;
        }
    }

    protected override void OnButtonPressed(Enum key)
    {
        switch (key)
        {
            case MediaIncomingParameter.Next:
                MediaController.TrySkipNextAsync();
                break;

            case MediaIncomingParameter.Previous:
                MediaController.TrySkipPreviousAsync();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(key), key, null);
        }
    }

    protected override void OnMediaUpdate()
    {
        execute();
    }

    private void execute()
    {
        setParameters();
        display();
    }

    private void setParameters()
    {
        SendParameter(MediaOutgoingParameter.Play, MediaState.IsPlaying);
        SendParameter(MediaOutgoingParameter.Shuffle, MediaState.IsShuffle);
        SendParameter(MediaOutgoingParameter.Repeat, (int)MediaState.RepeatMode);
    }

    private void display()
    {
        if (string.IsNullOrEmpty(MediaState.Title) || currentTitle == MediaState.Title || !MediaState.IsPlaying) return;

        currentTitle = MediaState.Title;

        var formattedText = GetSetting<string>(MediaSetting.TitleFormat)
                            .Replace("%title%", MediaState.Title)
                            .Replace("%artist%", MediaState.Artist);

        ChatBox.SetText(formattedText, true, ChatBoxPriority.Override, GetSetting<int>(MediaSetting.DisplayTime));
    }

    private enum MediaSetting
    {
        DisplayTitle,
        TitleFormat,
        DisplayTime
    }

    private enum MediaIncomingParameter
    {
        Play,
        Next,
        Previous,
        Shuffle,
        Repeat
    }

    private enum MediaOutgoingParameter
    {
        Play,
        Shuffle,
        Repeat
    }
}
