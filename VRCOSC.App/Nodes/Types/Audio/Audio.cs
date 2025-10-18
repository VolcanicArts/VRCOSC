// Copyright (c) VolcanicArts. Licensed under the GPL-3.0 License.
// See the LICENSE file in the repository root for full license text.

using System.IO;
using System.Threading.Tasks;
using SoundFlow.Components;
using SoundFlow.Providers;

namespace VRCOSC.App.Nodes.Types.Audio;

[Node("Create Track", "Audio")]
public sealed class AudioTrackCreateNode : Node
{
    public ValueInput<string> FilePath = new("File Path");
    public ValueInput<float> Volume = new("Volume", 1f);
    public ValueInput<float> Speed = new("Speed", 1f);

    public ValueOutput<SoundPlayer> Track = new();

    protected override Task Process(PulseContext c)
    {
        var filePath = FilePath.Read(c);

        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return Task.CompletedTask;

        var player = new SoundPlayer(new StreamDataProvider(File.OpenRead(filePath)))
        {
            Volume = Volume.Read(c),
            PlaybackSpeed = Speed.Read(c)
        };

        Mixer.Master.AddComponent(player);

        Track.Write(player, c);
        return Task.CompletedTask;
    }
}

[Node("Play Track", "Audio")]
public sealed class AudioTrackPlayNode : Node, IFlowInput
{
    public FlowContinuation OnFinished = new("On Finished");
    public FlowContinuation OnFailed = new("On Failed");

    public ValueInput<SoundPlayer> Track = new();

    protected override async Task Process(PulseContext c)
    {
        var player = Track.Read(c);

        if (player is null)
        {
            await OnFailed.Execute(c);
            return;
        }

        player.Play();

        while (player.Time < player.Duration)
        {
            await Task.Delay(10);
        }

        player.Stop();

        await OnFinished.Execute(c);
    }
}

[Node("Play Track Async", "Audio")]
public sealed class AudioTrackPlayAsyncNode : Node, IFlowInput
{
    public FlowContinuation OnPlaying = new("On Playing");
    public FlowContinuation OnFailed = new("On Failed");

    public ValueInput<SoundPlayer> Track = new();

    protected override async Task Process(PulseContext c)
    {
        var player = Track.Read(c);

        if (player is null)
        {
            await OnFailed.Execute(c);
            return;
        }

        player.Play();

        await OnPlaying.Execute(c);
    }
}

[Node("Stop Track", "Audio")]
public sealed class AudioTrackStopNode : Node, IFlowInput
{
    public FlowContinuation OnStopped = new("On Stopped");
    public FlowContinuation OnFailed = new("On Failed");

    public ValueInput<SoundPlayer> Track = new();

    protected override async Task Process(PulseContext c)
    {
        var player = Track.Read(c);

        if (player is null)
        {
            await OnFailed.Execute(c);
            return;
        }

        player.Stop();

        await OnStopped.Execute(c);
    }
}