// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System;
using System.Threading.Tasks;
using SoundFlow.Enums;
using SoundFlow.Interfaces;

namespace VRCOSC.App.Nodes.Types.Audio;

[Node("Player State", "Audio/Info")]
public sealed class AudioPlayerStateNode() : ValueSourceNode<PlaybackState>("State")
{
    public ValueInput<ISoundPlayer?> Player = new();

    protected override PlaybackState ComputeValue(PulseContext c) => Player.Read(c)?.State ?? PlaybackState.Stopped;
}

[Node("Player Settings", "Audio/Info")]
public sealed class AudioPlayerSettingsNode : Node, IContinuousNode
{
    public int UpdateOffset => 0;

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
}

[Node("Player Time", "Audio/Info")]
public sealed class AudioPlayerTimeNode : Node, IContinuousNode
{
    public int UpdateOffset => 0;

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
}