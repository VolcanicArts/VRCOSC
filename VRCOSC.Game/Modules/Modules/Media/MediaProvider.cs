// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using Windows.Media;
using Windows.Media.Control;
using VRCOSC.Game.Processes;

namespace VRCOSC.Game.Modules.Modules.Media;

public class MediaProvider
{
    private readonly WindowsMediaInterface mediaInterface;

    public MediaState State { get; private set; } = null!;

    public GlobalSystemMediaTransportControlsSession? Controller => mediaInterface.CurrentSession;

    public Action? OnPlaybackStateUpdate;

    public MediaProvider()
    {
        mediaInterface = new WindowsMediaInterface();
        mediaInterface.OnAnyPlaybackInfoChanged += onAnyPlaybackStateChanged;
        mediaInterface.OnAnyMediaPropertiesChanged += onAnyMediaPropertyChanged;
        mediaInterface.OnAnyTimelinePropertiesChanged += onAnyTimelinePropertiesChanged;
        mediaInterface.OnCurrentSessionChanged += onCurrentSessionChanged;
        mediaInterface.Initialise();
    }

    public void StartMediaHook()
    {
        State = new MediaState();
        mediaInterface.Hook();
    }

    public void StopMediaHook()
    {
        mediaInterface.UnHook();
    }

    private bool isFocusedSession(GlobalSystemMediaTransportControlsSession session) => session.SourceAppUserModelId == Controller?.SourceAppUserModelId;

    private void onAnyPlaybackStateChanged(GlobalSystemMediaTransportControlsSession session, GlobalSystemMediaTransportControlsSessionPlaybackInfo args)
    {
        if (!isFocusedSession(session)) return;

        State.IsShuffle = args.IsShuffleActive ?? default;
        State.RepeatMode = args.AutoRepeatMode ?? default;
        State.Status = args.PlaybackStatus;

        OnPlaybackStateUpdate?.Invoke();
    }

    private void onAnyMediaPropertyChanged(GlobalSystemMediaTransportControlsSession session, GlobalSystemMediaTransportControlsSessionMediaProperties args)
    {
        if (!isFocusedSession(session)) return;

        State.Title = args.Title;
        State.Artist = args.Artist;
    }

    private void onAnyTimelinePropertiesChanged(GlobalSystemMediaTransportControlsSession session, GlobalSystemMediaTransportControlsSessionTimelineProperties args)
    {
        if (!isFocusedSession(session)) return;

        State.Position = args;
    }

    private async void onCurrentSessionChanged(GlobalSystemMediaTransportControlsSession? session)
    {
        if (session is null)
        {
            State = new MediaState();
            return;
        }

        State.ProcessId = session.SourceAppUserModelId;

        onAnyPlaybackStateChanged(session, session.GetPlaybackInfo());
        onAnyMediaPropertyChanged(session, await session.TryGetMediaPropertiesAsync());
        onAnyTimelinePropertiesChanged(session, session.GetTimelineProperties());
    }
}

public class MediaState
{
    public string? ProcessId;
    public string Title = string.Empty;
    public string Artist = string.Empty;
    public MediaPlaybackAutoRepeatMode RepeatMode;
    public bool IsShuffle;
    public GlobalSystemMediaTransportControlsSessionPlaybackStatus Status;
    public GlobalSystemMediaTransportControlsSessionTimelineProperties? Position;
    public bool IsPlaying => Status == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;

    public float Volume
    {
        set => ProcessExtensions.SetProcessVolume(ProcessId, value);
        get => ProcessExtensions.RetrieveProcessVolume(ProcessId);
    }

    public bool Muted
    {
        set => ProcessExtensions.SetProcessMuted(ProcessId, value);
        get => ProcessExtensions.IsProcessMuted(ProcessId);
    }
}
