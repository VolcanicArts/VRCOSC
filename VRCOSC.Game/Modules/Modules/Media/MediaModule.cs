// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using Windows.Media;

namespace VRCOSC.Game.Modules.Modules.Media;

public sealed class MediaModule : MediaIntegrationModule
{
    public override string Title => "Media";
    public override string Description => "Integration with Windows Media";
    public override string Author => "VolcanicArts";
    public override string Prefab => "VRCOSC-Media";
    protected override int DeltaUpdate => 2000;
    protected override bool ExecuteUpdateImmediately => false;
    public override ModuleType ModuleType => ModuleType.Integrations;

    protected override void CreateAttributes()
    {
        CreateSetting(MediaSetting.Display, "Display", "If the song's details should be displayed in VRChat's ChatBox", true);
        CreateSetting(MediaSetting.TitleFormat, "Title Format", "How displaying the title should be formatted.\nAvailable values: %title%, %artist%, %curtime%, %duration%.", "[%curtime%/%duration%]                            Now Playing: %artist% - %title%");
        CreateSetting(MediaSetting.ContinuousShow, "Continuous Show", "Should the ChatBox always be showing the song's details? If you want to show the current time, this should be on", true);
        CreateSetting(MediaSetting.DisplayPeriod, "Display Period", "How long should the song's details display for when overwriting the ChatBox (Milliseconds). This is only applicable when Continuous Show is off", 5000);

        CreateOutgoingParameter(MediaOutgoingParameter.Repeat, "Repeat Mode", "The repeat mode of the current controller", "/avatar/parameters/VRCOSC/Media/Repeat");
        CreateOutgoingParameter(MediaOutgoingParameter.Shuffle, "Shuffle", "Whether shuffle is enabled in the current controller", "/avatar/parameters/VRCOSC/Media/Shuffle");
        CreateOutgoingParameter(MediaOutgoingParameter.Play, "Play", "Whether the song is currently playing or not", "/avatar/parameters/VRCOSC/Media/Play");
        CreateOutgoingParameter(MediaOutgoingParameter.Volume, "Volume", "The volume of the process that is controlling the media", "/avatar/parameters/VRCOSC/Media/Volume");
        CreateOutgoingParameter(MediaOutgoingParameter.Muted, "Mute", "Whether the volume of the process that is controlling the media is muted", "/avatar/parameters/VRCOSC/Media/Muted");

        RegisterButtonInput(MediaIncomingParameter.Next, "VRCOSC/Media/Next");
        RegisterButtonInput(MediaIncomingParameter.Previous, "VRCOSC/Media/Previous");
        RegisterIncomingParameter<float>(MediaIncomingParameter.Volume, "VRCOSC/Media/Volume");
        RegisterIncomingParameter<bool>(MediaIncomingParameter.Play, "VRCOSC/Media/Play");
        RegisterIncomingParameter<int>(MediaIncomingParameter.Repeat, "VRCOSC/Media/Repeat");
        RegisterIncomingParameter<bool>(MediaIncomingParameter.Shuffle, "VRCOSC/Media/Shuffle");
        RegisterIncomingParameter<bool>(MediaIncomingParameter.Muted, "VRCOSC/Media/Muted");
    }

    protected override void OnStart()
    {
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

    protected override void OnUpdate()
    {
        if (GetSetting<bool>(MediaSetting.ContinuousShow))
        {
            MediaState.Position = MediaController.GetTimelineProperties();
            execute();
        }
    }

    protected override void OnFloatParameterReceived(Enum key, float value)
    {
        switch (key)
        {
            case MediaIncomingParameter.Volume:
                SetVolume(value);
                break;
        }
    }

    protected override void OnBoolParameterReceived(Enum key, bool value)
    {
        switch (key)
        {
            case MediaIncomingParameter.Play:
                if (value)
                {
                    MediaController.TryPlayAsync();
                    display();
                }
                else
                    MediaController.TryPauseAsync();

                break;

            case MediaIncomingParameter.Shuffle:
                MediaController.TryChangeShuffleActiveAsync(value);
                break;

            case MediaIncomingParameter.Muted:
                SetMuted(value);
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
        SendParameter(MediaOutgoingParameter.Volume, GetVolume());
        SendParameter(MediaOutgoingParameter.Muted, IsMuted());
    }

    private void display()
    {
        if (string.IsNullOrEmpty(MediaState.Title) || !GetSetting<bool>(MediaSetting.Display)) return;

        var formattedText = GetSetting<string>(MediaSetting.TitleFormat)
                            .Replace("%title%", MediaState.Title)
                            .Replace("%artist%", MediaState.Artist)
                            .Replace("%curtime%", MediaState.Position.Position.ToString(@"mm\:ss"))
                            .Replace("%duration%", MediaState.Position.EndTime.ToString(@"mm\:ss"));

        ChatBox.SetText(formattedText, true, ChatBoxPriority.Override, GetSetting<int>(MediaSetting.DisplayPeriod));
    }

    private enum MediaSetting
    {
        Display,
        TitleFormat,
        DisplayPeriod,
        ContinuousShow
    }

    private enum MediaIncomingParameter
    {
        Play,
        Next,
        Previous,
        Shuffle,
        Repeat,
        Volume,
        Muted,
    }

    private enum MediaOutgoingParameter
    {
        Play,
        Shuffle,
        Repeat,
        Volume,
        Muted
    }
}
