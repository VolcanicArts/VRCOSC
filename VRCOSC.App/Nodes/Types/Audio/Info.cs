// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using SoundFlow.Enums;
using SoundFlow.Interfaces;

namespace VRCOSC.App.Nodes.Types.Audio;

[Node("Player State", "Audio/Info")]
public sealed class AudioPlayerStateNode : UpdateNode<PlaybackState>
{
    public ValueInput<ISoundPlayer?> Player = new();

    public ValueOutput<PlaybackState> State = new();

    protected override Task Process(PulseContext c)
    {
        var player = Player.Read(c);
        if (player is null) return Task.CompletedTask;

        State.Write(player.State, c);
        return Task.CompletedTask;
    }

    protected override Task<PlaybackState> GetValue(PulseContext c)
    {
        var player = Player.Read(c);
        if (player is null) return Task.FromResult(PlaybackState.Stopped);

        return Task.FromResult(player.State);
    }
}

[Node("Player Settings", "Audio/Info")]
public sealed class AudioPlayerSettingsNode : UpdateNode<float, float, bool>
{
    public ValueInput<ISoundPlayer?> Player = new();

    public ValueOutput<float> Volume = new();
    public ValueOutput<float> Speed = new();
    public ValueOutput<bool> IsLooping = new();

    protected override Task Process(PulseContext c)
    {
        var player = Player.Read(c);
        if (player is null) return Task.CompletedTask;

        Volume.Write(player.Volume, c);
        Speed.Write(player.PlaybackSpeed, c);
        IsLooping.Write(player.IsLooping, c);
        return Task.CompletedTask;
    }

    protected override Task<(float, float, bool)> GetValues(PulseContext c)
    {
        var player = Player.Read(c);
        if (player is null) return Task.FromResult((0f, 0f, false));

        return Task.FromResult((player.Volume, player.PlaybackSpeed, player.IsLooping));
    }
}

[Node("Player Time", "Audio/Info")]
public sealed class AudioPlayerTimeNode : UpdateNode<float, float, float>
{
    public ValueInput<ISoundPlayer?> Player = new();

    public ValueOutput<TimeSpan> Duration = new();
    public ValueOutput<TimeSpan> Current = new();
    public ValueOutput<float> Progress = new();

    protected override Task Process(PulseContext c)
    {
        var player = Player.Read(c);
        if (player is null) return Task.CompletedTask;

        var progress = player.Duration == 0f ? 0f : player.Time / player.Duration;

        Duration.Write(TimeSpan.FromSeconds(player.Duration), c);
        Current.Write(TimeSpan.FromSeconds(player.Time), c);
        Progress.Write(progress, c);

        return Task.CompletedTask;
    }

    protected override Task<(float, float, float)> GetValues(PulseContext c)
    {
        var player = Player.Read(c);
        if (player is null) return Task.FromResult((0f, 0f, 0f));

        var progress = player.Duration == 0f ? 0f : player.Time / player.Duration;

        return Task.FromResult((player.Duration, player.Time, progress));
    }
}