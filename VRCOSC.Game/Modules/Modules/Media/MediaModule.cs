// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Media;

namespace VRCOSC.Game.Modules.Modules.Media;

public sealed class MediaModule : MediaIntegrationModule
{
    public override string Title => "Media";
    public override string Description => "Integration with Windows Media";
    public override string Author => "VolcanicArts";
    public override string Prefab => "VRCOSC-Media";
    protected override int DeltaUpdate => 4000;
    protected override bool ExecuteUpdateImmediately => false;
    public override ModuleType ModuleType => ModuleType.Integrations;

    private bool doNotDisplay;

    protected override void CreateAttributes()
    {
        CreateSetting(MediaSetting.Display, "Display", "If the song's details should be displayed in VRChat's ChatBox", true);
        CreateSetting(MediaSetting.ChatBoxFormat, "ChatBox Format", "How displaying the song's details should be formatted for the ChatBox.\nAvailable values: %title%, %artist%, %curtime%, %duration%.",
            "[%curtime%/%duration%]                            Now Playing: %artist% - %title%");
        CreateSetting(MediaSetting.ContinuousShow, "Continuous Show", "Should the ChatBox always be showing the song's details? If you want to show the current time, this should be on", true);
        CreateSetting(MediaSetting.LaunchList, "Launch List", "What programs to launch on module start", new[] { $@"C:\Users\{Environment.UserName}\AppData\Roaming\Spotify\spotify.exe" });
        CreateSetting(MediaSetting.DisplayPeriod, "Display Period", "How long should the song's details display for when overwriting the ChatBox (Milliseconds). This is only applicable when Continuous Show is off", 5000);

        CreateOutgoingParameter(MediaOutgoingParameter.Repeat, "Repeat Mode", "The repeat mode of the current controller", "/avatar/parameters/VRCOSC/Media/Repeat");
        CreateOutgoingParameter(MediaOutgoingParameter.Shuffle, "Shuffle", "Whether shuffle is enabled in the current controller", "/avatar/parameters/VRCOSC/Media/Shuffle");
        CreateOutgoingParameter(MediaOutgoingParameter.Play, "Play", "Whether the song is currently playing or not", "/avatar/parameters/VRCOSC/Media/Play");
        CreateOutgoingParameter(MediaOutgoingParameter.Volume, "Volume", "The volume of the process that is controlling the media", "/avatar/parameters/VRCOSC/Media/Volume");
        CreateOutgoingParameter(MediaOutgoingParameter.Muted, "Mute", "Whether the volume of the process that is controlling the media is muted", "/avatar/parameters/VRCOSC/Media/Muted");

        RegisterButtonInput(MediaIncomingParameter.Next, "VRCOSC/Media/Next");
        RegisterButtonInput(MediaIncomingParameter.Previous, "VRCOSC/Media/Previous");
        RegisterRadialInput(MediaIncomingParameter.Volume, "VRCOSC/Media/Volume");
        RegisterIncomingParameter<bool>(MediaIncomingParameter.Play, "VRCOSC/Media/Play");
        RegisterIncomingParameter<int>(MediaIncomingParameter.Repeat, "VRCOSC/Media/Repeat");
        RegisterIncomingParameter<bool>(MediaIncomingParameter.Shuffle, "VRCOSC/Media/Shuffle");
        RegisterIncomingParameter<bool>(MediaIncomingParameter.Muted, "VRCOSC/Media/Muted");
    }

    protected override void OnStart()
    {
        StartMediaHook();
        GetSetting<List<string>>(MediaSetting.LaunchList).ForEach(program => Process.Start(program));
        doNotDisplay = false;
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
            if (MediaController is not null)
                MediaState.Position = MediaController.GetTimelineProperties();

            execute();
        }
    }

    protected override void OnRadialPuppetChange(Enum key, VRChatRadialPuppet radialData)
    {
        switch (key)
        {
            case MediaIncomingParameter.Volume:
                SetVolume(radialData.Value);
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
                    MediaController?.TryPlayAsync();
                    display();
                }
                else
                    MediaController?.TryPauseAsync();

                break;

            case MediaIncomingParameter.Shuffle:
                MediaController?.TryChangeShuffleActiveAsync(value);
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
                MediaController?.TryChangeAutoRepeatModeAsync((MediaPlaybackAutoRepeatMode)value);
                break;
        }
    }

    protected override void OnButtonPressed(Enum key)
    {
        switch (key)
        {
            case MediaIncomingParameter.Next:
                MediaController?.TrySkipNextAsync();
                break;

            case MediaIncomingParameter.Previous:
                MediaController?.TrySkipPreviousAsync();
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
        if (doNotDisplay)
        {
            if (MediaState.IsPlaying)
            {
                doNotDisplay = false;
                display();
            }
        }
        else
        {
            if (!MediaState.IsPlaying)
            {
                if (GetSetting<bool>(MediaSetting.ContinuousShow)) ChatBox.Clear();
                doNotDisplay = true;
            }
            else
            {
                if (string.IsNullOrEmpty(MediaState.Title) || !GetSetting<bool>(MediaSetting.Display)) return;

                var formattedText = GetSetting<string>(MediaSetting.ChatBoxFormat)
                                    .Replace("%title%", MediaState.Title)
                                    .Replace("%artist%", MediaState.Artist)
                                    .Replace("%curtime%", MediaState.Position.Position.ToString(@"mm\:ss"))
                                    .Replace("%duration%", MediaState.Position.EndTime.ToString(@"mm\:ss"));

                ChatBox.SetText(formattedText, true, ChatBoxPriority.Override, GetSetting<int>(MediaSetting.DisplayPeriod));
            }
        }
    }

    private enum MediaSetting
    {
        Display,
        ChatBoxFormat,
        DisplayPeriod,
        ContinuousShow,
        LaunchList
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
