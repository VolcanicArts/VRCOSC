// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

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

    protected void StartMediaHook()
    {
        mediaManager = new MediaManager();
        mediaManager.OnAnyPlaybackStateChanged += MediaManager_OnAnyPlaybackStateChanged;
        mediaManager.OnAnyMediaPropertyChanged += MediaManager_OnAnyMediaPropertyChanged;

        tokenSource = new CancellationTokenSource();

        Task.Run(() =>
        {
            mediaManager.Start();
            Task.Delay(-1);
        }, tokenSource.Token);
    }

    protected void StopMediaHook()
    {
        tokenSource.Cancel();
        mediaManager.Dispose();
    }

    protected virtual void OnMediaUpdate() { }

    private void MediaManager_OnAnyPlaybackStateChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionPlaybackInfo args)
    {
        lastSender = sender.Id;
        MediaState.IsShuffle = args.IsShuffleActive!.Value;
        MediaState.RepeatMode = args.AutoRepeatMode!.Value;
        MediaState.Status = args.PlaybackStatus;
        MediaState.Position = sender.ControlSession.GetTimelineProperties();

        OnMediaUpdate();
    }

    private void MediaManager_OnAnyMediaPropertyChanged(MediaManager.MediaSession sender, GlobalSystemMediaTransportControlsSessionMediaProperties args)
    {
        lastSender = sender.Id;
        var playbackInfo = sender.ControlSession.GetPlaybackInfo();
        MediaState.IsShuffle = playbackInfo.IsShuffleActive!.Value;
        MediaState.RepeatMode = playbackInfo.AutoRepeatMode!.Value;
        MediaState.Status = playbackInfo.PlaybackStatus;
        MediaState.Title = args.Title;
        MediaState.Artist = args.Artist;
        MediaState.Position = sender.ControlSession.GetTimelineProperties();

        OnMediaUpdate();
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
}
