// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;

namespace VRCOSC.Game.Modules.SDK.Providers.Media;

public abstract class MediaProvider
{
    public Action<string>? OnLog;

    public MediaState State = new();

    public abstract Task<bool> InitialiseAsync();
    public abstract void Update(TimeSpan delta);
    public abstract Task TerminateAsync();

    public abstract void Play();
    public abstract void Pause();
    public abstract void SkipNext();
    public abstract void SkipPrevious();
    public abstract void ChangeShuffle(bool active);
    public abstract void ChangePlaybackPosition(TimeSpan playbackPosition);
    public abstract void ChangeRepeatMode(MediaRepeatMode mode);
    public abstract void TryChangeVolume(float percentage);
    public abstract float TryGetVolume();

    public Action? OnPlaybackStateChange;
    public Action? OnPlaybackPositionChange;
    public Action? OnTrackChange;
}
