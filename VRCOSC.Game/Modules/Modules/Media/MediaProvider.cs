// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Control;
using VRCOSC.Game.Util;
using WindowsMediaController;

namespace VRCOSC.Game.Modules.Modules.Media;

public class MediaProvider
{
    private MediaManager mediaManager = null!;
    private CancellationTokenSource tokenSource = null!;

    public MediaState State { get; private set; } = new();

    public GlobalSystemMediaTransportControlsSession? Controller
        => mediaManager.CurrentMediaSessions.ContainsKey(lastSender) ? mediaManager.CurrentMediaSessions[lastSender].ControlSession : null;

    public IEnumerable<string> ProcessExclusions = Array.Empty<string>();

    private string lastSender = string.Empty;
    private Process? trackedProcess;

    private int processId => trackedProcess?.Id ?? -1;

    public Action? OnMediaSessionOpened;
    public Action? OnMediaUpdate;

    public void StartMediaHook()
    {
        lastSender = string.Empty;
        trackedProcess = null;
        State = new MediaState();

        mediaManager = new MediaManager();
        mediaManager.OnAnySessionOpened += MediaManager_OnAnySessionOpened;
        mediaManager.OnAnySessionClosed += MediaManager_OnAnySessionClosed;
        mediaManager.OnAnyPlaybackStateChanged += MediaManager_OnAnyPlaybackStateChanged;
        mediaManager.OnAnyMediaPropertyChanged += MediaManager_OnAnyMediaPropertyChanged;

        tokenSource = new CancellationTokenSource();

        Task.Run(() =>
        {
            mediaManager.Start();
            tokenSource.Token.WaitHandle.WaitOne();
        });
    }

    public void StopMediaHook()
    {
        mediaManager.Dispose();
        tokenSource.Cancel();
    }

    private void MediaManager_OnAnySessionOpened(MediaManager.MediaSession sender)
    {
        if (!updateTrackedProcess(sender)) return;

        State.Position = sender.ControlSession.GetTimelineProperties();

        OnMediaSessionOpened?.Invoke();
    }

    private void MediaManager_OnAnySessionClosed(MediaManager.MediaSession sender)
    {
        lastSender = string.Empty;
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

        var playbackInfo = sender.ControlSession.GetPlaybackInfo();
        State.IsShuffle = playbackInfo.IsShuffleActive ?? false;
        State.RepeatMode = playbackInfo.AutoRepeatMode ?? 0;
        State.Status = playbackInfo.PlaybackStatus;
        State.Title = args.Title;
        State.Artist = args.Artist;

        OnMediaUpdate?.Invoke();
    }

    private bool updateTrackedProcess(MediaManager.MediaSession sender)
    {
        if (ProcessExclusions.Contains(sender.Id.Replace(".exe", string.Empty))) return false;

        if (lastSender != sender.Id)
        {
            trackedProcess = Process.GetProcessesByName(sender.Id.Replace(".exe", string.Empty)).FirstOrDefault();
            lastSender = trackedProcess is null ? string.Empty : sender.Id;
        }

        return true;
    }

    public void SetVolume(float percentage)
    {
        if (!ensureProcessExists()) return;

        ProcessExtensions.SetProcessVolume(processId, percentage);
    }

    public float GetVolume()
    {
        return ensureProcessExists() ? ProcessExtensions.RetrieveProcessVolume(processId) : 0f;
    }

    public void SetMuted(bool muted)
    {
        if (!ensureProcessExists()) return;

        ProcessExtensions.SetProcessMuted(processId, muted);
    }

    public bool IsMuted()
    {
        return ensureProcessExists() ? ProcessExtensions.IsProcessMuted(processId) : false;
    }

    private bool ensureProcessExists()
    {
        try
        {
            Process.GetProcessById(processId);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

public class MediaState
{
    public string? Title;
    public string? Artist;
    public MediaPlaybackAutoRepeatMode RepeatMode;
    public bool IsShuffle;
    public GlobalSystemMediaTransportControlsSessionPlaybackStatus Status;
    public GlobalSystemMediaTransportControlsSessionTimelineProperties? Position;
    public bool IsPlaying => Status == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
}
