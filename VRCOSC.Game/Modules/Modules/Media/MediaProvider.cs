// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Control;
using VRCOSC.Game.Util;

namespace VRCOSC.Game.Modules.Modules.Media;

public class MediaProvider
{
    private MediaManager? mediaManager;
    private Process? trackedProcess;
    private string? lastSender;

    private int trackedProcessId => trackedProcess?.Id ?? -1;

    public MediaState State { get; private set; } = null!;

    public GlobalSystemMediaTransportControlsSession? Controller
        => mediaManager?.CurrentMediaSessions.ContainsKey(lastSender ?? string.Empty) ?? false ? mediaManager.CurrentMediaSessions[lastSender!].ControlSession : null;

    public IEnumerable<string> ProcessExclusions = Array.Empty<string>();

    public Action? OnMediaSessionOpened;
    public Action? OnMediaUpdate;

    public async Task StartMediaHook()
    {
        State = new MediaState();

        mediaManager = new MediaManager();
        mediaManager.OnAnySessionOpened += MediaManager_OnAnySessionOpened;
        mediaManager.OnAnySessionClosed += MediaManager_OnAnySessionClosed;
        mediaManager.OnAnyPlaybackStateChanged += MediaManager_OnAnyPlaybackStateChanged;
        mediaManager.OnAnyMediaPropertyChanged += MediaManager_OnAnyMediaPropertyChanged;
        await mediaManager.Start();
    }

    public void StopMediaHook()
    {
        mediaManager?.Dispose();
        mediaManager = null;

        lastSender = null;
        trackedProcess = null;
    }

    private void MediaManager_OnAnySessionOpened(MediaManager.MediaSession sender)
    {
        if (!updateTrackedProcess(sender)) return;

        State.Position = sender.ControlSession?.GetTimelineProperties() ?? null;

        OnMediaSessionOpened?.Invoke();
    }

    private void MediaManager_OnAnySessionClosed(MediaManager.MediaSession sender)
    {
        lastSender = null;
        trackedProcess = null;
    }

    private void MediaManager_OnAnyPlaybackStateChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionPlaybackInfo args)
    {
        if (!updateTrackedProcess(sender)) return;

        State.IsShuffle = args.IsShuffleActive ?? false;
        State.RepeatMode = args.AutoRepeatMode ?? 0;
        State.Status = args.PlaybackStatus;

        OnMediaUpdate?.Invoke();
    }

    private void MediaManager_OnAnyMediaPropertyChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionMediaProperties args)
    {
        if (!updateTrackedProcess(sender)) return;

        var playbackInfo = sender.ControlSession?.GetPlaybackInfo();
        if (playbackInfo is null) return;

        State.IsShuffle = playbackInfo.IsShuffleActive ?? false;
        State.RepeatMode = playbackInfo.AutoRepeatMode ?? 0;
        State.Status = playbackInfo.PlaybackStatus;
        State.Title = args.Title;
        State.Artist = args.Artist;

        OnMediaUpdate?.Invoke();
    }

    private bool updateTrackedProcess(MediaManager.MediaSession sender)
    {
        var senderName = sender.Id.Replace(".exe", string.Empty);

        if (ProcessExclusions.Contains(senderName)) return false;

        if (lastSender != senderName)
        {
            trackedProcess = Process.GetProcessesByName(senderName).FirstOrDefault();
            lastSender = trackedProcess is null ? null : senderName;
        }

        return true;
    }

    public void SetVolume(float percentage) => ProcessExtensions.SetProcessVolume(lastSender ?? string.Empty, percentage);
    public void SetMuted(bool muted) => ProcessExtensions.SetProcessMuted(lastSender ?? string.Empty, muted);

    public float GetVolume() => ProcessExtensions.RetrieveProcessVolume(lastSender ?? string.Empty);
    public bool IsMuted() => ProcessExtensions.IsProcessMuted(lastSender ?? string.Empty);
}

public class MediaState
{
    public string Title = string.Empty;
    public string Artist = string.Empty;
    public MediaPlaybackAutoRepeatMode RepeatMode;
    public bool IsShuffle;
    public GlobalSystemMediaTransportControlsSessionPlaybackStatus Status;
    public GlobalSystemMediaTransportControlsSessionTimelineProperties? Position;
    public bool IsPlaying => Status == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
}
