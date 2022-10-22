// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Control;
using WindowsMediaController;

namespace VRCOSC.Game.Modules;

public abstract class MediaIntegrationModule : Module
{
    private MediaManager mediaManager = null!;
    private CancellationTokenSource tokenSource = null!;

    protected MediaState MediaState = new();
    protected GlobalSystemMediaTransportControlsSession MediaController => mediaManager.CurrentMediaSessions[lastSender].ControlSession;

    private string lastSender = string.Empty;
    private Process? trackedProcess;

    private int processId => trackedProcess?.Id ?? 0;

    protected void StartMediaHook()
    {
        mediaManager = new MediaManager();
        mediaManager.OnAnyPlaybackStateChanged += MediaManager_OnAnyPlaybackStateChanged;
        mediaManager.OnAnyMediaPropertyChanged += MediaManager_OnAnyMediaPropertyChanged;

        tokenSource = new CancellationTokenSource();

        Task.Run(() =>
        {
            mediaManager.Start();
            tokenSource.Token.WaitHandle.WaitOne();
        });
    }

    protected void StopMediaHook()
    {
        tokenSource.Cancel();
        mediaManager.Dispose();
    }

    protected virtual void OnMediaUpdate() { }

    private void MediaManager_OnAnyPlaybackStateChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionPlaybackInfo args)
    {
        updateTrackedProcess(sender);

        MediaState.IsShuffle = args.IsShuffleActive!.Value;
        MediaState.RepeatMode = args.AutoRepeatMode!.Value;
        MediaState.Status = args.PlaybackStatus;
        MediaState.Position = sender.ControlSession.GetTimelineProperties();

        OnMediaUpdate();
    }

    private void MediaManager_OnAnyMediaPropertyChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionMediaProperties args)
    {
        updateTrackedProcess(sender);

        var playbackInfo = sender.ControlSession.GetPlaybackInfo();
        MediaState.IsShuffle = playbackInfo.IsShuffleActive!.Value;
        MediaState.RepeatMode = playbackInfo.AutoRepeatMode!.Value;
        MediaState.Status = playbackInfo.PlaybackStatus;
        MediaState.Title = args.Title;
        MediaState.Artist = args.Artist;
        MediaState.Position = sender.ControlSession.GetTimelineProperties();

        OnMediaUpdate();
    }

    private void updateTrackedProcess(MediaManager.MediaSession sender)
    {
        if (lastSender != sender.Id)
        {
            trackedProcess = Process.GetProcessesByName(sender.Id.Replace(".exe", string.Empty)).FirstOrDefault()!;
            lastSender = sender.Id;
        }
    }

    protected void SetVolume(float percentage)
    {
        ProcessVolume.SetApplicationVolume(processId, percentage);
    }

    protected float GetVolume()
    {
        return ProcessVolume.GetApplicationVolume(processId)!.Value;
    }

    protected void SetMuted(bool muted)
    {
        Console.WriteLine(muted);
        ProcessVolume.SetApplicationMute(processId, muted);
    }

    protected bool IsMuted()
    {
        return ProcessVolume.GetApplicationMute(processId)!.Value;
    }
}

public class MediaState
{
    public string? Title;
    public string? Artist;
    public MediaPlaybackAutoRepeatMode RepeatMode;
    public bool IsShuffle;
    public GlobalSystemMediaTransportControlsSessionPlaybackStatus Status;
    public GlobalSystemMediaTransportControlsSessionTimelineProperties Position;
    public bool IsPlaying => Status == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
}
